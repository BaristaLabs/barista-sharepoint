namespace Barista.WebSockets
{
  using Barista.Jurassic;
  using Barista.SuperWebSocket;

  internal class WebSocketServerInstance
  {
    public string Name
    {
      get;
      set;
    }

    public WebSocketServer Server
    {
      get;
      set;
    }

    public ScriptEngine ScriptEngine
    {
      get;
      set;
    }
  }
}
