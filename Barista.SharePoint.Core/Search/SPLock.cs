namespace Barista.SharePoint.Search
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using System;

  /// <summary>
  /// Represents a lock that uses SharePoint as the backing store.
  /// </summary>
  public sealed class SPLock : Lock
  {
    private readonly string m_folderUrl;

    private readonly string m_lockName;

    public SPLock(string folderUrl, string lockName)
    {
      if (String.IsNullOrEmpty(folderUrl))
        throw new ArgumentNullException("folderUrl");

      m_folderUrl = folderUrl;

      if (String.IsNullOrEmpty(lockName))
        throw new ArgumentNullException("lockName", @"To initialize the lock, a lock name must be specified.");

      m_lockName = lockName;
    }

    /// <summary>
    /// Attempts to obtain exclusive access and immediately return
    /// upon success or failure.
    /// </summary>
    /// <returns>true if exclusive access is obtained</returns>
    public override bool Obtain()
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);

          if (folder.Exists == false)
            return false;

          var file = web.GetFile(SPUtility.ConcatUrls(folder.Url, m_lockName));

          if (file.Exists)
            return true;

          web.AllowUnsafeUpdates = true;
          try
          {
            folder.Files.Add(m_lockName, new byte[0], true);
          }
          finally
          {
            web.AllowUnsafeUpdates = false;
          }

          return true;
        }
      }
    }

    /// <summary>
    /// Releases exclusive access.
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void Release()
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);

          if (folder.Exists == false)
            return;

          var file = web.GetFile(SPUtility.ConcatUrls(folder.Url, m_lockName));

          if (file.Exists == false)
            return;

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
    /// Returns true if the resource is currently locked.  Note that one must
    /// still call <see cref="M:Lucene.Net.Store.Lock.Obtain" /> before using the resource.
    /// </summary>
    /// <returns><c>true</c> if this instance is locked; otherwise, <c>false</c>.</returns>
    public override bool IsLocked()
    {
      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);

          if (folder.Exists == false)
            return false;

          var file = web.GetFile(SPUtility.ConcatUrls(folder.Url, m_lockName));

          return file.Exists;
        }
      }
    }
  }
}
