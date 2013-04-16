namespace Barista.WebSocket.Protocol
{
  using Barista;
  using Barista.SocketBase.Protocol;
  using Barista.WebSocket.Protocol.FramePartReader;

  class WebSocketDataFrameReceiveFilter : IReceiveFilter<IWebSocketFragment>
  {
    private WebSocketDataFrame m_frame;
    private IDataFramePartReader m_partReader;
    private int m_lastPartLength;

    public int LeftBufferSize
    {
      get { return m_frame.InnerData.Count; }
    }

    public IReceiveFilter<IWebSocketFragment> NextReceiveFilter
    {
      get { return this; }
    }

    public WebSocketDataFrameReceiveFilter()
    {
      m_frame = new WebSocketDataFrame(new ArraySegmentList());
      m_partReader = DataFramePartReader.NewReader;
    }

    protected void AddArraySegment(ArraySegmentList segments, byte[] buffer, int offset, int length, bool toBeCopied)
    {
      segments.AddSegment(buffer, offset, length, toBeCopied);
    }

    public IWebSocketFragment Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
    {
      if (m_frame == null)
        m_frame = new WebSocketDataFrame(new ArraySegmentList());

      this.AddArraySegment(m_frame.InnerData, readBuffer, offset, length, toBeCopied);

      IDataFramePartReader nextPartReader;

      int thisLength = m_partReader.Process(m_lastPartLength, m_frame, out nextPartReader);

      if (thisLength < 0)
      {
        left = 0;
        return null;
      }

      left = thisLength;

      if (left > 0)
        m_frame.InnerData.TrimEnd(left);

      //Means this part reader is the last one
      if (nextPartReader == null)
      {
        m_lastPartLength = 0;
        m_partReader = DataFramePartReader.NewReader;

        var frame = m_frame;
        m_frame = null;
        return frame;
      }

      m_lastPartLength = m_frame.InnerData.Count - thisLength;
      m_partReader = nextPartReader;

      return null;
    }

    public void Reset()
    {
      m_frame = null;
    }


    public FilterState State { get; private set; }
  }
}
