namespace Barista.WebSockets
{
  using System;
  using System.Collections.Concurrent;
  using System.Linq;
  using System.Net.NetworkInformation;
  using System.ServiceModel;
  using SuperWebSocket;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public class BaristaWebSocketsService : IBaristaWebSockets
  {
    internal static readonly ConcurrentDictionary<int, WebSocketServer> Servers = new ConcurrentDictionary<int, WebSocketServer>();

    public bool SetupWebSocketServer(int port, WebSocketServerOptions options)
    {
      var appServer = new WebSocketServer();
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

    public void SendMessage(int port, string message)
    {
      throw new NotImplementedException("Blah");
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
  }
}
