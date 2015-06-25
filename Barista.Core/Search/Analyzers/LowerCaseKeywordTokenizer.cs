namespace Barista.Search.Analyzers
{
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Tokenattributes;
  using Lucene.Net.Util;

  public sealed class LowerCaseKeywordTokenizer : Tokenizer
  {
    public LowerCaseKeywordTokenizer(System.IO.TextReader input)
      : base(input)
    {
      m_offsetAtt = AddAttribute<IOffsetAttribute>();
      m_termAtt = AddAttribute<ITermAttribute>();
    }

    public LowerCaseKeywordTokenizer(AttributeSource source, System.IO.TextReader input)
      : base(source, input)
    {
      m_offsetAtt = AddAttribute<IOffsetAttribute>();
      m_termAtt = AddAttribute<ITermAttribute>();
    }

    public LowerCaseKeywordTokenizer(AttributeFactory factory, System.IO.TextReader input)
      : base(factory, input)
    {
      m_offsetAtt = AddAttribute<IOffsetAttribute>();
      m_termAtt = AddAttribute<ITermAttribute>();
    }

    private int m_offset, m_bufferIndex, m_dataLen;
    private const int IOBufferSize = 4096;
    private readonly char[] m_ioBuffer = new char[IOBufferSize];

    private readonly ITermAttribute m_termAtt;
    private readonly IOffsetAttribute m_offsetAtt;

    /// <summary>Returns true iff a character should be included in a token.  This
    /// tokenizer generates as tokens adjacent sequences of characters which
    /// satisfy this predicate.  Characters for which this is false are used to
    /// define token boundaries and are not included in tokens. 
    /// </summary>
    internal bool IsTokenChar(char c)
    {
      return true;
    }

    /// <summary>Called on each token character to normalize it before it is added to the
    /// token.  The default implementation does nothing.  Subclasses may use this
    /// to, e.g., lowercase tokens. 
    /// </summary>
    internal char Normalize(char c)
    {
      return char.ToLowerInvariant(c);
    }

    public override bool IncrementToken()
    {
      ClearAttributes();
      int length = 0;
      int start = m_bufferIndex;
      char[] buffer = m_termAtt.TermBuffer();
      while (true)
      {

        if (m_bufferIndex >= m_dataLen)
        {
          m_offset += m_dataLen;
          m_dataLen = input.Read(m_ioBuffer, 0, m_ioBuffer.Length);
          if (m_dataLen <= 0)
          {
            m_dataLen = 0; // so next offset += dataLen won't decrement offset
            if (length > 0)
              break;
            return false;
          }
          m_bufferIndex = 0;
        }

        char c = m_ioBuffer[m_bufferIndex++];

        if (IsTokenChar(c))
        {
          // if it's a token char

          if (length == 0)
            // start of token
            start = m_offset + m_bufferIndex - 1;
          else if (length == buffer.Length)
            buffer = m_termAtt.ResizeTermBuffer(1 + length);

          buffer[length++] = Normalize(c); // buffer it, normalized
        }
        else if (length > 0)
          // at non-Letter w/ chars
          break; // return 'em
      }

      m_termAtt.SetTermLength(length);
      m_offsetAtt.SetOffset(CorrectOffset(start), CorrectOffset(start + length));
      return true;
    }

    public override void End()
    {
      // set final offset
      int finalOffset = CorrectOffset(m_offset);
      m_offsetAtt.SetOffset(finalOffset, finalOffset);
    }

    public override void Reset(System.IO.TextReader reader)
    {
      base.Reset(reader);
      m_bufferIndex = 0;
      m_offset = 0;
      m_dataLen = 0;
    }
  }
}
