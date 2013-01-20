namespace Barista.SharePoint.HostService
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class WebSocketServerOptions
  {
    [DataMember]
    public string ReceiverCode
    {
      get;
      set;
    }
  }
}
