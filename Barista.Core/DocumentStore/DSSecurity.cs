namespace Barista.DocumentStore
{
  using System;
  using System.Security.Principal;

  /// <summary>
  /// Provides static security management properties and methods.
  /// </summary>
  public static class DSSecurity
  {
    /// <summary>
    /// Executes the specified action using the application pool identity.
    /// </summary>
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
