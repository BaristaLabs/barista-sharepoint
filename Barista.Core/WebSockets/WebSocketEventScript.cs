namespace Barista.WebSockets
{
  using System.Runtime.Serialization;
  using Barista.Newtonsoft.Json;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class WebSocketEventScript
  {
    [DataMember]
    [JsonProperty("description")]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("script")]
    public string Script
    {
      get;
      set;
    }
  }
}
