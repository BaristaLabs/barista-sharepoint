namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a SharePoint-backed Document Store that uses document sets as containers to hold entities.
    /// </summary>
    [Serializable]
    public partial class SPDocumentStore :
      IFullyCapableDocumentStore,
      IDisposable
    {
        #region Fields
        /// <summary>
        /// Holds the original value of the access denied value from SPSecurity.CatchAccessDeniedException
        /// </summary>
        private readonly bool m_originalCatchAccessDeniedValue;

        /// <summary>
        /// The Id of SPLocks created by the document store.
        /// </summary>
        private const string BaristaDSEntityLockId = "BaristaDS Entity Lock";

        private readonly object m_syncRoot = new Object();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SPDocumentStore"/> class.
        /// </summary>
        public SPDocumentStore()
        {
            m_originalCatchAccessDeniedValue = SPSecurity.CatchAccessDeniedException;
            SPSecurity.CatchAccessDeniedException = false;

            if (SPBaristaContext.HasCurrentContext)
            {
                this.DocumentStoreUrl = SPUtility.ConcatUrls(SPBaristaContext.Current.Site.Url, SPBaristaContext.Current.Web.ServerRelativeUrl);
                this.CurrentUserLoginName = SPBaristaContext.Current.Web.CurrentUser.LoginName;
            }
            else if (SPContext.Current != null)
            {
                this.DocumentStoreUrl = SPUtility.ConcatUrls(SPContext.Current.Site.Url, SPContext.Current.Web.ServerRelativeUrl);
                this.CurrentUserLoginName = SPContext.Current.Web.CurrentUser.LoginName;
            }
            else
            {
                throw new InvalidOperationException(
                  "Could not determine the current context - either a Barista Context or SPContext must be present.");
            }
        }

        public SPDocumentStore(SPWeb web)
            : this()
        {
            if (web == null)
                throw new ArgumentNullException("web");

            this.DocumentStoreUrl = SPUtility.ConcatUrls(web.Site.Url, web.ServerRelativeUrl);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the url of the Document Store document library that is used by the current Document Store Instance.
        /// </summary>
        public string DocumentStoreUrl
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the login name of the user who instantiated the document store.
        /// </summary>
        public string CurrentUserLoginName
        {
            get;
            private set;
        }
        #endregion

        #region Locking

        /// <summary>
        /// Gets the entity lock status.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual LockStatus GetEntityLockStatus(string containerTitle, string path, Guid entityId)
        {
            var defaultEntityPart = GetDefaultEntityPart(containerTitle, path, entityId);

            if (defaultEntityPart.LockType == SPFile.SPLockType.None || defaultEntityPart.LockId != BaristaDSEntityLockId)
                return LockStatus.Unlocked;

            return LockStatus.Locked;
        }

        /// <summary>
        /// Locks the entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="timeoutMs"></param>
        public virtual void LockEntity(string containerTitle, string path, Guid entityId, int? timeoutMs)
        {
            bool isLocked = false;

            TimeSpan lockTimeout = TimeSpan.FromSeconds(60);
            if (timeoutMs.HasValue)
                lockTimeout = TimeSpan.FromMilliseconds(timeoutMs.Value);

            do
            {
                var defaultEntityPart = GetDefaultEntityPart(containerTitle, path, entityId);

                if (defaultEntityPart.LockType == SPFile.SPLockType.None)
                {
                    defaultEntityPart.Web.AllowUnsafeUpdates = true;
                    try
                    {
                        defaultEntityPart.Lock(SPFile.SPLockType.Exclusive, BaristaDSEntityLockId, lockTimeout);
                    }
                    catch (SPFileLockException) { /* Do Nothing */ }
                    finally
                    {
                        defaultEntityPart.Web.AllowUnsafeUpdates = false;
                    }
                }
                else if (defaultEntityPart.LockType == SPFile.SPLockType.Exclusive &&
                         defaultEntityPart.LockId == BaristaDSEntityLockId)
                {
                    //If the entity is locked by the current user, refresh the lock. Otherwise, wait until the lock is released.
                    if (defaultEntityPart.LockedByUser != null &&
                        defaultEntityPart.LockedByUser.LoginName == CurrentUserLoginName)
                    {
                        defaultEntityPart.Web.AllowUnsafeUpdates = true;
                        try
                        {
                            defaultEntityPart.RefreshLock(BaristaDSEntityLockId, lockTimeout);
                        }
                        catch (SPFileLockException) { /* Do Nothing */ }
                        finally
                        {
                            defaultEntityPart.Web.AllowUnsafeUpdates = false;
                        }
                    }
                    else
                    {
                        WaitForEntityLockRelease(containerTitle, path, entityId, 60000);
                        defaultEntityPart.Web.AllowUnsafeUpdates = true;
                        try
                        {
                            defaultEntityPart.Lock(SPFile.SPLockType.Exclusive, BaristaDSEntityLockId, lockTimeout);
                        }
                        catch (SPFileLockException) { /* Do Nothing */ }
                        finally
                        {
                            defaultEntityPart.Web.AllowUnsafeUpdates = false;
                        }
                    }
                }

                //Validate that we really got the lock
                defaultEntityPart = GetDefaultEntityPart(containerTitle, path, entityId);
                if (defaultEntityPart.LockType == SPFile.SPLockType.Exclusive &&
                    defaultEntityPart.LockId == BaristaDSEntityLockId &&
                    defaultEntityPart.LockedByUser != null &&
                    defaultEntityPart.LockedByUser.LoginName == CurrentUserLoginName)
                {
                    //We're good, we can continue.
                    isLocked = true;
                }
            }
            while (isLocked == false);
        }

        /// <summary>
        /// Waits for entity lock release.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="timeoutMs">The timeout ms.</param>
        public void WaitForEntityLockRelease(string containerTitle, string path, Guid entityId, int? timeoutMs)
        {
            LockStatus lockStatus = GetEntityLockStatus(containerTitle, path, entityId);

            if (timeoutMs != null)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (lockStatus == LockStatus.Locked && sw.ElapsedMilliseconds < timeoutMs)
                {
                    System.Threading.Thread.Sleep(250);
                    lockStatus = GetEntityLockStatus(containerTitle, path, entityId);
                }

                if (lockStatus == LockStatus.Locked && sw.ElapsedMilliseconds > timeoutMs)
                {
                    throw new InvalidOperationException("Timeout occurred while waiting for the lock to release. " + sw.ElapsedMilliseconds);
                }
                sw.Stop();
            }
            else
            {
                while (lockStatus == LockStatus.Locked)
                {
                    System.Threading.Thread.Sleep(250);
                    lockStatus = GetEntityLockStatus(containerTitle, path, entityId);
                }
            }
        }

        /// <summary>
        /// Unlocks the entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        public void UnlockEntity(string containerTitle, string path, Guid entityId)
        {
            var defaultEntityPart = GetDefaultEntityPart(containerTitle, path, entityId);

            if (defaultEntityPart.LockType == SPFile.SPLockType.Exclusive &&
                    defaultEntityPart.LockId == BaristaDSEntityLockId &&
                    defaultEntityPart.LockedByUser != null &&
                    defaultEntityPart.LockedByUser.LoginName == CurrentUserLoginName)
            {
                defaultEntityPart.Web.AllowUnsafeUpdates = true;

                try
                {
                    defaultEntityPart.ReleaseLock(BaristaDSEntityLockId);
                }
                finally
                {
                    defaultEntityPart.Web.AllowUnsafeUpdates = false;
                }
            }
        }

        #endregion

        #region Private Methods

        private SPSite m_documentStoreSite;
        private SPWeb m_documentStoreWeb;

        private SPSite m_elevatedDocumentStoreSite;
        private SPWeb m_elevatedDocumentStoreWeb;

        /// <summary>
        /// Returns the SPWeb object that is associated with the current context.
        /// </summary>
        /// <returns></returns>
        private SPWeb GetDocumentStoreWeb(out SPSite site)
        {
            if (SPHelper.IsElevated())
            {
                if (m_elevatedDocumentStoreWeb == null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_elevatedDocumentStoreWeb == null)
                        {
                            m_elevatedDocumentStoreSite = new SPSite(this.DocumentStoreUrl, SPBaristaContext.Current.Site.UserToken);
                            m_elevatedDocumentStoreWeb = m_elevatedDocumentStoreSite.OpenWeb();
                        }
                    }
                }
                site = m_elevatedDocumentStoreSite;
                return m_elevatedDocumentStoreWeb;
            }

            if (m_documentStoreWeb == null)
            {
                lock (m_syncRoot)
                {
                    if (m_documentStoreWeb == null)
                    {
                        m_documentStoreSite = new SPSite(this.DocumentStoreUrl, SPBaristaContext.Current.Site.UserToken);
                        m_documentStoreWeb = m_documentStoreSite.OpenWeb();
                    }
                }
            }
            site = m_documentStoreSite;
            return m_documentStoreWeb;
        }

        /// <summary>
        /// Gets the default entity part SPFile contained in the specified container title and path with the specified Id.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        private SPFile GetDefaultEntityPart(string containerTitle, string path, Guid entityId)
        {
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                        return null;

                    SPFile defaultEntityPart;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                        return null;

                    return defaultEntityPart;
                }
            }
        }

// ReSharper disable once UnusedParameter.Local
        private void CheckIfFileNameIsReserved(string fileName)
        {
            if (fileName == Constants.DocumentStoreDefaultEntityPartFileName ||
                fileName == Constants.DocumentStoreEntityContentsPartFileName)
                throw new InvalidOperationException("Filename is reserved.");
        }

        #endregion

        #region IDisposable
        private bool m_disposed;
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (m_documentStoreWeb != null)
                    {
                        m_documentStoreWeb.Dispose();
                        m_documentStoreWeb = null;
                    }

                    if (m_documentStoreSite != null)
                    {
                        m_documentStoreSite.Dispose();
                        m_documentStoreSite = null;
                    }

                    if (m_elevatedDocumentStoreWeb != null)
                    {
                        m_elevatedDocumentStoreWeb.Dispose();
                        m_elevatedDocumentStoreWeb = null;
                    }

                    if (m_elevatedDocumentStoreSite != null)
                    {
                        m_elevatedDocumentStoreSite.Dispose();
                        m_elevatedDocumentStoreSite = null;
                    }

                    SPSecurity.CatchAccessDeniedException = m_originalCatchAccessDeniedValue;
                }
                m_disposed = true;
            }
        }
        #endregion
    }
}
