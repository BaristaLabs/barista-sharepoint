namespace Barista.SharePoint.Services
{
    using Microsoft.SharePoint.Administration;

    public class BaristaServiceInstance : SPIisWebServiceInstance
    {
        internal const string ServiceInstanceName = "BaristaServiceInstance";

        public BaristaServiceInstance()
        {
        }

        public BaristaServiceInstance(SPServer server, BaristaService service)
            : base(server, service)
        {
            Name = ServiceInstanceName;
        }

        public override string DisplayName
        {
            get { return "Barista Service"; }
        }

        public override string Description
        {
            get { return "Barista Service providing scripted service capabilities."; }
        }

        public override string TypeName
        {
            get { return "Barista Service"; }
        }
    }
}
