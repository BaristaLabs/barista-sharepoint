﻿namespace Barista.WebSocket.Protocol
{
  using Barista.Extensions;
  using System;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Text;
  using System.Text.RegularExpressions;
  using Barista.SocketBase.Protocol;

  /// <summary>
  /// http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-00
  /// </summary>
  class DraftHybi00Processor : ProtocolProcessorBase
  {
    private static readonly byte[] ZeroKeyBytes = new byte[0];

    public DraftHybi00Processor()
      : base(0, new CloseStatusCodeHybi10())
    {

    }

    public override bool Handshake(IWebSocketSession session, WebSocketReceiveFilterBase previousFilter, out IReceiveFilter<IWebSocketFragment> dataFrameReader)
    {
      var secKey1 = session.Items.GetValue(WebSocketConstant.SecWebSocketKey1, string.Empty);
      var secKey2 = session.Items.GetValue(WebSocketConstant.SecWebSocketKey2, string.Empty);

      if (string.IsNullOrEmpty(secKey1) && string.IsNullOrEmpty(secKey2) && NextProcessor != null)
      {
        return NextProcessor.Handshake(session, previousFilter, out dataFrameReader);
      }

      session.ProtocolProcessor = this;

      var secKey3 = session.Items.GetValue(WebSocketConstant.SecWebSocketKey3, ZeroKeyBytes);

      var responseBuilder = new StringBuilder();

      responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseHeadLine00);
      responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseUpgradeLine);
      responseBuilder.AppendWithCrCf(WebSocketConstant.ResponseConnectionLine);

      if (!string.IsNullOrEmpty(session.Origin))
        responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseOriginLine, session.Origin);

      responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseLocationLine, session.UriScheme, session.Host, session.Path);

      var subProtocol = session.GetAvailableSubProtocol(session.Items.GetValue(WebSocketConstant.SecWebSocketProtocol, string.Empty));

      if (!string.IsNullOrEmpty(subProtocol))
        responseBuilder.AppendFormatWithCrCf(WebSocketConstant.ResponseProtocolLine, subProtocol);

      responseBuilder.AppendWithCrCf();
      byte[] data = Encoding.UTF8.GetBytes(responseBuilder.ToString());
      session.SendRawData(data, 0, data.Length);
      //Encrypt message
      byte[] secret = GetResponseSecurityKey(secKey1, secKey2, secKey3);
      session.SendRawData(secret, 0, secret.Length);

      dataFrameReader = new WebSocketDataReceiveFilter(previousFilter);

      return true;
    }

    private const string SecurityKeyRegex = "[^0-9]";

    private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
    {
      //Remove all symbols that are not numbers
      string k1 = Regex.Replace(secKey1, SecurityKeyRegex, String.Empty);
      string k2 = Regex.Replace(secKey2, SecurityKeyRegex, String.Empty);

      //Convert received string to 64 bit integer.
      Int64 intK1 = Int64.Parse(k1);
      Int64 intK2 = Int64.Parse(k2);

      //Dividing on number of spaces
      int k1Spaces = secKey1.Count(c => c == ' ');
      int k2Spaces = secKey2.Count(c => c == ' ');
      int k1FinalNum = (int)(intK1 / k1Spaces);
      int k2FinalNum = (int)(intK2 / k2Spaces);

      //Getting byte parts
      byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
      byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
      //byte[] b3 = Encoding.UTF8.GetBytes(secKey3);
      byte[] b3 = secKey3;

      //Concatenating everything into 1 byte array for hashing.
      byte[] bChallenge = new byte[b1.Length + b2.Length + b3.Length];
      Array.Copy(b1, 0, bChallenge, 0, b1.Length);
      Array.Copy(b2, 0, bChallenge, b1.Length, b2.Length);
      Array.Copy(b3, 0, bChallenge, b1.Length + b2.Length, b3.Length);

      //Hash and return
      byte[] hash = MD5.Create().ComputeHash(bChallenge);
      return hash;
    }

    public override void SendMessage(IWebSocketSession session, string message)
    {
      var maxByteCount = Encoding.UTF8.GetMaxByteCount(message.Length) + 2;
      var sendBuffer = new byte[maxByteCount];
      sendBuffer[0] = WebSocketConstant.StartByte;
      int bytesCount = Encoding.UTF8.GetBytes(message, 0, message.Length, sendBuffer, 1);
      sendBuffer[1 + bytesCount] = WebSocketConstant.EndByte;

      session.SendRawData(sendBuffer, 0, bytesCount + 2);
    }

    public override void SendCloseHandshake(IWebSocketSession session, int statusCode, string closeReason)
    {
      session.SendRawData(WebSocketConstant.ClosingHandshake, 0, WebSocketConstant.ClosingHandshake.Length);
    }

    public override void SendPong(IWebSocketSession session, byte[] pong)
    {

    }

    public override void SendPing(IWebSocketSession session, byte[] ping)
    {

    }

    public override bool CanSendBinaryData
    {
      get { return false; }
    }

    public override void SendData(IWebSocketSession session, byte[] data, int offset, int length)
    {
      throw new NotSupportedException();
    }

    public override bool IsValidCloseCode(int code)
    {
      throw new NotSupportedException();
    }
  }
}
