namespace Barista.SharePoint.Services
{
  using Microsoft.SharePoint;
  using System;
  using System.ServiceModel;
  using System.ServiceModel.Activation;

  public sealed class BaristaServiceHostFactory : ServiceHostFactory
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