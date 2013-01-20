namespace Barista.SocketBase.Protocol
{
  using System;
  using System.Text;

  /// <summary>
  /// Terminator Receive filter
  /// </summary>
  /// <typeparam name="TRequestInfo">The type of the request info.</typeparam>
  public abstract class TerminatorReceiveFilter<TRequestInfo> : ReceiveFilterBase<TRequestInfo>, IOffsetAdapter
      where TRequestInfo : IRequestInfo
  {
    private readonly SearchMarkState<byte> m_searchState;

    private readonly IAppSession m_session;

    /// <summary>
    /// Gets the session assosiated with the Receive filter.
    /// </summary>
    protected IAppSession Session
    {
      get { return m_session; }
    }

    /// <summary>
    /// Null RequestInfo
    /// </summary>
    protected static readonly TRequestInfo NullRequestInfo = default(TRequestInfo);

    private int m_parsedLengthInBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminatorReceiveFilter&lt;TRequestInfo&gt;"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="terminator">The terminator.</param>
    protected TerminatorReceiveFilter(IAppSession session, byte[] terminator)
    {
      m_session = session;
      m_searchState = new SearchMarkState<byte>(terminator);
    }

    /// <summary>
    /// Filters received data of the specific session into request info.
    /// </summary>
    /// <param name="readBuffer">The read buffer.</param>
    /// <param name="offset">The offset of the current received data in this read buffer.</param>
    /// <param name="length">The length of the current received data.</param>
    /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
    /// <param name="rest">The rest, the length of the data which hasn't been parsed.</param>
    /// <returns>return the parsed TRequestInfo</returns>
    public override TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
    {
      rest = 0;

      int prevMatched = m_searchState.Matched;

      int result = readBuffer.SearchMark(offset, length, m_searchState);

      if (result < 0)
      {
        if (m_offsetDelta != m_parsedLengthInBuffer)
        {
          Buffer.BlockCopy(readBuffer, offset - m_parsedLengthInBuffer, readBuffer, offset - m_offsetDelta, m_parsedLengthInBuffer + length);

          m_parsedLengthInBuffer += length;
          m_offsetDelta = m_parsedLengthInBuffer;
        }
        else
        {
          m_parsedLengthInBuffer += length;

          if (m_parsedLengthInBuffer >= m_session.Config.ReceiveBufferSize)
          {
            this.AddArraySegment(readBuffer, offset + length - m_parsedLengthInBuffer, m_parsedLengthInBuffer, toBeCopied);
            m_parsedLengthInBuffer = 0;
            m_offsetDelta = 0;

            return NullRequestInfo;
          }

          m_offsetDelta += length;
        }

        return NullRequestInfo;
      }

      var findLen = result - offset;

      rest = length - findLen - (m_searchState.Mark.Length - prevMatched);

      TRequestInfo requestInfo;

      if (findLen > 0)
      {
        if (this.BufferSegments != null && this.BufferSegments.Count > 0)
        {
          this.AddArraySegment(readBuffer, offset - m_parsedLengthInBuffer, findLen + m_parsedLengthInBuffer, toBeCopied);
          requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
        }
        else
        {
          requestInfo = ProcessMatchedRequest(readBuffer, offset - m_parsedLengthInBuffer, findLen + m_parsedLengthInBuffer);
        }
      }
      else if (prevMatched > 0)
      {
        if (m_parsedLengthInBuffer > 0)
        {
          if (m_parsedLengthInBuffer < prevMatched)
          {
            BufferSegments.TrimEnd(prevMatched - m_parsedLengthInBuffer);
            requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
          }
          else
          {
            if (this.BufferSegments != null && this.BufferSegments.Count > 0)
            {
              this.AddArraySegment(readBuffer, offset - m_parsedLengthInBuffer, m_parsedLengthInBuffer - prevMatched, toBeCopied);
              requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
            }
            else
            {
              requestInfo = ProcessMatchedRequest(readBuffer, offset - m_parsedLengthInBuffer, m_parsedLengthInBuffer - prevMatched);
            }
          }
        }
        else
        {
          BufferSegments.TrimEnd(prevMatched);
          requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
        }
      }
      else
      {
        if (this.BufferSegments != null && this.BufferSegments.Count > 0)
        {
          requestInfo = ProcessMatchedRequest(BufferSegments, 0, BufferSegments.Count);
        }
        else
        {
          requestInfo = ProcessMatchedRequest(readBuffer, offset - m_parsedLengthInBuffer, m_parsedLengthInBuffer);
        }
      }

      InternalReset();

      if (rest == 0)
      {
        m_offsetDelta = 0;
      }
      else
      {
        m_offsetDelta += (length - rest);
      }

      return requestInfo;
    }

    private void InternalReset()
    {
      m_parsedLengthInBuffer = 0;
      m_searchState.Matched = 0;
      base.Reset();
    }

    /// <summary>
    /// Resets this instance.
    /// </summary>
    public override void Reset()
    {
      InternalReset();
      m_offsetDelta = 0;
    }


    private TRequestInfo ProcessMatchedRequest(ArraySegmentList data, int offset, int length)
    {
      var targetData = data.ToArrayData(offset, length);
      return ProcessMatchedRequest(targetData, 0, length);
    }

    /// <summary>
    /// Resolves the specified data to TRequestInfo.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    protected abstract TRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length);


    private int m_offsetDelta;

    int IOffsetAdapter.OffsetDelta
    {
      get { return m_offsetDelta; }
    }
  }

  /// <summary>
  /// TerminatorRequestFilter
  /// </summary>
  public class TerminatorReceiveFilter : TerminatorReceiveFilter<StringRequestInfo>
  {
    private readonly Encoding m_encoding;
    private readonly IRequestInfoParser<StringRequestInfo> m_requestParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminatorReceiveFilter"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="terminator">The terminator.</param>
    /// <param name="encoding">The encoding.</param>
    public TerminatorReceiveFilter(IAppSession session, byte[] terminator, Encoding encoding)
      : this(session, terminator, encoding, BasicRequestInfoParser.DefaultInstance)
    {

    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TerminatorReceiveFilter"/> class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="terminator">The terminator.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="requestParser">The request parser.</param>
    public TerminatorReceiveFilter(IAppSession session, byte[] terminator, Encoding encoding, IRequestInfoParser<StringRequestInfo> requestParser)
      : base(session, terminator)
    {
      m_encoding = encoding;
      m_requestParser = requestParser;
    }

    /// <summary>
    /// Resolves the specified data to StringRequestInfo.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    protected override StringRequestInfo ProcessMatchedRequest(byte[] data, int offset, int length)
    {
      if (length == 0)
        return m_requestParser.ParseRequestInfo(string.Empty);

      return m_requestParser.ParseRequestInfo(m_encoding.GetString(data, offset, length));
    }
  }
}
