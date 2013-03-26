namespace Barista.SharePoint.Framework
{
  using Microsoft.Win32;

  using System;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Globalization;
  using System.Runtime.InteropServices;
  using System.Security;
  using System.Security.Permissions;
  using System.Threading;

  public sealed class SecurityContext : IDisposable
  {
    // Fields
    private const string AdvApi = "advapi32.dll";
    private const int ERROR_NO_TOKEN = 0x3f0;
    private const string Kernel = "kernel32.dll";
    private IntPtr m_hPriorToken;
    private bool m_IsAlive;
    private StackTrace m_StackTrace;
    private int m_ThreadId;
    private static DiagnosticSettings s_DiagnosticSettings = DiagnosticSettings.Uninitialized;
    private static object s_DiagnosticsInitializationLock = new object();

    // Methods
    private SecurityContext(IntPtr priorToken)
    {
      this.m_hPriorToken = priorToken;
      this.m_ThreadId = NativeMethods.GetCurrentThreadId();
    }

    [SuppressUnmanagedCodeSecurity]
    public void Dispose()
    {
      if (this.m_IsAlive)
      {
        int currentThreadId = -1;
        try
        {
          currentThreadId = NativeMethods.GetCurrentThreadId();
          if (currentThreadId != this.m_ThreadId)
          {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "SecurityContext must be disposed on original calling thread (ID: {0} CurrentID: {1}).", new object[] { this.m_ThreadId, currentThreadId }));
          }
          this.m_IsAlive = false;
          GC.SuppressFinalize(this);
          if (IntPtr.Zero == this.m_hPriorToken)
          {
            if (!NativeMethods.Win32RevertToSelf())
            {
              throw new Win32Exception(Marshal.GetLastWin32Error());
            }
          }
          else if (!NativeMethods.SetThreadToken(IntPtr.Zero, this.m_hPriorToken))
          {
            throw new Win32Exception(Marshal.GetLastWin32Error());
          }
        }
        catch (Exception exception)
        {
          new SecurityPermission(SecurityPermissionFlag.ControlThread).Assert();
          Thread.CurrentThread.Abort(exception);
        }
        finally
        {
          if (IntPtr.Zero != this.m_hPriorToken)
          {
            IntPtr hPriorToken = this.m_hPriorToken;
            this.m_hPriorToken = IntPtr.Zero;
            NativeMethods.CloseHandle(hPriorToken);
          }
        }
        GC.KeepAlive(this);
      }
    }

    ~SecurityContext()
    {
      if (IntPtr.Zero != this.m_hPriorToken)
      {
        IntPtr hPriorToken = this.m_hPriorToken;
        this.m_hPriorToken = IntPtr.Zero;
        NativeMethods.CloseHandle(hPriorToken);
      }
      if (this.m_IsAlive)
      {
        this.m_IsAlive = false;
      }
    }

    [SuppressUnmanagedCodeSecurity]
    public static SecurityContext Impersonate(IntPtr userToken)
    {
      SecurityContext context = null;
      try
      {
        context = SaveThreadTokenAndRevertToSelf();
        if ((IntPtr.Zero != userToken) && !NativeMethods.ImpersonateLoggedOnUser(userToken))
        {
          throw new Win32Exception(Marshal.GetLastWin32Error());
        }
      }
      catch
      {
        if (context != null)
        {
          context.Dispose();
          context = null;
        }
        throw;
      }
      return context;
    }

    public static SecurityContext Impersonate(IntPtr userToken, bool duplicateToken)
    {
      IntPtr duplicateTokenHandle = userToken;
      if (duplicateToken)
      {
        NativeMethods.DuplicateToken(userToken, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, out duplicateTokenHandle);
      }
      return Impersonate(duplicateTokenHandle);
    }

    private static void InitializeDiagnosticSettings()
    {
      lock (s_DiagnosticsInitializationLock)
      {
        if (s_DiagnosticSettings == DiagnosticSettings.Uninitialized)
        {
          try
          {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Shared Tools\Web Server Extensions\Diagnostics"))
            {
              if ((key != null) && (0 != ((int)key.GetValue("SecurityContextStackTrace", 0))))
              {
                s_DiagnosticSettings |= DiagnosticSettings.CaptureStackTrace;
              }
            }
            if (s_DiagnosticSettings == DiagnosticSettings.Uninitialized)
            {
              s_DiagnosticSettings = DiagnosticSettings.Disabled;
            }
          }
          catch
          {
            s_DiagnosticSettings = DiagnosticSettings.Disabled;
          }
        }
      }
    }

    public static SecurityContext RevertToSelf()
    {
      return SaveThreadTokenAndRevertToSelf();
    }

    public static void RunAsProcess(CodeToRunElevated secureCode)
    {
      if (secureCode == null)
      {
        throw new ArgumentNullException("secureCode");
      }
      try
      {
        using (SaveThreadTokenAndRevertToSelf())
        {
          secureCode();
        }
      }
      catch
      {
        throw;
      }
    }

    [SuppressUnmanagedCodeSecurity]
    private static SecurityContext SaveThreadTokenAndRevertToSelf()
    {
      IntPtr zero = IntPtr.Zero;
      SecurityContext context = null;
      try
      {
        if (NativeMethods.OpenThreadToken(NativeMethods.GetCurrentThread(), TokenAccessRights.TOKEN_IMPERSONATE, true, out zero))
        {
          if (!NativeMethods.Win32RevertToSelf())
          {
            throw new Win32Exception(Marshal.GetLastWin32Error());
          }
        }
        else
        {
          zero = IntPtr.Zero;
          int error = Marshal.GetLastWin32Error();
          if (0x3f0 != error)
          {
            throw new Win32Exception(error);
          }
        }
        context = new SecurityContext(zero);
      }
      catch
      {
        if (IntPtr.Zero != zero)
        {
          NativeMethods.CloseHandle(zero);
          zero = IntPtr.Zero;
        }
        throw;
      }
      context.m_IsAlive = true;
      if (DiagnosticSettings.Disabled != s_DiagnosticSettings)
      {
        if (s_DiagnosticSettings == DiagnosticSettings.Uninitialized)
        {
          InitializeDiagnosticSettings();
        }
        if (DiagnosticSettings.CaptureStackTrace != s_DiagnosticSettings)
        {
          return context;
        }
        try
        {
          context.m_StackTrace = new StackTrace();
        }
        catch
        {
        }
      }
      return context;
    }

    // Nested Types
    public delegate void CodeToRunElevated();

    [Flags]
    private enum DiagnosticSettings
    {
      CaptureStackTrace = 1,
      Disabled = 0x8000,
      Uninitialized = 0
    }

    [SuppressUnmanagedCodeSecurity]
    private static class NativeMethods
    {
      // Methods
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool CloseHandle(IntPtr hObject);
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool DuplicateToken(IntPtr ExistingTokenHandle, SecurityContext.SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, out IntPtr DuplicateTokenHandle);
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern IntPtr GetCurrentThread();
      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern int GetCurrentThreadId();
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool OpenThreadToken(IntPtr ThreadHandle, SecurityContext.TokenAccessRights DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle);
      [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool SetThreadToken(IntPtr Thread, IntPtr Token);
      [DllImport("advapi32.dll", EntryPoint = "RevertToSelf", CharSet = CharSet.Unicode, SetLastError = true)]
      internal static extern bool Win32RevertToSelf();
    }

    private enum SECURITY_IMPERSONATION_LEVEL
    {
      SecurityAnonymous,
      SecurityIdentification,
      SecurityImpersonation,
      SecurityDelegation
    }

    [Flags]
    private enum TokenAccessRights
    {
      TOKEN_ADJUST_DEFAULT = 0x80,
      TOKEN_ADJUST_GROUPS = 0x40,
      TOKEN_ADJUST_PRIVILEGES = 0x20,
      TOKEN_ADJUST_SESSIONID = 0x100,
      TOKEN_ASSIGN_PRIMARY = 1,
      TOKEN_DUPLICATE = 2,
      TOKEN_IMPERSONATE = 4,
      TOKEN_QUERY = 8,
      TOKEN_QUERY_SOURCE = 0x10
    }
  }
}
