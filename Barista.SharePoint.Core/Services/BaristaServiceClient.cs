namespace Barista.SharePoint.Services
{
    using Microsoft.SharePoint;
    using System;
    using System.Collections.Generic;

    public sealed class BaristaServiceClient : IBaristaServiceApplication
    {
        private readonly SPServiceContext m_serviceContext;

        public BaristaServiceClient(SPServiceContext serviceContext)
        {

            if (serviceContext == null)
                throw new ArgumentNullException("serviceContext");

            m_serviceContext = serviceContext;
        }

        public BrewResponse Eval(BrewRequest request)
        {
            BrewResponse result = null;
            BaristaServiceApplicationProxy.Invoke(
              m_serviceContext,
              proxy => result = proxy.Eval(request)
            );
            return result;
        }

        public void Exec(BrewRequest request)
        {
            BaristaServiceApplicationProxy.Invoke(
              m_serviceContext,
              proxy => proxy.Exec(request)
            );
        }

        public string ListPackages()
        {
            string result = string.Empty;
            BaristaServiceApplicationProxy.Invoke(
                m_serviceContext,
                proxy => result = proxy.ListPackages()
                );

            return result;
        }

        public string AddPackage(byte[] bundlePackage)
        {
            string result = string.Empty;
            BaristaServiceApplicationProxy.Invoke(
                m_serviceContext,
                proxy => result = proxy.AddPackage(bundlePackage)
                );

            return result;
        }
    }
}
