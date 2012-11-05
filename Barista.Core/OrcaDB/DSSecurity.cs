namespace Barista.OrcaDB
{
  using System;
  using System.Security.Principal;
  using System.Diagnostics;
  using System.Security;
  using System.Runtime.InteropServices;
  using System.ComponentModel;

  /// <summary>
  /// Provides static security management properties and methods.
  /// </summary>
  public static class DSSecurity
  {
    /// <summary>
    /// Executes the specified action using the application pool identity.
    /// </summary>
    /// <param name="action">The action to execute with elevated privledges.</param>
    public static void RunWithElevatedPrivledges(Action codeToRunElevated)
    {
      if (codeToRunElevated == null)
        return;

      using (var wiCtx = WindowsIdentity.Impersonate(IntPtr.Zero))
      {
        codeToRunElevated();
      }
    }
  }
}
