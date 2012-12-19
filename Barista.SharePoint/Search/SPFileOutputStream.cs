namespace Barista.SharePoint.Search
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using System;
  using System.IO;

  /// <summary>
  /// Represents an index output that uses a SPFile as a backing store.
  /// </summary>
  public sealed class SPFileOutputStream : IndexOutput
  {
    private readonly Guid m_siteId;
    private readonly Guid m_webId;
    private readonly Guid m_fileId;

    private MemoryStream m_data;
    private string m_eTag;

    public SPFileOutputStream(Guid siteId, Guid webId, Guid fileId)
    {
      if (siteId == Guid.Empty || siteId == default(Guid))
        throw new ArgumentNullException("siteId");

      if (webId == Guid.Empty || webId == default(Guid))
        throw new ArgumentNullException("webId");

      if (fileId == Guid.Empty || fileId == default(Guid))
        throw new ArgumentNullException("fileId");

      m_siteId = siteId;
      m_webId = webId;
      m_fileId = fileId;

      RefreshData();
    }

    /// <summary>
    /// Returns the current position in this file, where the next write will
    /// occur.
    /// </summary>
    /// <value>The file pointer.</value>
    /// <seealso cref="M:Lucene.Net.Store.IndexOutput.Seek(System.Int64)">
    ///   </seealso>
    public override long FilePointer
    {
      get { return m_data.Position; }
    }

    /// <summary>
    /// The number of bytes in the file.
    /// </summary>
    /// <value>The length.</value>
    public override long Length
    {
      get { return m_data.Length; }
    }

    /// <summary>
    /// Forces any buffered output to be written.
    /// </summary>
    /// <exception cref="System.IO.FileNotFoundException">Unable to open SharePoint File: File does not exist.</exception>
    public override void Flush()
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var file = web.GetFile(m_fileId);

          if (file.Exists == false)
            throw new FileNotFoundException("Unable to open SharePoint File: File does not exist.");

          var tempPosition = m_data.Position;
          m_data.Seek(0, SeekOrigin.Begin);

          file.SaveBinary(m_data, false, false, m_eTag, null, null, false, out m_eTag);
          m_data.Seek(tempPosition, SeekOrigin.Begin);
        }
      }
    }

    /// <summary>
    /// Sets current position in this file, where the next write will occur.
    /// </summary>
    /// <param name="pos">The pos.</param>
    /// <seealso cref="P:Lucene.Net.Store.IndexOutput.FilePointer">
    ///   </seealso>
    public override void Seek(long pos)
    {
      m_data.Seek(pos, SeekOrigin.Begin);
    }

    /// <summary>
    /// Writes a single byte.
    /// </summary>
    /// <param name="b">The b.</param>
    /// <seealso cref="M:Lucene.Net.Store.IndexInput.ReadByte">
    ///   </seealso>
    public override void WriteByte(byte b)
    {
      m_data.WriteByte(b);
    }

    /// <summary>
    /// Writes the bytes.
    /// </summary>
    /// <param name="b">The b.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="len">The len.</param>
    public override void WriteBytes(byte[] b, int offset, int len)
    {
      m_data.Write(b, offset, len);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;

      if (m_data == null)
        return;

      Flush();
      m_data.Dispose();
      m_data = null;
    }

    /// <summary>
    /// Refreshes in-memory data from the underlying SPFile if the e-tag has changed.
    /// </summary>
    /// <exception cref="System.IO.FileNotFoundException">Unable to open SharePoint File: File does not exist.</exception>
    private void RefreshData()
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var file = web.GetFile(m_fileId);

          if (file.Exists == false)
            throw new FileNotFoundException("Unable to open SharePoint File: File does not exist.");

          long currentPosition = 0;
          if (m_data != null)
            currentPosition = m_data.Position;

          if (file.ETag != m_eTag)
          {
            lock (file)
            {
              if (file.ETag != m_eTag)
              {
                var fileData = file.OpenBinary();
                m_data = new MemoryStream(fileData.Length);
                m_data.Write(fileData, 0, fileData.Length);
                m_data.Flush();

                m_eTag = file.ETag;
              }
            }
          }

          if (m_data != null && currentPosition != m_data.Position)
            m_data.Seek(currentPosition, SeekOrigin.Begin);
        }
      }
    }
  }
}
