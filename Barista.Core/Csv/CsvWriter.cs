namespace Barista.Csv
{
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Text;

  /// <summary>
  /// Provides a mechanism via which CSV data can be easily written.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <c>CsvWriter</c> class allows CSV data to be written to any stream-based destination. By default, CSV values are separated by commas
  /// (<c>,</c>) and delimited by double quotes (<c>"</c>). If necessary, custom characters can be specified when creating the <c>CsvWriter</c>.
  /// </para>
  /// <para>
  /// The number of records that have been written so far is exposed via the <see cref="RecordNumber"/> property. Writing a header record does not
  /// increment this property.
  /// </para>
  /// <para>
  /// A CSV header record can be optionally written by the <c>CsvWriter</c>. If a header record is to be written, it must be done first thing with
  /// the <see cref="WriteHeaderRecord"/> method. If a header record is written, it is exposed via the <see cref="HeaderRecord"/> property.
  /// </para>
  /// <para>
  /// Data records can be written with the <see cref="WriteDataRecord"/> or <see cref="WriteDataRecords"/> methods. These methods are overloaded to
  /// accept either instances of <see cref="DataRecord"/> or an array of <c>string</c>s.
  /// </para>
  /// </remarks>
  /// <threadsafety>
  /// The <c>CsvWriter</c> class does not lock internally. Therefore, it is unsafe to share an instance across threads without implementing your own
  /// synchronisation solution.
  /// </threadsafety>
  /// <example>
  /// <para>
  /// The following example writes some simple CSV data to a file:
  /// </para>
  /// <para>
  /// <code lang="C#">
  /// <![CDATA[
  /// using (CsvWriter writer = new CsvWriter(@"C:\Temp\data.csv")) {
  ///		writer.WriteHeaderRecord("Name", "Age", "Gender");
  ///		writer.WriteDataRecord("Kent", 25, Gender.Male);
  ///		writer.WriteDataRecord("Belinda", 26, Gender.Female);
  ///		writer.WriteDataRecord("Tempany", 0, Gender.Female);
  /// }
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// <code lang="vb">
  /// <![CDATA[
  /// Dim writer As CsvWriter = Nothing
  /// 
  /// Try
  ///		writer = New CsvWriter("C:\Temp\data.csv")
  ///		writer.WriteHeaderRecord("Name", "Age", "Gender")
  ///		writer.WriteDataRecord("Kent", 25, Gender.Male)
  ///		writer.WriteDataRecord("Belinda", 26, Gender.Female)
  ///		writer.WriteDataRecord("Tempany", 0, Gender.Female)
  /// Finally
  ///		If (Not writer Is Nothing) Then
  ///			writer.Close()
  ///		End If
  /// End Try
  /// ]]>
  /// </code>
  /// </para>
  /// </example>
  /// <example>
  /// <para>
  /// The following example writes the contents of a <c>DataTable</c> to a <see cref="MemoryStream"/>. CSV values are separated by tabs and
  /// delimited by the pipe characters (<c>|</c>). Linux-style line breaks are written by the <c>CsvWriter</c>, regardless of the hosting platform:
  /// </para>
  /// <para>
  /// <code lang="C#">
  /// <![CDATA[
  /// DataTable table = GetDataTable();
  /// MemoryStream memStream = new MemoryStream();
  /// 
  /// using (CsvWriter writer = new CsvWriter(memStream)) {
  ///		writer.NewLine = "\r";
  ///		writer.ValueSeparator = '\t';
  ///		writer.ValueDelimiter = '|';
  ///		writer.WriteAll(table, true);
  /// }
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// <code lang="vb">
  /// <![CDATA[
  /// Dim table As DataTable = GetDataTable
  /// Dim memStream As MemoryStream = New MemoryStream
  /// Dim writer As CsvWriter = Nothing
  /// 
  /// Try
  ///		writer = New CsvWriter(memStream)
  ///		writer.NewLine = vbLf
  ///		writer.ValueSeparator = vbTab
  ///		writer.ValueDelimiter = "|"c
  ///		writer.WriteAll(table, True)
  /// Finally
  ///		If (Not writer Is Nothing) Then
  ///			writer.Close()
  ///		End If
  /// End Try
  /// ]]>
  /// </code>
  /// </para>
  /// </example>
  public class CsvWriter : IDisposable
  {
    private static readonly ExceptionHelper ExceptionHelper = new ExceptionHelper(typeof(CsvWriter));

    private static readonly Encoding DefaultEncoding = Encoding.Default;

    /// <summary>
    /// The <see cref="TextWriter"/> used to output CSV data.
    /// </summary>
    private readonly TextWriter m_writer;

    /// <summary>
    /// Set to <see langword="true"/> when this <c>CsvWriter</c> is disposed.
    /// </summary>
    private bool m_disposed;

    /// <summary>
    /// See <see cref="HeaderRecord"/>.
    /// </summary>
    private HeaderRecord m_headerRecord;

    /// <summary>
    /// See <see cref="AlwaysDelimit"/>.
    /// </summary>
    private bool m_alwaysDelimit;

    /// <summary>
    /// The buffer of characters containing the current value.
    /// </summary>
    private char[] m_valueBuffer;

    /// <summary>
    /// The last valid index into <see cref="m_valueBuffer"/>.
    /// </summary>
    private int m_valueBufferEndIndex;

    /// <summary>
    /// See <see cref="ValueSeparator"/>.
    /// </summary>
    private char m_valueSeparator;

    /// <summary>
    /// See <see cref="ValueDelimiter"/>.
    /// </summary>
    private char m_valueDelimiter;

    /// <summary>
    /// See <see cref="RecordNumber"/>.
    /// </summary>
    private long m_recordNumber;

    /// <summary>
    /// Set to <see langword="true"/> once the first record is written.
    /// </summary>
    private bool m_passedFirstRecord;

    /// <summary>
    /// The space character.
    /// </summary>
    private const char Space = ' ';

    /// <summary>
    /// The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char Cr = (char)0x0d;

    /// <summary>
    /// The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char Lf = (char)0x0a;

    /// <summary>
    /// Gets the encoding of the underlying writer for this <c>CsvWriter</c>.
    /// </summary>
    public Encoding Encoding
    {
      get
      {
        EnsureNotDisposed();
        return m_writer.Encoding;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether values should always be delimited.
    /// </summary>
    /// <remarks>
    /// By default the <c>CsvWriter</c> will only delimit values that require delimiting. Setting this property to <c>true</c> will ensure that all written values are
    /// delimited.
    /// </remarks>
    public bool AlwaysDelimit
    {
      get
      {
        EnsureNotDisposed();
        return m_alwaysDelimit;
      }
      set
      {
        EnsureNotDisposed();
        m_alwaysDelimit = value;
      }
    }

    /// <summary>
    /// Gets or sets the character placed between values in the CSV data.
    /// </summary>
    /// <remarks>
    /// This property retrieves the character that this <c>CsvWriter</c> will use to separate distinct values in the CSV data. The default value
    /// of this property is a comma (<c>,</c>).
    /// </remarks>
    public char ValueSeparator
    {
      get
      {
        EnsureNotDisposed();
        return m_valueSeparator;
      }
      set
      {
        EnsureNotDisposed();
        ExceptionHelper.ResolveAndThrowIf(value == m_valueDelimiter, "value-separator-same-as-value-delimiter");
        ExceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

        m_valueSeparator = value;
      }
    }

    /// <summary>
    /// Gets the character possibly placed around values in the CSV data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property retrieves the character that this <c>CsvWriter</c> will use to demarcate values in the CSV data. The default value of this
    /// property is a double quote (<c>"</c>).
    /// </para>
    /// <para>
    /// If <see cref="AlwaysDelimit"/> is <c>true</c>, then values written by this <c>CsvWriter</c> will always be delimited with this character. Otherwise, the
    /// <c>CsvWriter</c> will decide whether values must be delimited based on their content.
    /// </para>
    /// </remarks>
    public char ValueDelimiter
    {
      get
      {
        EnsureNotDisposed();
        return m_valueDelimiter;
      }
      set
      {
        EnsureNotDisposed();
        ExceptionHelper.ResolveAndThrowIf(value == m_valueSeparator, "value-separator-same-as-value-delimiter");
        ExceptionHelper.ResolveAndThrowIf(value == Space, "value-separator-or-value-delimiter-space");

        m_valueDelimiter = value;
      }
    }

    /// <summary>
    /// Gets or sets the line terminator for this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This property gets or sets the line terminator for the underlying <c>TextWriter</c> used by this <c>CsvWriter</c>. If this is set to <see langword="null"/> the
    /// default newline string is used instead.
    /// </remarks>
    public string NewLine
    {
      get
      {
        EnsureNotDisposed();
        return m_writer.NewLine;
      }
      set
      {
        EnsureNotDisposed();
        m_writer.NewLine = value;
      }
    }

    /// <summary>
    /// Gets the CSV header for this writer.
    /// </summary>
    /// <value>
    /// The CSV header record for this writer, or <see langword="null"/> if no header record applies.
    /// </value>
    /// <remarks>
    /// This property can be used to retrieve the <see cref="HeaderRecord"/> that represents the header record for this <c>CsvWriter</c>. If a
    /// header record has been written (using, for example, <see cref="WriteHeaderRecord"/>) then this property will retrieve the details of the
    /// header record. If a header record has not been written, this property will return <see langword="null"/>.
    /// </remarks>
    public HeaderRecord HeaderRecord
    {
      get
      {
        EnsureNotDisposed();
        return m_headerRecord;
      }
    }

    /// <summary>
    /// Gets the current record number.
    /// </summary>
    /// <remarks>
    /// This property gives the number of records that the <c>CsvWriter</c> has written. The CSV header does not count. That is, calling
    /// <see cref="WriteHeaderRecord"/> will not increment this property. Only successful calls to <see cref="WriteDataRecord"/> (and related methods)
    /// will increment this property.
    /// </remarks>
    public long RecordNumber
    {
      get
      {
        EnsureNotDisposed();
        return m_recordNumber;
      }
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="stream">
    /// The stream to which CSV data will be written.
    /// </param>
    public CsvWriter(Stream stream)
      : this(stream, DefaultEncoding)
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="stream">
    /// The stream to which CSV data will be written.
    /// </param>
    /// <param name="encoding">
    /// The encoding for the data in <paramref name="stream"/>.
    /// </param>
    public CsvWriter(Stream stream, Encoding encoding)
      : this(new StreamWriter(stream, encoding))
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="path">
    /// The full path to the file to which CSV data will be written.
    /// </param>
    public CsvWriter(string path)
      : this(path, false, DefaultEncoding)
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="path">
    /// The full path to the file to which CSV data will be written.
    /// </param>
    /// <param name="encoding">
    /// The encoding for the data in <paramref name="path"/>.
    /// </param>
    public CsvWriter(string path, Encoding encoding)
      : this(path, false, encoding)
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="path">
    /// The full path to the file to which CSV data will be written.
    /// </param>
    /// <param name="append">
    /// If <c>true</c>, data will be appended to the specified file.
    /// </param>
    public CsvWriter(string path, bool append)
      : this(path, append, DefaultEncoding)
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <remarks>
    /// If the specified file already exists, it will be overwritten.
    /// </remarks>
    /// <param name="path">
    /// The full path to the file to which CSV data will be written.
    /// </param>
    /// <param name="append">
    /// If <c>true</c>, data will be appended to the specified file.
    /// </param>
    /// <param name="encoding">
    /// The encoding for the data in <paramref name="path"/>.
    /// </param>
    public CsvWriter(string path, bool append, Encoding encoding)
      : this(new StreamWriter(path, append, encoding))
    {
    }

    /// <summary>
    /// Constructs and initialises an instance of <c>CsvWriter</c> based on the information provided.
    /// </summary>
    /// <param name="writer">
    /// The <see cref="TextWriter"/> to which CSV data will be written.
    /// </param>
    public CsvWriter(TextWriter writer)
    {
      writer.AssertNotNull("writer");

      m_writer = writer;
      m_valueSeparator = CsvParser.DefaultValueSeparator;
      m_valueDelimiter = CsvParser.DefaultValueDelimiter;
      m_valueBuffer = new char[128];
    }

    /// <summary>
    /// Disposes of this <c>CsvWriter</c> instance.
    /// </summary>
    void IDisposable.Dispose()
    {
      Close();
      Dispose(true);
    }

    /// <summary>
    /// Allows sub classes to implement disposing logic.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> if this method is being called in response to a <see cref="Dispose"/> call, or <see langword="false"/> if
    /// it is being called during finalization.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Closes this <c>CsvWriter</c> instance and releases all resources acquired by it.
    /// </summary>
    /// <remarks>
    /// Once an instance of <c>CsvWriter</c> is no longer needed, call this method to immediately release any resources. Closing a <c>CsvWriter</c> is equivalent to
    /// disposing of it in a C# <c>using</c> block.
    /// </remarks>
    public void Close()
    {
      if (m_writer != null)
      {
        m_writer.Close();
      }

      m_disposed = true;
    }

    /// <summary>
    /// Flushes the underlying buffer of this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method can be used to flush the underlying <c>Stream</c> that this <c>CsvWriter</c> writes to.
    /// </remarks>
    public void Flush()
    {
      EnsureNotDisposed();
      m_writer.Flush();
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(HeaderRecord headerRecord)
    {
      headerRecord.AssertNotNull("headerRecord");
      ExceptionHelper.ResolveAndThrowIf(m_passedFirstRecord, "WriteHeaderRecord.passed-first-record");
      WriteHeaderRecord(headerRecord.Values);
    }

    /// <summary>
    /// Writes a data record with the specified values.
    /// </summary>
    /// <remarks>
    /// Each item in <paramref name="headerRecord"/> is converted to a <c>string</c> via its <c>ToString</c> implementation. If any item is <see langword="null"/>, it is substituted
    /// for an empty <c>string</c> (<see cref="string.Empty"/>).
    /// </remarks>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(params object[] headerRecord)
    {
      WriteHeaderRecord(CultureInfo.CurrentCulture, headerRecord);
    }

    /// <summary>
    /// Writes a data record with the specified values.
    /// </summary>
    /// <remarks>
    /// Each item in <paramref name="headerRecord"/> is converted to a <c>string</c> via its <c>ToString</c> implementation. If any item is <see langword="null"/>, it is substituted
    /// for an empty <c>string</c> (<see cref="string.Empty"/>).
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any items in the data record that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(IFormatProvider provider, params object[] headerRecord)
    {
      WriteHeaderRecord(provider, (IEnumerable<object>)headerRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(IEnumerable<object> headerRecord)
    {
      WriteHeaderRecord(CultureInfo.CurrentCulture, headerRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any items in the data record that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(IFormatProvider provider, IEnumerable<object> headerRecord)
    {
      var headerRecords = headerRecord as IList<object> ?? headerRecord.ToList();
      headerRecords.AssertNotNull("dataRecord");

      var dataRecordAsStrings = headerRecords.Select(x => ConvertItemToString(provider, x));
      WriteHeaderRecord(dataRecordAsStrings);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(params string[] headerRecord)
    {
      WriteHeaderRecord((IEnumerable<string>)headerRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="headerRecord">
    /// The record to be written.
    /// </param>
    public void WriteHeaderRecord(IEnumerable<string> headerRecord)
    {
      EnsureNotDisposed();
      var headerRecords = headerRecord as IList<string> ?? headerRecord.ToList();
      headerRecords.AssertNotNull("headerRecord");
      ExceptionHelper.ResolveAndThrowIf(m_passedFirstRecord, "WriteHeaderRecord.passed-first-record");
      m_headerRecord = new HeaderRecord(headerRecords, true);
      WriteRecord(headerRecords, false);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(DataRecord dataRecord)
    {
      dataRecord.AssertNotNull("dataRecord");
      WriteDataRecord(dataRecord.Values);
    }

    /// <summary>
    /// Writes a data record with the specified values.
    /// </summary>
    /// <remarks>
    /// Each item in <paramref name="dataRecord"/> is converted to a <c>string</c> via its <c>ToString</c> implementation. If any item is <see langword="null"/>, it is substituted
    /// for an empty <c>string</c> (<see cref="string.Empty"/>).
    /// </remarks>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(params object[] dataRecord)
    {
      WriteDataRecord(CultureInfo.CurrentCulture, dataRecord);
    }

    /// <summary>
    /// Writes a data record with the specified values.
    /// </summary>
    /// <remarks>
    /// Each item in <paramref name="dataRecord"/> is converted to a <c>string</c> via its <c>ToString</c> implementation. If any item is <see langword="null"/>, it is substituted
    /// for an empty <c>string</c> (<see cref="string.Empty"/>).
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any items in the data record that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(IFormatProvider provider, params object[] dataRecord)
    {
      WriteDataRecord(provider, (IEnumerable<object>)dataRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(IEnumerable<object> dataRecord)
    {
      WriteDataRecord(CultureInfo.CurrentCulture, dataRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any items in the data record that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(IFormatProvider provider, IEnumerable<object> dataRecord)
    {
      var dataRecords = dataRecord as IList<object> ?? dataRecord.ToList();
      dataRecords.AssertNotNull("dataRecord");

      var dataRecordAsStrings = dataRecords.Select(x => ConvertItemToString(provider, x));
      WriteDataRecord(dataRecordAsStrings);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(params string[] dataRecord)
    {
      WriteDataRecord((IEnumerable<string>)dataRecord);
    }

    /// <summary>
    /// Writes the specified record to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes a single data record to this <c>CsvWriter</c>. The <see cref="RecordNumber"/> property is incremented upon successfully writing
    /// the record.
    /// </remarks>
    /// <param name="dataRecord">
    /// The record to be written.
    /// </param>
    public void WriteDataRecord(IEnumerable<string> dataRecord)
    {
      EnsureNotDisposed();
      var dataRecords = dataRecord as IList<string> ?? dataRecord.ToList();
      dataRecords.AssertNotNull("dataRecord");

      WriteRecord(dataRecords, true);
    }

    /// <summary>
    /// Writes all records specified by <paramref name="dataRecords"/> to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes all data records in <paramref name="dataRecords"/> to this <c>CsvWriter</c> and increments the <see cref="RecordNumber"/> property
    /// as records are written.
    /// </remarks>
    /// <param name="dataRecords">
    /// The records to be written.
    /// </param>
    public void WriteDataRecords(ICollection<DataRecord> dataRecords)
    {
      EnsureNotDisposed();
      dataRecords.AssertNotNull("dataRecords");

      foreach (DataRecord dataRecord in dataRecords)
      {
        WriteRecord(dataRecord.Values, true);
      }
    }

    /// <summary>
    /// Writes all records specified by <paramref name="dataRecords"/> to this <c>CsvWriter</c>.
    /// </summary>
    /// <remarks>
    /// This method writes all data records in <paramref name="dataRecords"/> to this <c>CsvWriter</c> and increments the <see cref="RecordNumber"/> property
    /// as records are written.
    /// </remarks>
    /// <param name="dataRecords">
    /// The records to be written.
    /// </param>
    public void WriteDataRecords(ICollection<string[]> dataRecords)
    {
      EnsureNotDisposed();
      dataRecords.AssertNotNull("dataRecords");

      foreach (string[] dataRecord in dataRecords)
      {
        WriteRecord(dataRecord, true);
      }
    }

    /// <summary>
    /// Writes the data in <paramref name="table"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in <paramref name="table"/> to this <c>CsvWriter</c>, including a header record. If a header record has already
    /// been written to this <c>CsvWriter</c> this method will throw an exception. That being the case, you should use <see cref="WriteAll(DataTable, bool)"/>
    /// instead, specifying <see langword="false"/> for the second parameter.
    /// </remarks>
    /// <param name="table">
    /// The <c>DataTable</c> whose data is to be written as CSV data.
    /// </param>
    public void WriteAll(DataTable table)
    {
      WriteAll(CultureInfo.CurrentCulture, table);
    }

    /// <summary>
    /// Writes the data in <paramref name="table"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in <paramref name="table"/> to this <c>CsvWriter</c>, including a header record. If a header record has already
    /// been written to this <c>CsvWriter</c> this method will throw an exception. That being the case, you should use <see cref="WriteAll(DataTable, bool)"/>
    /// instead, specifying <see langword="false"/> for the second parameter.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any values in the data table that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="table">
    /// The <c>DataTable</c> whose data is to be written as CSV data.
    /// </param>
    public void WriteAll(IFormatProvider provider, DataTable table)
    {
      WriteAll(provider, table, true);
    }

    /// <summary>
    /// Writes the data in <paramref name="table"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in <paramref name="table"/> to this <c>CsvWriter</c>, optionally writing a header record based on the columns in the
    /// table.
    /// </remarks>
    /// <param name="table">
    /// The <c>DataTable</c> whose data is to be written as CSV data.
    /// </param>
    /// <param name="writeHeaderRecord">
    /// If <see langword="true"/>, a CSV header will be written based on the column names for the table.
    /// </param>
    public void WriteAll(DataTable table, bool writeHeaderRecord)
    {
      WriteAll(CultureInfo.CurrentCulture, table, writeHeaderRecord);
    }

    /// <summary>
    /// Writes the data in <paramref name="table"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in <paramref name="table"/> to this <c>CsvWriter</c>, optionally writing a header record based on the columns in the
    /// table.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any items in the data table that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="table">
    /// The <c>DataTable</c> whose data is to be written as CSV data.
    /// </param>
    /// <param name="writeHeaderRecord">
    /// If <see langword="true"/>, a CSV header will be written based on the column names for the table.
    /// </param>
    public void WriteAll(IFormatProvider provider, DataTable table, bool writeHeaderRecord)
    {
      EnsureNotDisposed();
      table.AssertNotNull("table");

      if (writeHeaderRecord)
      {
        HeaderRecord headerRecord = new HeaderRecord();

        foreach (DataColumn column in table.Columns)
        {
          headerRecord.Values.Add(column.ColumnName);
        }

        WriteHeaderRecord(headerRecord);
      }

      foreach (DataRow row in table.Rows)
      {
        DataRecord dataRecord = new DataRecord(m_headerRecord);

        foreach (object item in row.ItemArray)
        {
          dataRecord.Values.Add(ConvertItemToString(provider, item));
        }

        WriteDataRecord(dataRecord);
      }
    }

    /// <summary>
    /// Writes the first <see cref="DataTable"/> in <paramref name="dataSet"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in the first table of <paramref name="dataSet"/> to this <c>CsvWriter</c>, including a header record.
    /// If a header record has already been written to this <c>CsvWriter</c> this method will throw an exception. That being the case, you
    /// should use <see cref="WriteAll(DataSet, bool)"/> instead, specifying <see langword="false"/> for the second parameter.
    /// </remarks>
    /// <param name="dataSet">
    /// The <c>DataSet</c> whose first table is to be written as CSV data.
    /// </param>
    public void WriteAll(DataSet dataSet)
    {
      WriteAll(CultureInfo.CurrentCulture, dataSet);
    }

    /// <summary>
    /// Writes the first <see cref="DataTable"/> in <paramref name="dataSet"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in the first table of <paramref name="dataSet"/> to this <c>CsvWriter</c>, including a header record.
    /// If a header record has already been written to this <c>CsvWriter</c> this method will throw an exception. That being the case, you
    /// should use <see cref="WriteAll(DataSet, bool)"/> instead, specifying <see langword="false"/> for the second parameter.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any values in the data set that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="dataSet">
    /// The <c>DataSet</c> whose first table is to be written as CSV data.
    /// </param>
    public void WriteAll(IFormatProvider provider, DataSet dataSet)
    {
      WriteAll(provider, dataSet, true);
    }

    /// <summary>
    /// Writes the first <see cref="DataTable"/> in <paramref name="dataSet"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in the first table of <paramref name="dataSet"/> to this <c>CsvWriter</c>, optionally writing a header
    /// record based on the columns in the table.
    /// </remarks>
    /// <param name="dataSet">
    /// The <c>DataSet</c> whose first table is to be written as CSV data.
    /// </param>
    /// <param name="writeHeaderRecord">
    /// If <see langword="true"/>, a CSV header will be written based on the column names for the table.
    /// </param>
    public void WriteAll(DataSet dataSet, bool writeHeaderRecord)
    {
      WriteAll(CultureInfo.CurrentCulture, dataSet, writeHeaderRecord);
    }

    /// <summary>
    /// Writes the first <see cref="DataTable"/> in <paramref name="dataSet"/> as CSV data.
    /// </summary>
    /// <remarks>
    /// This method writes all the data in the first table of <paramref name="dataSet"/> to this <c>CsvWriter</c>, optionally writing a header
    /// record based on the columns in the table.
    /// </remarks>
    /// <param name="provider">
    /// The format provider to use for any values in the data set that implement <see cref="IConvertible"/>.
    /// </param>
    /// <param name="dataSet">
    /// The <c>DataSet</c> whose first table is to be written as CSV data.
    /// </param>
    /// <param name="writeHeaderRecord">
    /// If <see langword="true"/>, a CSV header will be written based on the column names for the table.
    /// </param>
    public void WriteAll(IFormatProvider provider, DataSet dataSet, bool writeHeaderRecord)
    {
      EnsureNotDisposed();
      dataSet.AssertNotNull("dataSet");
      ExceptionHelper.ResolveAndThrowIf(dataSet.Tables.Count == 0, "WriteAll.dataSet-no-table");

      WriteAll(provider, dataSet.Tables[0], writeHeaderRecord);
    }

    /// <summary>
    /// Writes the specified record to the target <see cref="TextWriter"/>, ensuring all values are correctly separated and escaped.
    /// </summary>
    /// <remarks>
    /// This method is used internally by the <c>CsvWriter</c> to write CSV records.
    /// </remarks>
    /// <param name="record">
    /// The record to be written.
    /// </param>
    /// <param name="incrementRecordNumber">
    /// <see langword="true"/> if the record number should be incremented, otherwise <see langword="false"/>.
    /// </param>
    private void WriteRecord(IEnumerable<string> record, bool incrementRecordNumber)
    {
      bool firstValue = true;

      foreach (string value in record)
      {
        if (!firstValue)
        {
          m_writer.Write(m_valueSeparator);
        }
        else
        {
          firstValue = false;
        }

        WriteValue(value);
      }

      //uses the underlying TextWriter.NewLine property
      m_writer.WriteLine();
      m_passedFirstRecord = true;

      if (incrementRecordNumber)
      {
        ++m_recordNumber;
      }
    }

    /// <summary>
    /// Writes the specified value to the target <see cref="TextWriter"/>, ensuring it is correctly escaped.
    /// </summary>
    /// <remarks>
    /// This method is used internally by the <c>CsvWriter</c> to write individual CSV values.
    /// </remarks>
    /// <param name="val">
    /// The value to be written.
    /// </param>
    private void WriteValue(string val)
    {
      m_valueBufferEndIndex = 0;
      bool delimit = m_alwaysDelimit;

      if (!string.IsNullOrEmpty(val))
      {
        //delimit to preserve white-space at the beginning or end of the value
        if ((val[0] == Space) || (val[val.Length - 1] == Space))
        {
          delimit = true;
        }

        foreach (char c in val)
        {
          if ((c == m_valueSeparator) || (c == Cr) || (c == Lf))
          {
            //all these characters require the value to be delimited
            AppendToValue(c);
            delimit = true;
          }
          else if (c == m_valueDelimiter)
          {
            //escape the delimiter by writing it twice
            AppendToValue(m_valueDelimiter);
            AppendToValue(m_valueDelimiter);
            delimit = true;
          }
          else
          {
            AppendToValue(c);
          }
        }
      }

      if (delimit)
      {
        m_writer.Write(m_valueDelimiter);
      }

      //write the value
      m_writer.Write(m_valueBuffer, 0, m_valueBufferEndIndex);

      if (delimit)
      {
        m_writer.Write(m_valueDelimiter);
      }

      m_valueBufferEndIndex = 0;
    }

    /// <summary>
    /// Appends the specified character onto the end of the current value.
    /// </summary>
    /// <param name="c">
    /// The character to append.
    /// </param>
    private void AppendToValue(char c)
    {
      EnsureValueBufferCapacity(1);
      m_valueBuffer[m_valueBufferEndIndex++] = c;
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
    /// Makes sure the object isn't disposed and, if so, throws an exception.
    /// </summary>
    private void EnsureNotDisposed()
    {
      ExceptionHelper.ResolveAndThrowIf(m_disposed, "disposed");
    }

    /// <summary>
    /// Converts an item to a string given an <see cref="IFormatProvider"/>.
    /// </summary>
    private static string ConvertItemToString(IFormatProvider provider, object item)
    {
      if (item == null)
      {
        return string.Empty;
      }

      var convertible = item as IConvertible;

      if (convertible == null)
      {
        return item.ToString();
      }

      return convertible.ToString(provider);
    }
  }
}