namespace Barista.SharePoint.Features.BaristaServiceInstaller
{
    using global::Barista.SharePoint.Services;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Feature Receiver that contains behavior to install instances of the Barista Service, Service Proxy and service instance in the local farm.
    /// </summary>
    [Guid("08c45199-e04f-4033-80bb-16e015f096f8")]
    public class BaristaServiceInstallerEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            // install the service
            var service = BaristaHelper.GetBaristaService(SPFarm.Local);
            if (service == null)
            {
                service = new BaristaService(SPFarm.Local);
                service.Update(true);
            }


            // install the service proxy
            var serviceProxy = BaristaHelper.GetBaristaServiceProxy(SPFarm.Local);
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
            var serviceInstance = BaristaHelper.GetBaristaServiceInstance(SPFarm.Local);
            if (serviceInstance != null)
            {
                serviceInstance.Delete();
                SPServer.Local.ServiceInstances.Remove(serviceInstance.Id);
            }

            // uninstall the service proxy
            var serviceProxy = BaristaHelper.GetBaristaServiceProxy(SPFarm.Local);
            if (serviceProxy != null)
            {
                serviceProxy.Delete();
                SPFarm.Local.ServiceProxies.Remove(serviceProxy.Id);
            }

            // uninstall the service
            var service = BaristaHelper.GetBaristaService(SPFarm.Local);
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
