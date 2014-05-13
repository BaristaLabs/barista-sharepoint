namespace Barista.SharePoint.DocumentStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.SharePoint;

    public partial class SPDocumentStore
    {
        #region Metadata

        /// <summary>
        /// Gets the container metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual string GetContainerMetadata(string containerTitle, string key)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            if (list.RootFolder.Properties.ContainsKey(Constants.MetadataPrefix + key))
                return list.RootFolder.Properties[Constants.MetadataPrefix + key] as string;
            return null;
        }

        /// <summary>
        /// Sets the container metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetContainerMetadata(string containerTitle, string key, string value)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                list.RootFolder.Properties[Constants.MetadataPrefix + key] = value;
                list.RootFolder.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the container metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual IDictionary<string, string> ListContainerMetadata(string containerTitle)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            return list.RootFolder.Properties.Keys.OfType<string>()
                .Where(k => k.StartsWith(Constants.MetadataPrefix))
                .ToDictionary(key => key.Substring(Constants.MetadataPrefix.Length), key => list.RootFolder.Properties[key] as string);
        }

        /// <summary>
        /// Gets the entity metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual string GetEntityMetadata(string containerTitle, Guid entityId, string key)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                return null;

            if (defaultEntityPart.Properties.ContainsKey(Constants.MetadataPrefix + key))
                return defaultEntityPart.Properties[Constants.MetadataPrefix + key] as string;
            return null;
        }

        /// <summary>
        /// Sets the entity metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetEntityMetadata(string containerTitle, Guid entityId, string key, string value)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                defaultEntityPart.Properties[Constants.MetadataPrefix + key] = value;
                defaultEntityPart.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the entity metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual IDictionary<string, string> ListEntityMetadata(string containerTitle, Guid entityId)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                return null;

            return defaultEntityPart.Properties.Keys.OfType<string>()
                .Where(k => k.StartsWith(Constants.MetadataPrefix))
                .ToDictionary(key => key.Substring(Constants.MetadataPrefix.Length), key => defaultEntityPart.Properties[key] as string);
        }

        /// <summary>
        /// Gets the entity part metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual string GetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
                return null;

            if (entityPart.Properties.ContainsKey(Constants.MetadataPrefix + key))
                return entityPart.Properties[Constants.MetadataPrefix + key] as string;
            return null;
        }

        /// <summary>
        /// Sets the entity part metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key, string value)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
                return false;

            web.AllowUnsafeUpdates = true;

            try
            {
                entityPart.Properties[Constants.MetadataPrefix + key] = value;
                entityPart.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the entity part metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public virtual IDictionary<string, string> ListEntityPartMetadata(string containerTitle, Guid entityId, string partName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPart) == false)
                return null;

            return entityPart.Properties.Keys.OfType<string>()
                .Where(k => k.StartsWith(Constants.MetadataPrefix))
                .ToDictionary(key => key.Substring(Constants.MetadataPrefix.Length), key => entityPart.Properties[key] as string);
        }

        /// <summary>
        /// Gets the attachment metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual string GetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return null;

            if (attachment.Properties.ContainsKey(Constants.MetadataPrefix + key))
                return attachment.Properties[Constants.MetadataPrefix + key] as string;
            return null;
        }

        /// <summary>
        /// Sets the attachment metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual bool SetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key, string value)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                attachment.Properties[Constants.MetadataPrefix + key] = value;
                attachment.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the attachment metadata.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual IDictionary<string, string> ListAttachmentMetadata(string containerTitle, Guid entityId, string fileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, entityId, fileName, out attachment) == false)
                return null;

            return attachment.Properties.Keys.OfType<string>()
                .Where(k => k.StartsWith(Constants.MetadataPrefix))
                .ToDictionary(key => key.Substring(Constants.MetadataPrefix.Length), key => attachment.Properties[key] as string);
        }
        #endregion
    }
}
