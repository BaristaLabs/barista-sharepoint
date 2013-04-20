namespace Barista
{
  using System.ServiceModel;
  using Barista.WebSockets;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaWebSockets
  {
    [OperationContract]
    bool IsPortAvailable(int port);

    [OperationContract]
    int? GetAvailablePort(int start, int end);

    [OperationContract]
    bool SetupWebSocketServer(int port, WebSocketServerOptions options);

    [OperationContract]
    bool StartWebSocketServer(int port);

    [OperationContract]
    void SendMessage(int port, string message);

    [OperationContract]
    void StopWebSocketServer(int port);

    [OperationContract]
    void RemoveWebSocketServer(int port);
  }
}
