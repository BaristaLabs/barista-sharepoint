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
    private readonly string m_folderUrl;

    internal SPLockFactory(SPFolder lockFolder)
    {
      if (lockFolder == null)
        throw new ArgumentNullException("lockFolder");

      m_folderUrl = lockFolder.Url;
    }

    internal SPLockFactory(string folderUrl)
    {
      if (String.IsNullOrEmpty(folderUrl))
        throw new ArgumentNullException("folderUrl");

      m_folderUrl = folderUrl;
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

      using (var site = new SPSite(m_folderUrl))
      {
        using (var web = site.OpenWeb())
        {
          var folder = web.GetFolder(m_folderUrl);

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

      return new SPLock(m_folderUrl, lockName);
    }
  }
}
