namespace Barista.WebSocket.Protocol
{
  using System.Globalization;
  using Barista;

  public class WebSocketDataFrame : IWebSocketFragment
  {
    private readonly ArraySegmentList m_innerData;

    public ArraySegmentList InnerData
    {
      get { return m_innerData; }
    }

    public WebSocketDataFrame(ArraySegmentList data)
    {
      m_innerData = data;
      m_innerData.ClearSegements();
    }

    // ReSharper disable InconsistentNaming
    public bool FIN
    {
      get { return ((m_innerData[0] & 0x80) == 0x80); }
    }

    public bool RSV1
    {
      get { return ((m_innerData[0] & 0x40) == 0x40); }
    }

    public bool RSV2
    {
      get { return ((m_innerData[0] & 0x20) == 0x20); }
    }

    public bool RSV3
    {
      get { return ((m_innerData[0] & 0x10) == 0x10); }
    }
    // ReSharper restore InconsistentNaming

    public sbyte OpCode
    {
      get { return (sbyte)(m_innerData[0] & 0x0f); }
    }

    public bool HasMask
    {
      get { return ((m_innerData[1] & 0x80) == 0x80); }
    }

    public sbyte PayloadLenght
    {
      get { return (sbyte)(m_innerData[1] & 0x7f); }
    }

    private long m_actualPayloadLength = -1;

    public long ActualPayloadLength
    {
      get
      {
        if (m_actualPayloadLength >= 0)
          return m_actualPayloadLength;

        var payloadLength = PayloadLenght;

        if (payloadLength < 126)
          m_actualPayloadLength = payloadLength;
        else if (payloadLength == 126)
        {
          m_actualPayloadLength = m_innerData[2] * 256 + m_innerData[3];
        }
        else
        {
          long len = 0;
          int n = 1;

          for (int i = 7; i >= 0; i--)
          {
            len += m_innerData[i + 2] * n;
            n *= 256;
          }

          m_actualPayloadLength = len;
        }

        return m_actualPayloadLength;
      }
    }

    public byte[] MaskKey { get; set; }

    public byte[] ExtensionData { get; set; }

    public byte[] ApplicationData { get; set; }

    public int Length
    {
      get { return m_innerData.Count; }
    }

    public void Clear()
    {
      m_innerData.ClearSegements();
      ExtensionData = new byte[0];
      ApplicationData = new byte[0];
      m_actualPayloadLength = -1;
    }

    public string Key
    {
      get { return OpCode.ToString(CultureInfo.InvariantCulture); }
    }
  }
}
