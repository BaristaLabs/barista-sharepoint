namespace Barista.SharePoint.Features.BaristaServiceInstaller
{
    using global::Barista.SharePoint.Services;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System.Runtime.InteropServices;

    [Guid("08c45199-e04f-4033-80bb-16e015f096f8")]
    public class BaristaServiceInstallerEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // install the service
            var service = SPFarm.Local.Services.GetValue<BaristaService>();
            if (service == null)
            {
                service = new BaristaService(SPFarm.Local);
                service.Update(true);
            }


            // install the service proxy
            var serviceProxy = SPFarm.Local.ServiceProxies.GetValue<BaristaServiceProxy>();
            if (serviceProxy == null)
            {
                serviceProxy = new BaristaServiceProxy(SPFarm.Local);
                serviceProxy.Update(true);
            }


            // with service added to the farm, install instance
            var serviceInstance = new BaristaServiceInstance(SPServer.Local, service);
            serviceInstance.Update(true);
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // uninstall the instance
            var serviceInstance = SPFarm.Local.Services.GetValue<BaristaServiceInstance>();
            if (serviceInstance != null)
            {
                serviceInstance.Delete();
                SPServer.Local.ServiceInstances.Remove(serviceInstance.Id);
            }

            // uninstall the service proxy
            var serviceProxy = SPFarm.Local.ServiceProxies.GetValue<BaristaServiceProxy>();
            if (serviceProxy != null)
            {
                serviceProxy.Delete();
                SPFarm.Local.ServiceProxies.Remove(serviceProxy.Id);
            }

            // uninstall the service
            var service = SPFarm.Local.Services.GetValue<BaristaService>();
            if (service == null)
                return;

            if (service.Instances.Count != 0)
            {
                foreach (var remainingInstance in service.Instances)
                {
                    SPServer.Local.ServiceInstances.Remove(remainingInstance.Id);
                }
            }
            service.Delete();
            SPFarm.Local.Services.Remove(service.Id);
        }
    }
}
