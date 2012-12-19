namespace Barista.SharePoint.Search
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using System;
  using System.IO;

  /// <summary>
  /// Represents an index input that uses a SPFile as a backing store.
  /// </summary>
  public sealed class SPFileInputStream : IndexInput
  {
    private readonly Guid m_siteId;
    private readonly Guid m_webId;
    private readonly Guid m_fileId;

    private MemoryStream m_data;
    private string m_eTag;

    public SPFileInputStream(Guid siteId, Guid webId, Guid fileId)
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
    /// Returns the current position in this file, where the next read will
    /// occur.
    /// </summary>
    /// <value>The file pointer.</value>
    /// <seealso cref="M:Lucene.Net.Store.IndexInput.Seek(System.Int64)">
    ///   </seealso>
    public override long FilePointer
    {
      get { return m_data.Position; }
    }


    /// <summary>
    /// The number of bytes in the file.
    /// </summary>
    /// <returns>System.Int64.</returns>
    public override long Length()
    {
      RefreshData();
      return m_data.Length;
    }

    /// <summary>
    /// Reads and returns a single byte.
    /// </summary>
    /// <returns>System.Byte.</returns>
    /// <seealso cref="M:Lucene.Net.Store.IndexOutput.WriteByte(System.Byte)">
    ///   </seealso>
    public override byte ReadByte()
    {
      RefreshData();
      return (byte) m_data.ReadByte();
    }

    /// <summary>
    /// Reads a specified number of bytes into an array at the specified offset.
    /// </summary>
    /// <param name="b">the array to read bytes into</param>
    /// <param name="offset">the offset in the array to start storing bytes</param>
    /// <param name="len">the number of bytes to read</param>
    /// <seealso cref="M:Lucene.Net.Store.IndexOutput.WriteBytes(System.Byte[],System.Int32)">
    ///   </seealso>
    public override void ReadBytes(byte[] b, int offset, int len)
    {
      RefreshData();
      m_data.Read(b, offset, len);
    }

    /// <summary>
    /// Sets current position in this file, where the next read will occur.
    /// </summary>
    /// <param name="pos">The pos.</param>
    /// <seealso cref="P:Lucene.Net.Store.IndexInput.FilePointer">
    ///   </seealso>
    public override void Seek(long pos)
    {
      RefreshData();
      m_data.Seek(pos, SeekOrigin.Begin);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      // Do Nothing...
    }

    /// <summary>
    /// Refreshes the data from the underlying SPFile if the e-tag has changed.
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
