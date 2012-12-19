namespace Barista.SharePoint.Search
{
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using System;

  /// <summary>
  /// Represents a lock factory that uses SharePoint as a backing store.
  /// </summary>
  public sealed class SPLockFactory : LockFactory
  {
    private readonly Guid m_siteId;
    private readonly Guid m_webId;
    private readonly Guid m_folderId;

    internal SPLockFactory(SPFolder lockFolder)
    {
      if (lockFolder == null)
        throw new ArgumentNullException("lockFolder");

      m_siteId = lockFolder.ParentWeb.Site.ID;
      m_webId = lockFolder.ParentWeb.ID;
      m_folderId = lockFolder.UniqueId;
    }

    internal SPLockFactory(Guid siteId, Guid webId, Guid folderId)
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
    /// Attempt to clear (forcefully unlock and remove) the
    /// specified lock.  Only call this at a time when you are
    /// certain this lock is no longer in use.
    /// </summary>
    /// <param name="lockName">name of the lock to be cleared.</param>
    public override void ClearLock(string lockName)
    {
      if (String.IsNullOrEmpty(internalLockPrefix) == false)
        lockName = internalLockPrefix + "-" + lockName;

      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);

          if (folder.Exists == false)
            return;

          var file = web.GetFile(SPUtility.ConcatUrls(folder.Url, lockName));

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
    /// Return a new Lock instance identified by lockName.
    /// </summary>
    /// <param name="lockName">name of the lock to be created.</param>
    /// <returns>Lock.</returns>
    public override Lock MakeLock(string lockName)
    {
      if (String.IsNullOrEmpty(internalLockPrefix) == false)
        lockName = internalLockPrefix + "-" + lockName;

      return new SPLock(m_siteId, m_webId, m_folderId, lockName);
    }
  }
}
