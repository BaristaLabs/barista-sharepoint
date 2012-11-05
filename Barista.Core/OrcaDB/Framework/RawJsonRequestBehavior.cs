namespace OFS.OrcaDB.Core.Framework
{
  using System;
  using System.Collections.ObjectModel;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Configuration;
  using System.ServiceModel.Description;
  using System.ServiceModel.Dispatcher;
  using System.ServiceModel.Web;

  public class RawJsonRequestBehaviorAttribute : Attribute, IServiceBehavior
  {
    public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }
  }
}
