namespace Barista.SharePoint
{
    using System;
    using System.Security.Principal;

    public sealed class SPSecurityContext : IDisposable
    {
        WindowsImpersonationContext _ctx;
        public SPSecurityContext()
        {
            UseAppPoolIdentity();
        }

        private void UseAppPoolIdentity()
        {
            try
            {
                if (!WindowsIdentity.GetCurrent().IsSystem)
                {
                    _ctx = WindowsIdentity.Impersonate(IntPtr.Zero);
                }
            }
            catch { }
        }

        private void ReturnToCurrentUser()
        {
            try
            {
                if (_ctx != null)
                {
                    _ctx.Undo();
                }
            }
            catch { }
        }
        public void Dispose()
        {
            ReturnToCurrentUser();
        }
    }
}