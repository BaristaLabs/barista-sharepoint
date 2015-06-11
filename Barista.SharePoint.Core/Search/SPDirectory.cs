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
        private readonly bool m_disposeOfSiteAndWeb;
        private readonly object m_syncRoot = new object();
        private SPSite m_site;
        private SPWeb m_web;
        private readonly Lazy<SPFolder> m_folder;

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
            : this(folder, lockFactory, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SPDirectory" /> class with the specified cache directory.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="lockFactory"></param>
        /// <param name="cacheDirectory"></param>
        public SPDirectory(SPFolder folder, LockFactory lockFactory, Lucene.Net.Store.Directory cacheDirectory)
        {
            if (folder == null)
                throw new ArgumentNullException("folder");

            if (folder.Exists == false)
                throw new ArgumentException("A folder was specified, however, the specified folder indicates that it does not exist.");

            m_folderUrl = SPUtility.ConcatUrls(folder.ParentWeb.Url, folder.ServerRelativeUrl);

            m_site = folder.ParentWeb.Site;
            m_web = folder.ParentWeb;
            m_folder = new Lazy<SPFolder>(() => folder);

            if (lockFactory == null)
                lockFactory = new SPLockFactory(m_folderUrl);

            SetLockFactory(lockFactory);
            InitCacheDirectory(cacheDirectory);
            m_disposeOfSiteAndWeb = false;
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
            : this(folderUrl, lockFactory, null)
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SPDirectory"/> class with the specified cache directory.
        /// </summary>
        /// <param name="folderUrl"></param>
        /// <param name="lockFactory"></param>
        /// <param name="cacheDirectory"></param>
        public SPDirectory(string folderUrl, LockFactory lockFactory, Lucene.Net.Store.Directory cacheDirectory)
        {
            if (String.IsNullOrEmpty(folderUrl))
                throw new ArgumentNullException("folderUrl");

            m_folderUrl = folderUrl;

            m_folder = new Lazy<SPFolder>(() =>
              {
                  lock (m_syncRoot)
                  {
                      m_site = new SPSite(m_folderUrl, SPBaristaContext.Current.Site.UserToken);
                      m_web = m_site.OpenWeb();
                      m_web.AllowUnsafeUpdates = true;
                      var folder = m_web.GetFolder(m_folderUrl);
                      if (folder.Exists == false)
                          throw new InvalidOperationException("The specified folder does not exist: " + folder.Url);
                      return folder;
                  }
              });

            if (lockFactory == null)
                lockFactory = new SPLockFactory(m_folderUrl);

            SetLockFactory(lockFactory);
            InitCacheDirectory(cacheDirectory);
            m_disposeOfSiteAndWeb = true;
        }

        #region Properties
        /// <summary>
        /// Gets the Directory used as a cache for the SPDirectory.
        /// </summary>
        /// <remarks>
        /// SharePoint file IO is sloooooow. The cache is meant to speed things up.
        /// </remarks>
        public Lucene.Net.Store.Directory CacheDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an object with with to synchronize against.
        /// </summary>
        public object SyncRoot
        {
            get { return m_syncRoot; }
        }

        public SPSite Site
        {
            get
            {
                if (m_folder.IsValueCreated == false)
                {
                    //Bump the value.
#pragma warning disable 168
                    var value = m_folder.Value;
#pragma warning restore 168
                }

                return m_site;
            }
        }

        public SPWeb Web
        {
            get
            {
                if (m_folder.IsValueCreated == false)
                {
                    //Bump the value.
#pragma warning disable 168
                    var value = m_folder.Value;
#pragma warning restore 168
                }

                return m_web;
            }
        }

        public SPFolder Folder
        {
            get { return m_folder.Value; }
        }

        #endregion

        /// <summary>
        /// Creates a new, empty file in the directory with the given name.
        /// Returns a stream writing this file.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IndexOutput.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override IndexOutput CreateOutput(string name)
        {
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folderUrl, name);

                var file = m_folder.Value.Files.Add(fileUrl, new byte[0], true);
                return new SPFileOutputStream(this, file);
            }
        }

        /// <summary>
        /// Removes an existing file in the directory.
        /// </summary>
        /// <param name="name">The name.</param>
        public override void DeleteFile(string name)
        {
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);

                file.Delete();
            }

            if (CacheDirectory.FileExists(name))
                CacheDirectory.DeleteFile(name);

            if (CacheDirectory.FileExists(name + ".etag"))
                CacheDirectory.DeleteFile(name + ".etag");
        }

        /// <summary>
        /// Returns true if a file with the given name exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns><c>true</c> if the file exists, <c>false</c> otherwise</returns>
        public override bool FileExists(string name)
        {
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);
                return file.Exists;
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
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);

                if (file.Exists == false)
                    throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                                  name));

                return file.Length;
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
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);

                if (file.Exists == false)
                    throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                                  name));

                var lastWriteTime = file.TimeLastModified;
                var universalTime = lastWriteTime.ToUniversalTime();
                var timeSpan = universalTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
                return (long)timeSpan.TotalMilliseconds;
            }
        }

        public override string GetLockId()
        {
            lock (m_syncRoot)
            {
                return "SPDirectoryLock_" + m_folder.Value.UniqueId;
            }
        }

        /// <summary>
        /// Returns an array of strings, one for each file in the directory.
        /// </summary>
        /// <returns>System.String[][].</returns>
        public override string[] ListAll()
        {
            lock (m_syncRoot)
            {
                //TODO: Change this to a ContentIterator
                return m_folder.Value.Files
                               .OfType<SPFile>()
                               .Select(f => f.Name)
                               .ToArray();
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
            lock (m_syncRoot)
            {
                System.Threading.Thread.Sleep(100);
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);

                if (file.Exists == false)
                    throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                                  name));

                return new SPFileInputStream(this, file);
            }
        }

        /// <summary>
        /// Set the modified time of an existing file to now.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public override void TouchFile(string name)
        {
            lock (m_syncRoot)
            {
                var fileUrl = SPUtility.ConcatUrls(m_folder.Value.ServerRelativeUrl, name);
                var file = m_web.GetFile(fileUrl);

                if (file.Exists == false)
                    throw new FileNotFoundException(String.Format("The specified file does not exist:{0} {1}", file.Url,
                                                                  name));
                file.Update();

                WriteCachedFileETag(name, file.ETag);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_disposeOfSiteAndWeb == false)
                return;

            CacheDirectory.Dispose();

            lock (m_syncRoot)
            {
                if (m_site != null)
                {
                    m_site.Dispose();
                    m_site = null;
                }

                if (m_web != null)
                {
                    m_web.Dispose();
                    m_web = null;
                }
            }
        }

        #region Private Methods
        /// <summary>
        /// Initialize the Directory used for caching -- if no cache directory is specified, create one.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        private void InitCacheDirectory(Lucene.Net.Store.Directory cacheDirectory)
        {
            if (cacheDirectory != null)
            {
                // save it off
                this.CacheDirectory = cacheDirectory;
            }
            else
            {
                var cachePath = Utilities.GetTempFolder();
                var cacheDirectoryInfo = new DirectoryInfo(cachePath);

                if (!cacheDirectoryInfo.Exists)
                    cacheDirectoryInfo.Create();

                this.CacheDirectory = new RAMDirectory();
            }
        }

        public string ReadCachedFileETag(string name)
        {
            var fileName = name + ".etag";
            if (CacheDirectory.FileExists(name) == false)
                return String.Empty;

            using (var input = CacheDirectory.OpenInput(fileName))
            {
                return input.ReadString();
            }
        }

        public void WriteCachedFileETag(string name, string eTag)
        {
            var fileName = name + ".etag";
            using (var output = CacheDirectory.CreateOutput(fileName))
            {
                output.WriteString(eTag);
            }
        }

        public StreamInput OpenCachedInputAsStream(string name)
        {
            return new StreamInput(CacheDirectory.OpenInput(name));
        }

        public StreamOutput CreateCachedOutputAsStream(string name)
        {
            return new StreamOutput(CacheDirectory.CreateOutput(name));
        }
        #endregion
    }
}
