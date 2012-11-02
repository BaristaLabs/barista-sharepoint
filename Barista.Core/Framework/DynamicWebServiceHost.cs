namespace Barista.Framework
{
  using System;
  using System.ServiceModel.Description;
  using System.ServiceModel.Web;

  /// <summary>
  /// Represents a WCF Web service host that modifies the response format to match the Accept-Header of the request.
  /// </summary>
  public class DynamicWebServiceHost : WebServiceHost
  {
    public DynamicWebServiceHost(Type serviceType, params Uri[] baseAddresses)
      : base(serviceType, baseAddresses) { }

    protected override void OnOpening()
    {
      base.OnOpening();

      foreach (var ep in this.Description.Endpoints)
      {
        if (ep.Behaviors.Find<WebHttpBehavior>() != null)
        {
          ep.Behaviors.Remove<WebHttpBehavior>();
          ep.Behaviors.Add(new DynamicWebHttpBehavior());
        }
      }
    }
  }
}
