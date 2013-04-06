namespace Barista.Helpers
{
  using System.Net;
  using System.Net.Sockets;

  public static class PortUtilities
  {
    public static int FindFreePort()
    {
      int port;
      var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      try
      {
        IPEndPoint pEndPoint = new IPEndPoint(IPAddress.Any, 0);
        socket.Bind(pEndPoint);
        pEndPoint = (IPEndPoint)socket.LocalEndPoint;
        port = pEndPoint.Port;
      }
      finally
      {
        socket.Close();
      }
      return port;
    }
  }
}
