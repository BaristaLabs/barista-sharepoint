namespace Barista.WebSockets
{
  using SuperWebSocket;
  using System;
  using System.Collections.Concurrent;
  using System.Linq;
  using System.Net.NetworkInformation;
  using System.ServiceModel;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public class BaristaWebSocketsService : IBaristaWebSockets
  {
    internal static readonly ConcurrentDictionary<int, WebSocketServer> Servers = new ConcurrentDictionary<int, WebSocketServer>();

    public bool SetupWebSocketServer(int port, WebSocketServerOptions options)
    {
      var appServer = new WebSocketServer();
      appServer.NewMessageReceived += appServer_MessageReceived;
      return appServer.Setup(port) && Servers.TryAdd(port, appServer);
    }

    public bool IsPortAvailable(int port)
    {
      var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
      var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

      return tcpConnInfoArray.All(tcpi => tcpi.LocalEndPoint.Port != port);
    }

    public int? GetAvailablePort(int start, int end)
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

    public bool StartWebSocketServer(int port)
    {
      WebSocketServer appServer;
      return Servers.TryGetValue(port, out appServer) && appServer.Start();
    }

    public void SendMessage(int port, string sessionId, string message)
    {
      WebSocketServer appServer;
      if (Servers.TryGetValue(port, out appServer) == false)
        throw new InvalidOperationException("Unable to send message -- an app server does not exist on the specified port.");

      var session = appServer.GetAppSessionByID(sessionId);
      session.Send(message);
    }

    public void StopWebSocketServer(int port)
    {
      WebSocketServer appServer;
      if (Servers.TryGetValue(port, out appServer))
      {
        appServer.Stop();
      }
    }

    public void RemoveWebSocketServer(int port)
    {
      WebSocketServer appServer;
      Servers.TryRemove(port, out appServer);
    }

    private static void appServer_MessageReceived(WebSocketSession session, string message)
    {
      //TODO: Obtain a script engine, and execute the onmessagedreceived script form configuration.
      //Send the received message back
      session.Send("Server: " + message);
    }
  }
}
