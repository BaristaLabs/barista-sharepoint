namespace Barista.WebSockets
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class WebSocketServerOptions
  {
    [DataMember]
    public string OnMessageReceived
    {
      get;
      set;
    }

    [DataMember]
    public string OnNewSessionConnected
    {
      get;
      set;
    }

    [DataMember]
    public string OnSessionConnected
    {
      get;
      set;
    }
  }
}
