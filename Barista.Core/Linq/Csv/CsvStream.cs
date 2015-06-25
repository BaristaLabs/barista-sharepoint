namespace Barista.DocumentStore.Linq.Csv
{
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Text;

  /// <summary>
  /// Based on code found at
  /// http://knab.ws/blog/index.php?/archives/3-CSV-file-parser-and-writer-in-C-Part-1.html
  /// and
  /// http://knab.ws/blog/index.php?/archives/10-CSV-file-parser-and-writer-in-C-Part-2.html
  /// </summary>
  internal class CsvStream
  {
    private readonly TextReader m_instream;
    private readonly TextWriter m_outStream;
    private readonly char m_separatorChar;
    private readonly char[] m_specialChars;

    // Current line number in the file. Only used when reading a file, not when writing a file.
    private int m_lineNbr;

    /// ///////////////////////////////////////////////////////////////////////
    /// CsvStream
    /// 
    public CsvStream(TextReader inStream, TextWriter outStream, char separatorChar)
    {
      m_instream = inStream;
      m_outStream = outStream;
      m_separatorChar = separatorChar;
      m_specialChars = ("\"\x0A\x0D" + m_separatorChar.ToString(CultureInfo.InvariantCulture)).ToCharArray();
      m_lineNbr = 1;
    }

    /// ///////////////////////////////////////////////////////////////////////
    /// WriteRow
    /// 
    public void WriteRow(List<string> row, bool quoteAllFields)
    {
      bool firstItem = true;
      foreach (string item in row)
      {
        if (!firstItem) { m_outStream.Write(m_separatorChar); }

        // If the item is null, don't write anything.
        if (item != null)
        {
          // If user always wants quoting, or if the item has special chars
          // (such as "), or if item is the empty string or consists solely of
          // white space, surround the item with quotes.

          if ((quoteAllFields ||
              (item.IndexOfAny(m_specialChars) > -1) ||
              (item.Trim() == "")))
          {
            m_outStream.Write("\"" + item.Replace("\"", "\"\"") + "\"");
          }
          else
          {
            m_outStream.Write(item);
          }
        }

        firstItem = false;
      }

      m_outStream.WriteLine("");
    }


    /// ///////////////////////////////////////////////////////////////////////
    /// ReadRow
    /// 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="row">
    /// Contains the values in the current row, in the order in which they 
    /// appear in the file.
    /// </param>
    /// <returns>
    /// True if a row was returned in parameter "row".
    /// False if no row returned. In that case, you're at the end of the file.
    /// </returns>
    public bool ReadRow(ref IDataRow row)
    {
      row.Clear();

      while (true)
      {
        // Number of the line where the item starts. Note that an item
        // can span multiple lines.
        int startingLineNbr = m_lineNbr;

        string item = null;

        bool moreAvailable = GetNextItem(ref item);
        if (!moreAvailable)
        {
          return (row.Count > 0);
        }
        row.Add(new DataRowItem(item, startingLineNbr));
      }
    }

    private bool m_eos;
    private bool m_eol;
    private bool m_previousWasCr;

    private bool GetNextItem(ref string itemString)
    {
      itemString = null;
      if (m_eol)
      {
        // previous item was last in line, start new line
        m_eol = false;
        return false;
      }

      bool itemFound = false;
      bool quoted = false;
      bool predata = true;
      bool postdata = false;
      StringBuilder item = new StringBuilder();

      while (true)
      {
        char c = GetNextChar(true);
        if (m_eos)
        {
          if (itemFound) { itemString = item.ToString(); }
          return itemFound;
        }

        // ---------
        // Keep track of line number. 
        // Note that line breaks can happen within a quoted field, not just at the
        // end of a record.

        // Don't count 0D0A as two line breaks.
        if ((!m_previousWasCr) && (c == '\x0A'))
        {
          m_lineNbr++;
        }

        if (c == '\x0D')
        {
          m_lineNbr++;
          m_previousWasCr = true;
        }
        else
        {
          m_previousWasCr = false;
        }

        // ---------

        if ((postdata || !quoted) && c == m_separatorChar)
        {
          // end of item, return
          if (itemFound) { itemString = item.ToString(); }
          return true;
        }

        if ((predata || postdata || !quoted) && (c == '\x0A' || c == '\x0D'))
        {
          // we are at the end of the line, eat newline characters and exit
          m_eol = true;
          if (c == '\x0D' && GetNextChar(false) == '\x0A')
          {
            // new line sequence is 0D0A
            GetNextChar(true);
          }

          if (itemFound) { itemString = item.ToString(); }
          return true;
        }

        if (predata && c == ' ')
          // whitespace preceeding data, discard
          continue;

        if (predata && c == '"')
        {
          // quoted data is starting
          quoted = true;
          predata = false;
          itemFound = true;
          continue;
        }

        if (predata)
        {
          // data is starting without quotes
          predata = false;
          item.Append(c);
          itemFound = true;
          continue;
        }

        if (c == '"' && quoted)
        {
          if (GetNextChar(false) == '"')
          {
            // double quotes within quoted string means add a quote       
            item.Append(GetNextChar(true));
          }
          else
          {
            // end-quote reached
            postdata = true;
          }

          continue;
        }

        // all cases covered, character must be data
        item.Append(c);
      }
    }

    private readonly char[] m_buffer = new char[4096];
    private int m_pos;
    private int m_length;

    private char GetNextChar(bool eat)
    {
      if (m_pos >= m_length)
      {
        m_length = m_instream.ReadBlock(m_buffer, 0, m_buffer.Length);
        if (m_length == 0)
        {
          m_eos = true;
          return '\0';
        }
        m_pos = 0;
      }
      if (eat)
        return m_buffer[m_pos++];

      return m_buffer[m_pos];
    }
  }
}
