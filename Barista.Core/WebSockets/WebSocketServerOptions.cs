namespace Barista.WebSockets
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Barista.Newtonsoft.Json;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class WebSocketServerOptions
  {
    [DataMember]
    [JsonProperty("startPortRange")]
    public int StartPortRange
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("endPortRange")]
    public int EndPortRange
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("onNewMessageReceived")]
    public IDictionary<string, WebSocketEventScript> OnNewMessageReceived
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("onNewSessionConnected")]
    public IDictionary<string, WebSocketEventScript> OnNewSessionConnected
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("onSessionClosed")]
    public IDictionary<string, WebSocketEventScript> OnSessionClosed
    {
      get;
      set;
    }
  }
}
