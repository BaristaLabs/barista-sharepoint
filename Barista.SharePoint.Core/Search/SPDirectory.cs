namespace Barista.SharePoint.Search
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.IO;
  using System.Linq;

  /// <summary>
  /// Class SPDirectory
  /// </summary>
  public sealed class SPDirectory : Lucene.Net.Store.Directory
  {
    private readonly string m_folderUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="SPDirectory" /> class.
    /// </summary>
    /// <param name="folder">The folder.</param>
    public SPDirectory(SPFolder folder)
      : this(folder, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SPDirectory" /> class.
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <param name="lockFactory">The lock factory.</param>
    /// <exception cref="System.ArgumentNullException">folder</exception>
    public SPDirectory(SPFolder folder, LockFactory lockFactory)
    {
      if (folder == null)
        throw new ArgumentNullException("folder");

      m_folderUrl = SPUtility.ConcatUrls(folder.ParentWeb.Url, folder.ServerRelativeUrl);

      if (lockFactory == null)
        lockFactory = new SPLockFactory(m_folderUrl);

      SetLockFactory(lockFactory);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SPDirectory" /> class.
    /// </summary>
    /// <param name="folderUrl">The folder URL.</param>
    public SPDirectory(string folderUrl)
      : this(folderUrl, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SPDirectory" /> class.
    /// </summary>
    /// <param name="folderUrl"></param>
    /// <param name="lockFactory"></param>
    /// <exception cref="System.ArgumentNullException">siteId</exception>
    public SPDirectory(string folderUrl, LockFactory lockFactory)
    {
      if (String.IsNullOrEmpty(folderUrl))
        throw new ArgumentNullException("folderUrl");

      m_folderUrl = folderUrl;

      if (lockFactory == null)
        lockFactory = new SPLockFactory(m_folderUrl);

      SetLockFactory(lockFactory);
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
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          web.AllowUnsafeUpdates = true;
          try
          {
            var file = folder.Files.Add(name, new byte[0], true);

            return new SPFileOutputStream(SPUtility.ConcatUrls(file.Web.Url, file.ServerRelativeUrl));
          }
          finally
          {
            web.AllowUnsafeUpdates = false;
          }
        }
      }
    }

    /// <summary>
    /// Removes an existing file in the directory.
    /// </summary>
    /// <param name="name">The name.</param>
    public override void DeleteFile(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
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

    /// <summary>
    /// Returns true if a file with the given name exists.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns><c>true</c> if the file exists, <c>false</c> otherwise</returns>
    public override bool FileExists(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));
          return file.Exists;
        }
      }
    }

    /// <summary>
    /// Returns the length of a file in the directory.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>System.Int64.</returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    public override long FileLength(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                              name));

          return file.Length;
        }
      }
    }

    /// <summary>
    /// Returns the time the named file was last modified.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>System.Int64.</returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public override long FileModified(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                              name));

          var lastWriteTime = file.TimeLastModified;
          var universalTime = lastWriteTime.ToUniversalTime();
          var timeSpan = universalTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
          return (long)timeSpan.TotalMilliseconds;
        }
      }
    }

    public override string GetLockId()
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          return "SPDirectoryLock_" + folder.UniqueId;
        }
      }
    }

    /// <summary>
    /// Returns an array of strings, one for each file in the directory.
    /// </summary>
    /// <returns>System.String[][].</returns>
    public override string[] ListAll()
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          //TODO: Change this to a ContentIterator
          var folder = web.GetFolder(m_folderUrl);
          return folder.Files
                       .OfType<SPFile>()
                       .Select(f => f.Name)
                       .ToArray();
        }
      }
    }

    /// <summary>
    /// Returns a stream reading an existing file.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>IndexInput.</returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public override IndexInput OpenInput(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                              name));

          return new SPFileInputStream(SPUtility.ConcatUrls(file.Web.Url, file.ServerRelativeUrl));
        }
      }
    }

    /// <summary>
    /// Set the modified time of an existing file to now.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    public override void TouchFile(string name)
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);
          var file = web.GetFile(SPUtility.ConcatUrls(folder.ServerRelativeUrl, name));

          if (file.Exists == false)
            throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                              name));
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
  }
}
