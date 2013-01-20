namespace Barista.WebSocket.SubProtocol
{
  using Barista.SocketBase.Protocol;

  /// <summary>
  /// SubProtocol RequestInfo type
  /// </summary>
  public class SubRequestInfo : RequestInfo<string>
  {
    /// <summary>
    /// Gets the token of this request, used for callback
    /// </summary>
    public string Token { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SubRequestInfo"/> class.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="token">The token.</param>
    /// <param name="data">The data.</param>
    public SubRequestInfo(string key, string token, string data)
      : base(key, data)
    {
      Token = token;
    }
  }
}
