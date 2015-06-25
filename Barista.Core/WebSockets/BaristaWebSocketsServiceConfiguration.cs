namespace Barista.WebSockets
{
  using System.Collections.Generic;
  using Barista.Jurassic;
  using Barista.Newtonsoft.Json;
  using Barista.SuperWebSocket;

  public sealed class BaristaWebSocketsServiceConfiguration
  {
    public BaristaWebSocketsServiceConfiguration()
    {
      Servers = new Dictionary<string, WebSocketServerConfiguration>();
    }

    [JsonProperty("servers")]
    public IDictionary<string, WebSocketServerConfiguration> Servers
    {
      get;
      set;
    }
  }

  public sealed class WebSocketServerConfiguration
  {
    [JsonIgnore]
    public ScriptEngine ScriptEngine
    {
      get;
      set;
    }

    [JsonIgnore]
    public WebSocketServer WebSocketServer
    {
      get;
      set;
    }

    [JsonProperty("startPortRange")]
    public int StartPortRange
    {
      get;
      set;
    }

    [JsonProperty("endPortRange")]
    public int EndPortRange
    {
      get;
      set;
    }

    [JsonProperty("details")]
    public WebSocketServerDetails Details
    {
      get;
      set;
    }
  }
}
