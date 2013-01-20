﻿namespace Barista.WebSocket.Command
{
  using Barista.SocketBase.Command;
  using Barista.WebSocket.Protocol;

  /// <summary>
  /// The command to handling text message in plain text of hybi00
  /// </summary>
  /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
  internal class Plain<TWebSocketSession> : CommandBase<TWebSocketSession, IWebSocketFragment>
      where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    public override string Name
    {
      get
      {
        return OpCode.PlainTag;
      }
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
    {
      var plainFragment = requestInfo as PlainFragment;

      session.AppServer.OnNewMessageReceived(session, plainFragment.Message);
    }
  }
}
