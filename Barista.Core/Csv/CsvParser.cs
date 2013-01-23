namespace Barista.Csv
{
  using Barista.Extensions;
  using System;
  using System.Diagnostics;
  using System.IO;

  /// <summary>
  /// Implements the CSV parser.
  /// </summary>
  /// <remarks>
  /// This class implements CSV parsing capabilities.
  /// </remarks>
  internal sealed class CsvParser : IDisposable
  {
    private static readonly ExceptionHelper ExceptionHelper = new ExceptionHelper(typeof(CsvParser));

    /// <summary>
    /// The source of the CSV data.
    /// </summary>
    private readonly TextReader m_reader;

    /// <summary>
    /// See <see cref="PreserveLeadingWhiteSpace"/>.
    /// </summary>
    private bool m_preserveLeadingWhiteSpace;

    /// <summary>
    /// See <see cref="PreserveTrailingWhiteSpace"/>.
    /// </summary>
    private bool m_preserveTrailingWhiteSpace;

    /// <summary>
    /// See <see cref="ValueSeparator"/>.
    /// </summary>
    private char m_valueSeparator;

    /// <summary>
    /// See <see cref="ValueDelimiter"/>.
    /// </summary>
    private char m_valueDelimiter;

    /// <summary>
    /// Buffers CSV data.
    /// </summary>
    private readonly char[] m_buffer;

    /// <summary>
    /// The current index into <see cref="m_buffer"/>.
    /// </summary>
    private int m_bufferIndex;

    /// <summary>
    /// The last valid index into <see cref="m_buffer"/>.
    /// </summary>
    private int m_bufferEndIndex;

    /// <summary>
    /// The list of values currently parsed by the parser.
    /// </summary>
    private string[] m_valueList;

    /// <summary>
    /// The last valid index into <see cref="m_valueList"/>.
    /// </summary>
    public int ValuesListEndIndex;

    /// <summary>
    /// The buffer of characters containing the current value.
    /// </summary>
    private char[] m_valueBuffer;

    /// <summary>
    /// The last valid index into <see cref="m_valueBuffer"/>.
    /// </summary>
    private int m_valueBufferEndIndex;

    /// <summary>
    /// An index into <see cref="m_valueBuffer"/> indicating the first character that might be removed if it is leading white-space.
    /// </summary>
    private int m_valueBufferFirstEligibleLeadingWhiteSpace;

    /// <summary>
    /// An index into <see cref="m_valueBuffer"/> indicating the first character that might be removed if it is trailing white-space.
    /// </summary>
    private int m_valueBufferFirstEligibleTrailingWhiteSpace;

    /// <summary>
    /// <see langword="true"/> if the current value is delimited and the parser is in the delimited area.
    /// </summary>
    private bool m_inDelimitedArea;

    /// <summary>
    /// The starting index of the current value part.
    /// </summary>
    private int m_valuePartStartIndex;

    /// <summary>
    /// Set to <see langword="true"/> once the first record is passed (or the <see cref="CsvReader"/> decides that the first record has been passed.
    /// </summary>
    private bool m_passedFirstRecord;

    /// <summary>
    /// Used to quickly recognise whether a character is potentially special or not.
    /// </summary>
    private int m_specialCharacterMask;

    /// <summary>
    /// The space character.
    /// </summary>
    private const char Space = ' ';

    /// <summary>
    /// The tab character.
    /// </summary>
    private const char Tab = '\t';

    /// <summary>
    /// The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char Cr = (char)0x0d;

    /// <summary>
    /// The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char Lf = (char)0x0a;

    /// <summary>
    /// One char less than the size of the internal buffer. The extra char is used to support a faster peek operation.
    /// </summary>
    private const int BufferSize = 2047;

    /// <summary>
    /// The default value separator.
    /// </summary>
    public const char DefaultValueSeparator = ',';

    /// <summary>
    /// The default value delimiter.
    /// </summary>
    public const char DefaultValueDelimiter = '"';

    /// <summary>
    /// Gets or sets a value indicating whether leading whitespace is to be preserved.
    /// </summary>
    public bool PreserveLeadingWhiteSpace
    {
      get
      {
        return m_preserveLeadingWhiteSpace;
      }
      set
      {
        m_preserveLeadingWhiteSpace = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether trailing whitespace is to be preserved.
    /// </summary>
    public bool PreserveTrailingWhiteSpace
    {
      get
      {
        return m_preserveTrailingWhiteSpace;
      }
      set
      {
        m_preserveTrailingWhiteSpace = value;
      }
    }

    /// <summary>
    /// Gets or sets the character that separates values in the CSV data.
    /// </summary>
    public char ValueSeparator
    {
      get
      {
        return m_valueSeparator;
      }
      set
      {
        ExceptionHelper.ResolveAndThrowIf(value == m_valueDelimiter, "value-separator-same-as-value-delimiter");
        ExceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

        m_valueSeparator = value;
        UpdateSpecialCharacterMask();
      }
    }

    /// <summary>
    /// Gets or sets the character that optionally delimits values in the CSV data.
    /// </summary>
    public char ValueDelimiter
    {
      get
      {
        return m_valueDelimiter;
      }
      set
      {
        ExceptionHelper.ResolveAndThrowIf(value == m_valueSeparator, "value-separator-same-as-value-delimiter");
        ExceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

        m_valueDelimiter = value;
        UpdateSpecialCharacterMask();
      }
    }

    /// <summary>
    /// Gets a value indicating whether the parser's buffer contains more records in addition to those already parsed.
    /// </summary>
    public bool HasMoreRecords
    {
      get
      {
        if (m_bufferIndex < m_bufferEndIndex)
        {
          //the buffer isn't empty so there must be more records
          return true;
        }

        //the buffer is empty so peek into the reader to see whether there is more data
        return (m_reader.Peek() != -1);
      }
    }

    /// <summary>
    /// Gets a value indicating whether the parser has passed the first record in the input source.
    /// </summary>
    public bool PassedFirstRecord
    {
      get
      {
        return m_passedFirstRecord;
      }
      set
      {
        m_passedFirstRecord = value;
      }
    }

    /// <summary>
    /// Constructs and inititialises an instance of <c>CsvParser</c> with the details provided.
    /// </summary>
    /// <param name="reader">
    /// The instance of <see cref="TextReader"/> from which CSV data will be read.
    /// </param>
    public CsvParser(TextReader reader)
    {
      reader.AssertNotNull("reader");

      m_reader = reader;
      //the extra char is used to facilitate a faster peek operation
      m_buffer = new char[BufferSize + 1];
      m_valueList = new string[16];
      m_valueBuffer = new char[128];
      m_valuePartStartIndex = -1;
      //set defaults
      m_valueSeparator = DefaultValueSeparator;
      m_valueDelimiter = DefaultValueDelimiter;
      //get the default special character mask
      UpdateSpecialCharacterMask();
    }

    /// <summary>
    /// Efficiently skips the next CSV record.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a record was successfully skipped, otherwise <see langword="false"/>.
    /// </returns>
    public bool SkipRecord()
    {
      if (HasMoreRecords)
      {
        //taking a local copy allows optimisations that otherwise could not be performed because the CLR knows that no other thread
        //can touch our local reference
        char[] buffer = m_buffer;

        while (true)
        {
          if (m_bufferIndex != m_bufferEndIndex)
          {
            char c = buffer[m_bufferIndex++];

            if ((c & m_specialCharacterMask) == c)
            {
              if (!m_inDelimitedArea)
              {
                if (c == m_valueDelimiter)
                {
                  //found a delimiter so enter delimited area and set the start index for the value
                  m_inDelimitedArea = true;
                }
                else if (c == Cr)
                {
                  if (buffer[m_bufferIndex] == Lf)
                  {
                    SwallowChar();
                  }

                  return true;
                }
                else if (c == Lf)
                {
                  return true;
                }
              }
              else if (c == m_valueDelimiter)
              {
                if (buffer[m_bufferIndex] == m_valueDelimiter)
                {
                  // delimiter is escaped, so swallow the escape char
                  SwallowChar();
                }
                else
                {
                  //delimiter isn't escaped so we are no longer in a delimited area
                  m_inDelimitedArea = false;
                }
              }
            }
          }
          else if (!FillBufferIgnoreValues())
          {
            //data exhausted - get out of here
            return true;
          }
        }
      }

      //no more records - can't skip
      return false;
    }

    /// <summary>
    /// Reads and parses the CSV into a <c>string</c> array containing the values contained in a single CSV record.
    /// </summary>
    /// <returns>
    /// An array of field values for the record, or <see langword="null"/> if no record was found.
    /// </returns>
    public string[] ParseRecord()
    {
      ValuesListEndIndex = 0;
      char c = char.MinValue;
      //taking a local copy allows optimisations that otherwise could not be performed because the CLR knows that no other thread
      //can touch our local reference
      char[] buffer = m_buffer;

      while (true)
      {
        if (m_bufferIndex != m_bufferEndIndex)
        {
          if (m_valuePartStartIndex == -1)
          {
            m_valuePartStartIndex = m_bufferIndex;
          }

          c = buffer[m_bufferIndex++];

          if ((c & m_specialCharacterMask) == c)
          {
            if (!m_inDelimitedArea)
            {
              if (c == m_valueDelimiter)
              {
                //found a delimiter so enter delimited area and set the start index for the value
                m_inDelimitedArea = true;
                CloseValuePartExcludeCurrent();
                m_valuePartStartIndex = m_bufferIndex;

                //we have to make sure that delimited text isn't stripped if it is white-space
                if (m_valueBufferFirstEligibleLeadingWhiteSpace == 0)
                {
                  m_valueBufferFirstEligibleLeadingWhiteSpace = m_valueBufferEndIndex;
                }
              }
              else if (c == m_valueSeparator)
              {
                CloseValue(false);
              }
              else if (c == Cr)
              {
                CloseValue(false);

                if (buffer[m_bufferIndex] == Lf)
                {
                  SwallowChar();
                }

                break;
              }
              else if (c == Lf)
              {
                CloseValue(false);
                break;
              }
            }
            else if (c == m_valueDelimiter)
            {
              if (buffer[m_bufferIndex] == m_valueDelimiter)
              {
                CloseValuePart();
                SwallowChar();
                m_valuePartStartIndex = m_bufferIndex;
              }
              else
              {
                //delimiter isn't escaped so we are no longer in a delimited area
                m_inDelimitedArea = false;
                CloseValuePartExcludeCurrent();
                m_valuePartStartIndex = m_bufferIndex;
                //we have to make sure that delimited text isn't stripped if it is white-space
                m_valueBufferFirstEligibleTrailingWhiteSpace = m_valueBufferEndIndex;
              }
            }
          }
        }
        else if (!FillBuffer())
        {
          //special case: if the last character was a separator we need to add a blank value. eg. CSV "Value," will result in "Value", ""
          if (c == m_valueSeparator)
          {
            AddValue(string.Empty);
          }

          //data exhausted - get out of loop
          break;
        }
      }

      //this will return null if there are no values
      return GetValues();
    }

    /// <summary>
    /// Closes the current value part.
    /// </summary>
    private void CloseValuePart()
    {
      AppendToValue(m_valuePartStartIndex, m_bufferIndex);
      m_valuePartStartIndex = -1;
    }

    /// <summary>
    /// Closes the current value part, but excludes the current character from the value part.
    /// </summary>
    private void CloseValuePartExcludeCurrent()
    {
      AppendToValue(m_valuePartStartIndex, m_bufferIndex - 1);
    }

    /// <summary>
    /// Closes the current value by adding it to the list of values in the current record. Assumes that there is actually a value to add, either in <c>_value</c> or in
    /// <see cref="m_buffer"/> starting at <see cref="m_valuePartStartIndex"/> and ending at <see cref="m_bufferIndex"/>.
    /// </summary>
    /// <param name="includeCurrentChar">
    /// If <see langword="true"/>, the current character is included in the value. Otherwise, it is excluded.
    /// </param>
    private void CloseValue(bool includeCurrentChar)
    {
      int endIndex = m_bufferIndex;

      if ((!includeCurrentChar) && (endIndex > m_valuePartStartIndex))
      {
        endIndex -= 1;
      }

      Debug.Assert(m_valuePartStartIndex >= 0, "_valuePartStartIndex must be > 0");
      Debug.Assert(m_valuePartStartIndex <= m_bufferIndex, "_valuePartStartIndex must be less than or equal to _bufferIndex (" + m_valuePartStartIndex + " > " + m_bufferIndex + ")");
      Debug.Assert(m_valuePartStartIndex <= endIndex, "_valuePartStartIndex must be less than or equal to endIndex (" + m_valuePartStartIndex + " > " + endIndex + ")");

      if (m_valueBufferEndIndex == 0)
      {
        if (endIndex == 0)
        {
          AddValue(string.Empty);
        }
        else
        {
          //the value did not require the use of the ValueBuilder
          int startIndex = m_valuePartStartIndex;
          //taking a local copy allows optimisations that otherwise could not be performed because the CLR knows that no other thread
          //can touch our local reference
          char[] buffer = m_buffer;

          if (!m_preserveLeadingWhiteSpace)
          {
            //strip all leading white-space
            while ((startIndex < endIndex) && (IsWhiteSpace(buffer[startIndex])))
            {
              ++startIndex;
            }
          }

          if (!m_preserveTrailingWhiteSpace)
          {
            //strip all trailing white-space
            while ((endIndex > startIndex) && (IsWhiteSpace(buffer[endIndex - 1])))
            {
              --endIndex;
            }
          }

          AddValue(new string(buffer, startIndex, endIndex - startIndex));
        }
      }
      else
      {
        //we needed the ValueBuilder to compose the value
        AppendToValue(m_valuePartStartIndex, endIndex);

        if (!m_preserveLeadingWhiteSpace || !m_preserveTrailingWhiteSpace)
        {
          //strip all white-space prior to _valueBufferFirstEligibleLeadingWhiteSpace and after _valueBufferFirstEligibleTrailingWhiteSpace
          AddValue(GetValue(m_valueBufferFirstEligibleLeadingWhiteSpace, m_valueBufferFirstEligibleTrailingWhiteSpace));
        }
        else
        {
          AddValue(GetValue());
        }

        m_valueBufferEndIndex = 0;
        m_valueBufferFirstEligibleLeadingWhiteSpace = 0;
      }

      m_valuePartStartIndex = -1;
    }

    /// <summary>
    /// Determines whether <paramref name="c"/> is white-space.
    /// </summary>
    /// <param name="c">
    /// The character to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="c"/> is white-space, otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsWhiteSpace(char c)
    {
      return ((c == Space) || (c == Tab));
    }

    /// <summary>
    /// Fills that data buffer. Assumes that the buffer is already exhausted.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if data was read into the buffer, otherwise <see langword="false"/>.
    /// </returns>
    private bool FillBuffer()
    {
      Debug.Assert(m_bufferIndex == m_bufferEndIndex);

      //may need to close a value or value part depending on the state of the stream
      if (m_valuePartStartIndex != -1)
      {
        if (m_reader.Peek() != -1)
        {
          CloseValuePart();
          m_valuePartStartIndex = 0;
        }
        else
        {
          CloseValue(true);
        }
      }

      m_bufferEndIndex = m_reader.Read(m_buffer, 0, BufferSize);
      //this is possible because the buffer is one char bigger than BUFFER_SIZE. This fact is used to implement a faster peek operation
      m_buffer[m_bufferEndIndex] = (char)m_reader.Peek();
      m_bufferIndex = 0;
      m_passedFirstRecord = true;
      return (m_bufferEndIndex > 0);
    }

    /// <summary>
    /// Fills the buffer with data, but does not bother with closing values. This is used from the <see cref="SkipRecord"/> method,
    /// since that does not concern itself with values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if data was read into the buffer, otherwise <see langword="false"/>.
    /// </returns>
    private bool FillBufferIgnoreValues()
    {
      Debug.Assert(m_bufferIndex == m_bufferEndIndex);
      m_bufferEndIndex = m_reader.Read(m_buffer, 0, BufferSize);
      //this is possible because the buffer is one char bigger than BUFFER_SIZE. This fact is used to implement a faster peek operation
      m_buffer[m_bufferEndIndex] = (char)m_reader.Peek();
      m_bufferIndex = 0;
      m_passedFirstRecord = true;
      return (m_bufferEndIndex > 0);
    }


    /// <summary>
    /// Swallows the current character in the data buffer. Assumes that there is a character to swallow, but refills the buffer if necessary.
    /// </summary>
    private void SwallowChar()
    {
      if (m_bufferIndex < BufferSize)
      {
        //in this case there are still unread chars in the buffer so just skip one
        ++m_bufferIndex;
      }
      else if (m_bufferIndex < m_bufferEndIndex)
      {
        //in this case we are pointing to the second-to-last char in the buffer, so we need to refill the buffer since the last char is a peeked char
        FillBuffer();
      }
      else
      {
        //in this case we are pointing to the last char in the buffer, which is a peeked char. therefore, we need to refill and skip past that char
        FillBuffer();
        ++m_bufferIndex;
      }
    }

    /// <summary>
    /// Disposes of this <c>CsvParser</c> instance.
    /// </summary>
    void IDisposable.Dispose()
    {
      Close();
    }

    /// <summary>
    /// Closes this <c>CsvParser</c> instance and releases all resources acquired by it.
    /// </summary>
    public void Close()
    {
      if (m_reader != null)
      {
        m_reader.Close();
      }
    }

    /// <summary>
    /// Adds a value to the value list.
    /// </summary>
    /// <param name="val">
    /// The value to add.
    /// </param>
    private void AddValue(string val)
    {
      EnsureValueListCapacity();
      m_valueList[ValuesListEndIndex++] = val;
    }

    /// <summary>
    /// Gets an array of values that have been added to <see cref="m_valueList"/>.
    /// </summary>
    /// <returns>
    /// An array of type <c>string</c> containing all the values in the value list, or <see langword="null"/> if there are no values in the list.
    /// </returns>
    private string[] GetValues()
    {
      if (ValuesListEndIndex > 0)
      {
        string[] retVal = new string[ValuesListEndIndex];

        for (int i = 0; i < ValuesListEndIndex; ++i)
        {
          retVal[i] = m_valueList[i];
        }

        return retVal;
      }

      return null;
    }

    /// <summary>
    /// Ensures the value list contains enough space for another value, and increases its size if not.
    /// </summary>
    private void EnsureValueListCapacity()
    {
      if (ValuesListEndIndex == m_valueList.Length)
      {
        string[] newBuffer = new string[m_valueList.Length * 2];

        for (int i = 0; i < ValuesListEndIndex; ++i)
        {
          newBuffer[i] = m_valueList[i];
        }

        m_valueList = newBuffer;
      }
    }

    /// <summary>
    /// Appends the specified characters from <see cref="m_buffer"/> onto the end of the current value.
    /// </summary>
    /// <param name="startIndex">
    /// The index at which to begin copying.
    /// </param>
    /// <param name="endIndex">
    /// The index at which to cease copying. The character at this index is not copied.
    /// </param>
    private void AppendToValue(int startIndex, int endIndex)
    {
      EnsureValueBufferCapacity(endIndex - startIndex);
      char[] valueBuffer = m_valueBuffer;
      char[] buffer = m_buffer;

      //profiling revealed a loop to be faster than Array.Copy
      //in addition, profiling revealed that taking a local copy of the _buffer reference impedes performance here
      for (int i = startIndex; i < endIndex; ++i)
      {
        valueBuffer[m_valueBufferEndIndex++] = buffer[i];
      }
    }

    /// <summary>
    /// Gets the current value.
    /// </summary>
    /// <returns></returns>
    private string GetValue()
    {
      return new string(m_valueBuffer, 0, m_valueBufferEndIndex);
    }

    /// <summary>
    /// Gets the current value, optionally removing trailing white-space.
    /// </summary>
    /// <param name="valueBufferFirstEligibleLeadingWhiteSpace">
    /// The index of the first character that cannot possibly be leading white-space.
    /// </param>
    /// <param name="valueBufferFirstEligibleTrailingWhiteSpace">
    /// The index of the first character that may be trailing white-space.
    /// </param>
    /// <returns>
    /// An instance of <c>string</c> containing the resultant value.
    /// </returns>
    private string GetValue(int valueBufferFirstEligibleLeadingWhiteSpace, int valueBufferFirstEligibleTrailingWhiteSpace)
    {
      int startIndex = 0;
      int endIndex = m_valueBufferEndIndex - 1;

      if (!m_preserveLeadingWhiteSpace)
      {
        while ((startIndex < valueBufferFirstEligibleLeadingWhiteSpace) && (IsWhiteSpace(m_valueBuffer[startIndex])))
        {
          ++startIndex;
        }
      }

      if (!m_preserveTrailingWhiteSpace)
      {
        while ((endIndex >= valueBufferFirstEligibleTrailingWhiteSpace) && (IsWhiteSpace(m_valueBuffer[endIndex])))
        {
          --endIndex;
        }
      }

      return new string(m_valueBuffer, startIndex, endIndex - startIndex + 1);
    }

    /// <summary>
    /// Ensures the value buffer contains enough space for <paramref name="count"/> more characters.
    /// </summary>
    private void EnsureValueBufferCapacity(int count)
    {
      if ((m_valueBufferEndIndex + count) > m_valueBuffer.Length)
      {
        char[] newBuffer = new char[Math.Max(m_valueBuffer.Length * 2, (count >> 1) << 2)];

        //profiling revealed a loop to be faster than Array.Copy, despite Array.Copy having an internal implementation
        for (int i = 0; i < m_valueBufferEndIndex; ++i)
        {
          newBuffer[i] = m_valueBuffer[i];
        }

        m_valueBuffer = newBuffer;
      }
    }

    /// <summary>
    /// Updates the mask used to quickly filter out non-special characters.
    /// </summary>
    private void UpdateSpecialCharacterMask()
    {
      m_specialCharacterMask = m_valueSeparator | m_valueDelimiter | Cr | Lf;
    }
  }
}
