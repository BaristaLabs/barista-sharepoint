namespace Barista.WebSocket.Protocol
{
  /// <summary>
  /// Handshake request
  /// </summary>
  internal class HandshakeRequest : IWebSocketFragment
  {
    /// <summary>
    /// Gets the key of this request.
    /// </summary>
    public string Key
    {
      get { return OpCode.HandshakeTag; }
    }
  }
}
