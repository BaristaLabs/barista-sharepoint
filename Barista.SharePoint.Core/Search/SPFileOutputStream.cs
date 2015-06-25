namespace Barista.SharePoint.Search
{
  using System.IO;
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using System;
  using Microsoft.SharePoint.Utilities;
  using System.Threading;

  /// <summary>
  /// Represents an index output that uses a SPFile as a backing store.
  /// </summary>
  public sealed class SPFileOutputStream : IndexOutput
  {
    private readonly SPDirectory m_directory;
    private readonly IndexOutput m_indexOutput;
    private readonly string m_fileName;
    private readonly string m_fileUrl;

    private readonly Mutex m_mutex;

    public SPFileOutputStream(SPDirectory directory, SPFile file)
    {
      if (directory == null)
        throw new ArgumentNullException("directory");

      if (file == null)
        throw new ArgumentNullException("file");

      if (file.Exists == false)
        throw new ArgumentException("A file object was passed, but the file does not exist. " + file.Url);

      m_directory = directory;
      m_fileName = file.Name;
      m_fileUrl = SPUtility.ConcatUrls(file.Web.Url, file.ServerRelativeUrl);

      m_mutex = SPFileMutexManager.GrabMutex(m_fileUrl);
      m_mutex.WaitOne();
      try
      {
        m_indexOutput = m_directory.CacheDirectory.CreateOutput(file.Name);
      }
      finally
      {
        m_mutex.ReleaseMutex();
      }
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
      get { return m_indexOutput.FilePointer; }
    }

    /// <summary>
    /// The number of bytes in the file.
    /// </summary>
    /// <value>The length.</value>
    public override long Length
    {
      get { return m_indexOutput.Length; }
    }

    /// <summary>
    /// Forces any buffered output to be written.
    /// </summary>
    /// <exception cref="System.IO.FileNotFoundException">Unable to open SharePoint File: File does not exist.</exception>
    public override void Flush()
    {
      m_indexOutput.Flush();
    }

    /// <summary>
    /// Sets current position in this file, where the next write will occur.
    /// </summary>
    /// <param name="pos">The pos.</param>
    /// <seealso cref="P:Lucene.Net.Store.IndexOutput.FilePointer">
    ///   </seealso>
    public override void Seek(long pos)
    {
      m_indexOutput.Seek(pos);
    }

    /// <summary>
    /// Writes a single byte.
    /// </summary>
    /// <param name="b">The b.</param>
    /// <seealso cref="M:Lucene.Net.Store.IndexInput.ReadByte">
    ///   </seealso>
    public override void WriteByte(byte b)
    {
      m_indexOutput.WriteByte(b);
    }

    /// <summary>
    /// Writes the bytes.
    /// </summary>
    /// <param name="b">The b.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="len">The len.</param>
    public override void WriteBytes(byte[] b, int offset, int len)
    {
      m_indexOutput.WriteBytes(b, offset, len);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      m_mutex.WaitOne();
      try
      {
        m_indexOutput.Flush();
        m_indexOutput.Dispose();

        //Ship the file bytes to SharePoint.
        using (var fileStream = new StreamInput(m_directory.CacheDirectory.OpenInput(m_fileName)))
        {
          fileStream.Seek(0, SeekOrigin.Begin);

          //Create a new site/web since SharePoint access is inheritly single-threaded.
          using (var site = new SPSite(m_directory.Site.ID))
          {
            using (var web = site.OpenWeb())
            {
              var folder = web.GetFolder(m_fileUrl);
              var file = folder.Files.Add(m_fileUrl, fileStream, true);
              m_directory.WriteCachedFileETag(file.Name, file.ETag);
            }
          }
        }
      }
      finally
      {
        m_mutex.ReleaseMutex();
      }
    }
  }
}
