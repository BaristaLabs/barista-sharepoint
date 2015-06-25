namespace Barista.WebSockets
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Barista.Newtonsoft.Json;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  public sealed class WebSocketServerDetails
  {
    [DataMember]
    [JsonProperty("name")]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("state")]
    public WebSocketServerState State
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("activePort")]
    public int Port
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
