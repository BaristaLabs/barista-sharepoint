namespace Barista.SharePoint.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.ServiceModel.Activation;
  using System.ServiceModel;
  using Microsoft.SharePoint;

  internal sealed class BaristaServiceHostFactory : ServiceHostFactory
  {
    public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
    {
      ServiceHost serviceHost = new ServiceHost(typeof(BaristaServiceApplication), baseAddresses);

      // configure the service for claims
      serviceHost.Configure(SPServiceAuthenticationMode.Claims);

      return serviceHost;
    }
  }
}