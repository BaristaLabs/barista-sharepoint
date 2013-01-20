namespace Barista.WebSocket.Command
{
  using Barista.WebSocket.Protocol;

  /// <summary>
  /// The command handling Text fragment
  /// </summary>
  /// <typeparam name="TWebSocketSession">The type of the web socket session.</typeparam>
  internal class Text<TWebSocketSession> : FragmentCommand<TWebSocketSession>
      where TWebSocketSession : WebSocketSession<TWebSocketSession>, new()
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    public override string Name
    {
      get
      {
        return OpCode.TextTag;
      }
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="requestInfo">The request info.</param>
    public override void ExecuteCommand(TWebSocketSession session, IWebSocketFragment requestInfo)
    {
      var frame = requestInfo as WebSocketDataFrame;

      if (!CheckFrame(frame))
      {
        session.Close();
        return;
      }

      if (frame != null && frame.FIN)
      {
        if (session.Frames.Count > 0)
        {
          session.Close();
          return;
        }

        var text = GetWebSocketText(frame);
        session.AppServer.OnNewMessageReceived(session, text);
      }
      else
      {
        session.Frames.Add(frame);
      }
    }
  }
}
