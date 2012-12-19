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
    private readonly Guid m_siteId;
    private readonly Guid m_webId;
    private readonly Guid m_folderId;

    private readonly string m_lockName;

    internal SPLock(Guid siteId, Guid webId, Guid folderId, string lockName)
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
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);

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
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);

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
      using (var site = new SPSite(m_siteId))
      {
        using (var web = site.OpenWeb(m_webId))
        {
          var folder = web.GetFolder(m_folderId);

          if (folder.Exists == false)
            return false;

          var file = web.GetFile(SPUtility.ConcatUrls(folder.Url, m_lockName));

          return file.Exists;
        }
      }
    }
  }
}
