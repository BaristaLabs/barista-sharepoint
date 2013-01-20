namespace Barista.WebSocket.SubProtocol
{
  /// <summary>
  /// SubCommand interface
  /// </summary>
  /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
  public interface ISubCommand<TWebSocketSession>
      where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    void ExecuteCommand(TWebSocketSession session, SubRequestInfo requestInfo);
  }
}
