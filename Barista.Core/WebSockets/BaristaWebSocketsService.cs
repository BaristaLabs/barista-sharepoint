namespace Barista.WebSockets
{
  using Barista.Extensions;
  using Barista.Jurassic;
  using SuperWebSocket;
  using System;
  using System.Linq;
  using System.Net.NetworkInformation;
  using System.ServiceModel;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public abstract class BaristaWebSocketsService : IBaristaWebSockets
  {
    internal static BaristaWebSocketsServiceConfiguration Configuration = new BaristaWebSocketsServiceConfiguration();

    public WebSocketServerDetails GetWebSocketServerDetails(string serverName)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        return null;

      return Configuration.Servers[serverName].Details;
    }

    public WebSocketServerDetails InitializeWebSocketServer(string serverName, WebSocketServerOptions options)
    {
      //Argument Validation.
      if (serverName.IsNullOrWhiteSpace())
        throw new ArgumentNullException("serverName");

      if (options == null)
        throw new ArgumentNullException("options");

      //Validate that a service with the specified name is not present in configuration.
      if (Configuration.Servers.ContainsKey(serverName))
        throw new InvalidOperationException(
          "A web socket server with the specified name has already been defined. Please remove this existing server prior to initializing it.");

      lock (Configuration)
      {
        var appServer = new WebSocketServer();

        appServer.NewMessageReceived += appServer_MessageReceived;
        appServer.NewSessionConnected += appServer_NewSessionConnected;
        appServer.SessionClosed += appServer_SessionClosed;

        var port = GetAvailablePort(options.StartPortRange, options.EndPortRange);

        if (port.HasValue == false)
          throw new InvalidOperationException("An open port in the specified range could not be found.");

        if (appServer.Setup(port.Value) == false)
          throw new InvalidOperationException("A Web Socket Server could not be initialized on the specified port.");

        var config = new WebSocketServerConfiguration
        {
          StartPortRange = options.StartPortRange,
          EndPortRange = options.EndPortRange,
          ScriptEngine = this.GetScriptEngine(),
          WebSocketServer = appServer,
          Details = new WebSocketServerDetails
          {
            Name = serverName,
            Port = port.Value,
            State = WebSocketServerState.Initialized,

            OnNewMessageReceived = options.OnNewMessageReceived,
            OnNewSessionConnected = options.OnNewSessionConnected,
            OnSessionClosed = options.OnSessionClosed,
          }
        };


        Configuration.Servers.Add(serverName, config);
        return config.Details;
      }
    }

    public bool StartWebSocketServer(string serverName)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        return false;

      var appServer = Configuration.Servers[serverName].WebSocketServer;

      var result = appServer.Start();

      if (result)
        Configuration.Servers[serverName].Details.State = WebSocketServerState.Started;

      return result;
    }

    public void SendMessage(string serverName, string sessionId, string message)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        throw new InvalidOperationException("Unable to send message -- The specified server does not exist.");

      var appServer = Configuration.Servers[serverName].WebSocketServer;

      var session = appServer.GetAppSessionByID(sessionId);

      if (session == null)
        return;

      session.Send(message);
    }

    public void StopWebSocketServer(string serverName)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        throw new InvalidOperationException("Unable to stop the server -- The specified server does not exist.");

      var appServer = Configuration.Servers[serverName].WebSocketServer;
      appServer.Stop();

      Configuration.Servers[serverName].Details.State = WebSocketServerState.Stopped;
    }

    public void RemoveWebSocketServer(string serverName)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        throw new InvalidOperationException("Unable to remove the server -- The specified server does not exist.");

      var configuration = Configuration.Servers[serverName];
      configuration.WebSocketServer.Stop();
      configuration.WebSocketServer.Dispose();
      configuration.Details.State = WebSocketServerState.Stopped;

      Configuration.Servers.Remove(serverName);
    }

    public void PutEventScript(string serverName, string eventType, string scriptName, WebSocketEventScript eventScript)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        throw new InvalidOperationException("Unable to add the event script -- The specified server does not exist.");

      var configuration = Configuration.Servers[serverName];
      
      throw new NotImplementedException();
    }

    public void DeleteEventScript(string serverName, string eventType, string scriptName)
    {
      if (Configuration.Servers.ContainsKey(serverName) == false)
        throw new InvalidOperationException("Unable to add the event script -- The specified server does not exist.");

      var configuration = Configuration.Servers[serverName];

      throw new NotImplementedException();
    }

    #region Protected Methods

    /// <summary>
    /// In a concrete class, returns a new instance of a script engine. Called once per WebSocketServer on initialization.
    /// </summary>
    /// <returns></returns>
    protected abstract ScriptEngine GetScriptEngine();

    protected virtual void appServer_MessageReceived(WebSocketSession session, string message)
    {
      //TODO: Obtain a script engine, and execute the onmessagereceived script form configuration.
      //Send the received message back
      session.Send("Server: " + message);
    }

    protected virtual void appServer_NewSessionConnected(WebSocketSession session)
    {
      //TODO: Obtain a script engine, and execute the onnewsessionconnection script form configuration.
    }

    protected virtual void appServer_SessionClosed(WebSocketSession session, SuperSocket.SocketBase.CloseReason value)
    {
      throw new NotImplementedException();
    }

    protected bool IsPortAvailable(int port)
    {
      var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
      var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

      return tcpConnInfoArray.All(tcpi => tcpi.LocalEndPoint.Port != port);
    }

    protected int? GetAvailablePort(int start, int end)
    {
      if (end <= start || start > Int16.MaxValue || end > Int16.MaxValue || start <= 0 || end <= 0)
        throw new ArgumentOutOfRangeException();

      var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
      var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

      return Enumerable.Range(start, end - start)
                       .Except(tcpConnInfoArray.Select(t => t.LocalEndPoint.Port))
                       .OrderBy(i => i)
                       .FirstOrDefault();
    }
    #endregion
  }
}
