namespace Barista.SharePoint.Search
{
  using System.Threading;
  using Barista.DocumentStore;
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using System;
  using Microsoft.SharePoint.Utilities;

  /// <summary>
  /// Represents an index input that uses a SPFile as a backing store.
  /// </summary>
  public sealed class SPFileInputStream : IndexInput
  {
    private readonly SPDirectory m_directory;
    private readonly IndexInput m_indexInput;

    private readonly Mutex m_mutex;

    public SPFileInputStream(SPDirectory directory, SPFile file)
    {
      if (directory == null)
        throw new ArgumentNullException("directory");

      if (file == null)
        throw new ArgumentNullException("file");

      var fileUrl = SPUtility.ConcatUrls(file.Web.Url, file.ServerRelativeUrl);

      m_directory = directory;

      m_mutex = SPFileMutexManager.GrabMutex(fileUrl);
      m_mutex.WaitOne();

      try
      {
        if (!m_directory.CacheDirectory.FileExists(file.Name))
        {
          CreateOrUpdateCachedFile(file, directory);
        }
        else
        {
          //Check the eTag.
          var cachedETag = m_directory.ReadCachedFileETag(file.Name);

          //If it doesn't match, re-retrieve the file from SharePoint.
          if (cachedETag != file.ETag)
            CreateOrUpdateCachedFile(file, directory);
        }

        m_indexInput = m_directory.CacheDirectory.OpenInput(file.Name);
      }
      finally
      {
        m_mutex.ReleaseMutex();
      }
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
      get { return m_indexInput.FilePointer; }
    }


    /// <summary>
    /// The number of bytes in the file.
    /// </summary>
    /// <returns>System.Int64.</returns>
    public override long Length()
    {
      return m_indexInput.Length();
    }

    /// <summary>
    /// Reads and returns a single byte.
    /// </summary>
    /// <returns>System.Byte.</returns>
    /// <seealso cref="M:Lucene.Net.Store.IndexOutput.WriteByte(System.Byte)">
    ///   </seealso>
    public override byte ReadByte()
    {
      return m_indexInput.ReadByte();
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
      m_indexInput.ReadBytes(b, offset, len);
    }

    /// <summary>
    /// Sets current position in this file, where the next read will occur.
    /// </summary>
    /// <param name="pos">The pos.</param>
    /// <seealso cref="P:Lucene.Net.Store.IndexInput.FilePointer">
    ///   </seealso>
    public override void Seek(long pos)
    {
      m_indexInput.Seek(pos);
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
        m_indexInput.Dispose();
      }
      finally
      {
        m_mutex.ReleaseMutex();
      }
    }

    private static void CreateOrUpdateCachedFile(SPFile spFile, SPDirectory directory)
    {
      using (var spFileStream = spFile.OpenBinaryStream())
      {
        using (var outputStream = directory.CreateCachedOutputAsStream(spFile.Name))
        {
          DocumentStoreHelper.CopyStream(spFileStream, outputStream);
          outputStream.Flush();
        }
      }

      directory.WriteCachedFileETag(spFile.Name, spFile.ETag);
    }
  }
}
