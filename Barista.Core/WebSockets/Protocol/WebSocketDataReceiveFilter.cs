namespace Barista.WebSocket.Protocol
{
  using Barista.SocketBase;
  using System.Text;

  internal class WebSocketDataReceiveFilter : WebSocketReceiveFilterBase
  {
    private byte? m_type;
    private int m_tempLength;
    private int? m_length;

    private const byte ClosingHandshakeType = 0xFF;

    public WebSocketDataReceiveFilter(WebSocketReceiveFilterBase prevFilter)
      : base(prevFilter)
    {

    }

    public override IWebSocketFragment Filter(byte[] readBuffer, int offset, int length, bool isReusableBuffer, out int rest)
    {
      rest = 0;

      var skipByteCount = 0;

      if (!m_type.HasValue)
      {
        byte startByte = readBuffer[offset];
        skipByteCount = 1;
        m_type = startByte;
      }

      //0xxxxxxx: Collect protocol data by end mark
      if ((m_type.Value & 0x80) == 0x00)
      {
        const byte lookForByte = 0xFF;

        int i;

        for (i = offset + skipByteCount; i < offset + length; i++)
        {
          if (readBuffer[i] == lookForByte)
          {
            rest = length - (i - offset + 1);

            if (BufferSegments.Count <= 0)
            {
              var commandInfo = new PlainFragment(Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, i - offset - skipByteCount));
              Reset();
              return commandInfo;
            }
            else
            {
              AddArraySegment(readBuffer, offset + skipByteCount, i - offset - skipByteCount, false);
              var commandInfo = new PlainFragment(BufferSegments.Decode(Encoding.UTF8));
              Reset();
              return commandInfo;
            }
          }
        }

        AddArraySegment(readBuffer, offset + skipByteCount, length - skipByteCount, isReusableBuffer);
        return null;
      }

      while (!m_length.HasValue)
      {
        if (length <= skipByteCount)
        {
          //No data to read
          return null;
        }

        byte lengthByte = readBuffer[skipByteCount];
        //Closing handshake
        if (lengthByte == 0x00 && m_type.Value == ClosingHandshakeType)
        {
          Session.Close(CloseReason.ClientClosing);
          return null;
        }

        int thisLength = lengthByte & 0x7F;
        m_tempLength = m_tempLength * 128 + thisLength;
        skipByteCount++;

        if ((lengthByte & 0x80) != 0x80)
        {
          m_length = m_tempLength;
          break;
        }
      }

      int requiredSize = m_length.Value - BufferSegments.Count;

      int leftSize = length - skipByteCount;

      if (leftSize < requiredSize)
      {
        AddArraySegment(readBuffer, skipByteCount, length - skipByteCount, isReusableBuffer);
        return null;
      }

      rest = leftSize - requiredSize;

      if (BufferSegments.Count <= 0)
      {
        var commandInfo = new PlainFragment(Encoding.UTF8.GetString(readBuffer, offset + skipByteCount, requiredSize));
        Reset();
        return commandInfo;
      }
      else
      {
        AddArraySegment(readBuffer, offset + skipByteCount, requiredSize, false);
        var commandInfo = new PlainFragment(BufferSegments.Decode(Encoding.UTF8));
        Reset();
        return commandInfo;
      }
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
      base.Reset();

      m_type = null;
      m_length = null;
      m_tempLength = 0;
    }
  }
}
