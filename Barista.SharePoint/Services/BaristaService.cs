namespace Barista.SharePoint.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Text;
  using Microsoft.SharePoint.Administration;

  [Guid("929093C2-7BBF-41DE-8FAB-C4B2A161924D")]
  public class BaristaService : SPIisWebService, IServiceAdministration
  {
    #region Constructors
    public BaristaService()
    {
    }

    public BaristaService(SPFarm farm)
      : base(farm)
    {
    }
    #endregion

    public SPServiceApplication CreateApplication(string name, Type serviceApplicationType, SPServiceProvisioningContext provisioningContext)
    {
      if (serviceApplicationType != typeof(BaristaServiceApplication))
        throw new NotSupportedException();

      if (provisioningContext == null)
        throw new ArgumentNullException("provisioningContext");

      // if the service doesn't already exist, create it
      BaristaServiceApplication serviceApp = this.Farm.GetObject(name, this.Id, serviceApplicationType) as BaristaServiceApplication;
      if (serviceApp == null)
        serviceApp = BaristaServiceApplication.Create(name, this, provisioningContext.IisWebServiceApplicationPool);

      return serviceApp;
    }

    public SPServiceApplicationProxy CreateProxy(string name, SPServiceApplication serviceApplication, SPServiceProvisioningContext provisioningContext)
    {
      if (serviceApplication.GetType() != typeof(BaristaServiceApplication))
        throw new NotSupportedException();

      if (serviceApplication == null)
        throw new ArgumentNullException("serviceApplication");

      // verify the service proxy exists
      BaristaServiceProxy serviceProxy = (BaristaServiceProxy)this.Farm.GetObject(name, this.Farm.Id, typeof(BaristaServiceProxy));
      if (serviceProxy == null)
        throw new InvalidOperationException("BaristaServiceProxy does not exist in the farm.");

      // if the app proxy doesn't exist, create it
      BaristaServiceApplicationProxy applicationProxy = serviceProxy.ApplicationProxies.GetValue<BaristaServiceApplicationProxy>(name);
      if (applicationProxy == null)
      {
        Uri serviceAppAddress = ((BaristaServiceApplication)serviceApplication).Uri;
        applicationProxy = new BaristaServiceApplicationProxy(name, serviceProxy, serviceAppAddress);
      }

      return applicationProxy;
    }

    public SPPersistedTypeDescription GetApplicationTypeDescription(Type serviceApplicationType)
    {
      if (serviceApplicationType != typeof(BaristaServiceApplication))
        throw new NotSupportedException();

      return new SPPersistedTypeDescription("Barista Service", "Custom service application providing scripted service capabilities.");
    }

    public Type[] GetApplicationTypes()
    {
      return new Type[] { typeof(BaristaServiceApplication) };
    }

    public override SPAdministrationLink GetCreateApplicationLink(Type serviceApplicationType)
    {
      return new SPAdministrationLink("/_admin/BaristaService/Create.aspx");
    }
  }
}
