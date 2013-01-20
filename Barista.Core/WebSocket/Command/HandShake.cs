namespace Barista.WebSocket.Command
{
  using Barista.SocketBase.Command;
  using Barista.WebSocket.Protocol;

  /// <summary>
  /// The command handle handshake request
  /// </summary>
  /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
  internal class HandShake<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
      where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    public override string Name
    {
      get
      {
        return OpCode.HandshakeTag;
      }
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
    {
      session.OnHandshakeSuccess();
    }
  }
}
