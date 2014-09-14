namespace Barista.SharePoint.Services
{
    using System;
    using Microsoft.SharePoint.Administration;
    using System.Runtime.InteropServices;

    [Guid("8948B03B-FB70-4B84-897E-A0173C71D819")]
    [SupportedServiceApplication("9B4C0B5C-8A42-401A-9ACB-42EA6246E960",
                                "1.0.0.0",
                                typeof(BaristaServiceApplicationProxy))]
    public class BaristaServiceProxy : SPIisWebServiceProxy, IServiceProxyAdministration
    {
        internal const string ProxyName = "BaristaServiceProxy";

        public BaristaServiceProxy()
        {
        }

        public BaristaServiceProxy(SPFarm farm)
            : base(farm)
        {
            Name = ProxyName;
        }

        public SPServiceApplicationProxy CreateProxy(Type serviceApplicationProxyType, string name, Uri serviceApplicationUri, SPServiceProvisioningContext provisioningContext)
        {
            if (serviceApplicationProxyType != typeof(BaristaServiceApplicationProxy))
                throw new NotSupportedException();

            return new BaristaServiceApplicationProxy(name, this, serviceApplicationUri);
        }

        public SPPersistedTypeDescription GetProxyTypeDescription(Type serviceApplicationProxyType)
        {
            return new SPPersistedTypeDescription("Barista Service Proxy", "Custom service application proxy providing scripted service capabilities.");
        }

        public Type[] GetProxyTypes()
        {
            return new[]
            {
                typeof (BaristaServiceApplicationProxy)
            };
        }
    }
}
