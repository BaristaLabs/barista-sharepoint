namespace Barista
{
  using System.ServiceModel;
  using System.ServiceModel.Web;
  using Barista.WebSockets;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaWebSockets
  {
    /// <summary>
    /// Initializes and configures a new web socket server with the specified options and places the server in the Stopped state.
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    [OperationContract]
    [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    WebSocketServerDetails InitializeWebSocketServer(string serverName, WebSocketServerOptions options);

    /// <summary>
    /// Returns an object that contains details about the specified web socket server. If a web socket server with the specified name does not exist, returns null.
    /// </summary>
    /// <param name="serverName"></param>
    /// <returns></returns>
    [OperationContract]
    [WebGet(ResponseFormat = WebMessageFormat.Json)]
    WebSocketServerDetails GetWebSocketServerDetails(string serverName);

    /// <summary>
    /// Places the specified web socket server in the started state.
    /// </summary>
    /// <param name="serverName"></param>
    /// <returns></returns>
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    bool StartWebSocketServer(string serverName);
    
    /// <summary>
    /// Sends a message to the specified web socket server using the specified session id.
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="sessionId"></param>
    /// <param name="message"></param>
    [OperationContract]
    [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    void SendMessage(string serverName, string sessionId, string message);

    /// <summary>
    /// Places the specified web socket server in the stopped state.
    /// </summary>
    /// <param name="serverName"></param>
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    void StopWebSocketServer(string serverName);

    /// <summary>
    /// Stops the specified web socket server and removes it.
    /// </summary>
    /// <param name="serverName"></param>
    [OperationContract]
    [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json)]
    void RemoveWebSocketServer(string serverName);

    /// <summary>
    /// Adds, or overwrites, an event script associated with a specific type of event.
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="scriptName"></param>
    /// <param name="eventType"></param>
    /// <param name="eventScript"></param>
    [OperationContract]
    [WebInvoke(Method = "PUT", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    void PutEventScript(string serverName, string eventType, string scriptName, WebSocketEventScript eventScript);

    /// <summary>
    /// Removes an event script associated with a specific type of event.
    /// </summary>
    /// <param name="serverName"></param>
    /// <param name="eventType"></param>
    /// <param name="scriptName"></param>
    [OperationContract]
    [WebInvoke(Method = "DELETE", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
    void DeleteEventScript(string serverName, string eventType, string scriptName);
  }
}
