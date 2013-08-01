namespace Barista.Framework
{
  using System;
  using System.Collections.ObjectModel;
  using System.ServiceModel;
  using System.ServiceModel.Channels;
  using System.ServiceModel.Configuration;
  using System.ServiceModel.Description;
  using System.ServiceModel.Dispatcher;

  /// <summary>
  /// Represents a Fault Behavior that sets the response code to 200 on fault so that the exception details can be read by the Silverlight Client.
  /// </summary>
  /// <remarks>
  /// See http://msdn.microsoft.com/en-us/library/ee844556(v=vs.95).aspx
  /// </remarks>
  public class SilverlightFaultBehaviorAttribute : Attribute, IServiceBehavior
  {
    public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
    {
      foreach (var endpoint in endpoints)
      {
        endpoint.Behaviors.Add(new SilverlightFaultBehavior());
      }
    }

    public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }

    public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
    {
    }
  }

  public class SilverlightFaultBehavior : BehaviorExtensionElement, IEndpointBehavior
  {
    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
    {
      SilverlightFaultMessageInspector inspector = new SilverlightFaultMessageInspector();
      endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
    }
    public class SilverlightFaultMessageInspector : IDispatchMessageInspector
    {
      public void BeforeSendReply(ref Message reply, object correlationState)
      {
        if (reply != null && reply.IsFault)
        {
          var property = new HttpResponseMessageProperty {
            StatusCode = System.Net.HttpStatusCode.OK,
            StatusDescription = reply.ToString()
          };

          // Here the response code is changed to 200.

          reply.Properties[HttpResponseMessageProperty.Name] = property;
        }
      }

      public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
      {
        // Do nothing to the incoming message.
        return null;
      }
    }

    // The following methods are stubs and not relevant. 
    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
    }

    public void Validate(ServiceEndpoint endpoint)
    {
    }

    public override System.Type BehaviorType
    {
      get { return typeof(SilverlightFaultBehavior); }
    }

    protected override object CreateBehavior()
    {
      return new SilverlightFaultBehavior();
    }
  }
}
