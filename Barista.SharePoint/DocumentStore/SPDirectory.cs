using System.Linq;

namespace Barista.SharePoint.DocumentStore
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using System.IO;
  using System;
  using Microsoft.SharePoint.Utilities;

  /// <summary>
  /// Class SPDirectory
  /// </summary>
  public sealed class SPDirectory : Lucene.Net.Store.Directory
  {
    private readonly Guid m_siteId;
    private readonly Guid m_webId;
    private readonly Guid m_folderId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SPDirectory" /> class.
    /// </summary>
    /// <param name="siteId">The site id.</param>
    /// <param name="webId">The web id.</param>
    /// <param name="folderId">The folder id.</param>
    /// <exception cref="System.ArgumentNullException">siteId</exception>
    public SPDirectory(Guid siteId, Guid webId, Guid folderId)
    {
      if (siteId == Guid.Empty || siteId == default(Guid))
        throw new ArgumentNullException("siteId");

      if (webId == Guid.Empty || webId == default(Guid))
        throw new ArgumentNullException("webId");

      if (folderId == Guid.Empty || folderId == default(Guid))
        throw new ArgumentNullException("folderId");

      m_siteId = siteId;
      m_webId = webId;
      m_folderId = folderId;
    }

    /// <summary>
    /// Creates a new, empty file in the directory with the given name.
    /// Returns a stream writing this file.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>IndexOutput.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public override IndexOutput CreateOutput(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          web.AllowUnsafeUpdates = true;
          try
          {
            var file = folder.Files.Add(name, new byte[0]);
            return new SPIndexOutput(m_siteId, m_webId, file.UniqueId);
          }
          finally
          {
            web.AllowUnsafeUpdates = false;
          }
        }
      }
    }

    public override void DeleteFile(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));
          web.AllowUnsafeUpdates = true;
          try
          {
            file.Delete();
          }
          finally
          {
            web.AllowUnsafeUpdates = false;
          }
        }
      }
    }

    public override bool FileExists(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));
          return file.Exists;
        }
      }
    }

    public override long FileLength(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new InvalidOperationException(String.Format("The specified file does not exist:{0} {1}", file.Url, name));

          return file.Length;
        }
      }
    }

    public override long FileModified(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new InvalidOperationException(String.Format("The specified file does not exist:{0} {1}", file.Url, name));

          var lastWriteTime = file.TimeLastModified;
          var universalTime = lastWriteTime.ToUniversalTime();
          var timeSpan = universalTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
          return (long)timeSpan.TotalMilliseconds;
        }
      }
    }

    public override string[] ListAll()
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          return folder.Files
            .OfType<SPFile>()
            .Select(f => f.Name)
            .ToArray();
        }
      }
    }

    public override IndexInput OpenInput(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));
          if (file.Exists == false)
            throw new InvalidOperationException(String.Format("The specified file does not exist:{0} {1}", file.Url, name));
          return new SPIndexInput(m_siteId, m_webId, file.UniqueId);
        }
      }
    }

    public override void TouchFile(string name)
    {
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));
          if (file.Exists == false)
            throw new InvalidOperationException(String.Format("The specified file does not exist:{0} {1}", file.Url, name));
          web.AllowUnsafeUpdates = true;
          try
          {
            file.Update();
          }
          finally
          {
            web.AllowUnsafeUpdates = false;
          }
        }
      }
    }

    protected override void Dispose(bool disposing)
    {
      //Surprisingly, nothing to dispose.
      //But... maybe it'll be more performant to just keep an instance of a SPWeb and dispose it here...
    }

    #region Nested Classes

    private class SPIndexInput : BufferedIndexInput
    {
      private readonly Guid m_siteId;
      private readonly Guid m_webId;
      private readonly Guid m_fileId;

      public SPIndexInput(Guid siteId, Guid webId, Guid fileId)
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
      }

      public override void ReadInternal(byte[] b, int offset, int length)
      {
        using (var site = new SPSite(m_siteId))
        {
          using (var web = site.OpenWeb(m_webId))
          {
            var file = web.GetFile(m_fileId);
            using (var s = file.OpenBinaryStream())
            {
              var reader = new BinaryReader(s);
              reader.Read(b, offset, length);
            }
          }
        }
      }

      public override long Length()
      {
        using (var site = new SPSite(m_siteId))
        {
          using (var web = site.OpenWeb(m_webId))
          {
            var file = web.GetFile(m_fileId);
            return file.Length;
          }
        }
      }

      public override void SeekInternal(long pos)
      {
        //Do Nothing.
      }

      protected override void Dispose(bool disposing)
      {
        //Surprisingly, nothing to dispose.
      }
    }

    private class SPIndexOutput : BufferedIndexOutput
    {
      private readonly Guid m_siteId;
      private readonly Guid m_webId;
      private readonly Guid m_fileId;

      public SPIndexOutput(Guid siteId, Guid webId, Guid fileId)
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
      }

      /// <summary>
      /// The number of bytes in the file.
      /// </summary>
      /// <value>The length.</value>
      public override long Length
      {
        get
        {
          using (var site = new SPSite(m_siteId))
          {
            using (var web = site.OpenWeb(m_webId))
            {
              var file = web.GetFile(m_fileId);
              return file.Length;
            }
          }
        }
      }

      /// <summary>
      /// Expert: implements buffer write.  Writes bytes at the current position in
      /// the output.
      /// </summary>
      /// <param name="b">the bytes to write</param>
      /// <param name="offset">the offset in the byte array</param>
      /// <param name="len">the number of bytes to write</param>
      public override void FlushBuffer(byte[] b, int offset, int len)
      {
        using (var site = new SPSite(m_siteId))
        {
          using (var web = site.OpenWeb(m_webId))
          {
            var file = web.GetFile(m_fileId);
            using (var s = file.OpenBinaryStream())
            {
              var writer = new BinaryWriter(s);
              writer.Write(b, offset, len);
            }
          }
        }
      }
    }

    #endregion
  }
}
