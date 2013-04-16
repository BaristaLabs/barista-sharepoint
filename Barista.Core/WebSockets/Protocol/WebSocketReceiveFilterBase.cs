namespace Barista.WebSocket.Protocol
{
  using Barista.SocketBase;
  using Barista.SocketBase.Protocol;

  /// <summary>
  /// WebSocketReceiveFilter basis
  /// </summary>
  public abstract class WebSocketReceiveFilterBase : ReceiveFilterBase<IWebSocketFragment>
  {
    /// <summary>
    /// The length of Sec3Key
    /// </summary>
    protected const int SecKey3Len = 8;

    private readonly IWebSocketSession m_session;

    internal IWebSocketSession Session
    {
      get { return m_session; }
    }

    static WebSocketReceiveFilterBase()
    {
      HandshakeRequestInfo = new HandshakeRequest();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketReceiveFilterBase" /> class.
    /// </summary>
    /// <param name="session">The session.</param>
    protected WebSocketReceiveFilterBase(IWebSocketSession session)
    {
      m_session = session;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketReceiveFilterBase" /> class.
    /// </summary>
    /// <param name="previousReceiveFilter">The previous receive filter.</param>
    protected WebSocketReceiveFilterBase(WebSocketReceiveFilterBase previousReceiveFilter)
      : base(previousReceiveFilter)
    {
      m_session = previousReceiveFilter.Session;
    }

    /// <summary>
    /// Handshakes the specified protocol processor.
    /// </summary>
    /// <param name="protocolProcessor">The protocol processor.</param>
    /// <param name="session">The session.</param>
    /// <returns></returns>
    protected bool Handshake(IProtocolProcessor protocolProcessor, IWebSocketSession session)
    {
      IReceiveFilter<IWebSocketFragment> dataFrameReader;

      if (!protocolProcessor.Handshake(session, this, out dataFrameReader))
      {
        session.Close(CloseReason.ServerClosing);
        return false;
      }

      //Processor handshake sucessfully, but output datareader is null, so the multiple protocol switch handled the handshake
      //In this case, the handshake is not completed
      if (dataFrameReader == null)
      {
        NextReceiveFilter = this;
        return false;
      }

      NextReceiveFilter = dataFrameReader;
      return true;
    }

    /// <summary>
    /// Gets the handshake request info.
    /// </summary>
    protected static IWebSocketFragment HandshakeRequestInfo { get; private set; }
  }
}
