namespace Barista.SharePoint.Extensions
{
    using System;
    using Microsoft.SharePoint;

    public static class SPWebExtensions
    {
        public static bool IsElevated(this SPWeb web)
        {
            if (web == null)
            {
                throw new ArgumentNullException("web");
            }

            if (web.CurrentUser == null)
                return false;

            return string.Equals(web.CurrentUser.LoginName, web.Site.SystemAccount.LoginName,
              StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
