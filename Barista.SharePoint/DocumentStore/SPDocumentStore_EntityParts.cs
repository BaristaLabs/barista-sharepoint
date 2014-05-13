namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class SPDocumentStore
    {
        #region EntityParts

        /// <summary>
        /// Creates the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string data)
        {
            return CreateEntityPart(containerTitle, entityId, partName, String.Empty, data);
        }

        /// <summary>
        /// Creates the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="category">The category.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string category, string data)
        {
            return CreateEntityPart(containerTitle, String.Empty, entityId, partName, category, data);
        }

        public virtual EntityPart CreateEntityPart(string containerTitle, string path, Guid entityId, string partName,
          string category, string data)
        {
            if (partName + Constants.DocumentSetEntityPartExtension == Constants.DocumentStoreDefaultEntityPartFileName ||
                partName + Constants.DocumentSetEntityPartExtension == Constants.DocumentStoreEntityContentsPartFileName)
                throw new InvalidOperationException("Filename is reserved.");

            SPSite site;
            var web = GetDocumentStoreWeb(out site);
            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var entityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
            var listEntityPartContentTypeId = list.ContentTypes.BestMatch(entityPartContentTypeId);
            var entityPartContentType = list.ContentTypes[listEntityPartContentTypeId];

            if (entityPartContentType == null)
                throw new InvalidOperationException("Unable to locate the entity part content type");

            var properties = new Hashtable
            {
              {"ContentTypeId", entityPartContentType.Id.ToString()},
              {"Content Type", entityPartContentType.Name}
            };

            if (String.IsNullOrEmpty(category) == false)
                properties.Add("Category", category);

            web.AllowUnsafeUpdates = true;
            try
            {
                if (documentSet.Item.DoesUserHavePermissions(SPBasePermissions.AddListItems) == false)
                    throw new InvalidOperationException("Insufficent Permissions.");

                var partFileName = partName + Constants.DocumentSetEntityPartExtension;

                var existingEntityPart = list.ParentWeb.GetFile(SPUtility.ConcatUrls(documentSet.Folder.Url, partFileName));

                //double-check lock pattern to prevent race-condition when creating an entity part.
                if (existingEntityPart.Exists == false)
                {
                    var mutex = SPEntityMutexManager.GrabMutex(this.DocumentStoreUrl, entityId);
                    mutex.WaitOne();
                    try
                    {
                        existingEntityPart = list.ParentWeb.GetFile(SPUtility.ConcatUrls(documentSet.Folder.Url, partFileName));

                        if (existingEntityPart.Exists == false)
                        {
                            var partFile = documentSet.Folder.Files.Add(partFileName,
                                System.Text.Encoding.Default.GetBytes(data), properties, true);
                            var entityPart = SPDocumentStoreHelper.MapEntityPartFromSPFile(partFile, data);

                            //Update the content Entity Part
                            string contentHash;
                            DateTime contentModified;
                            SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, partFile.ParentFolder, null,
                                entityPart,
                                out contentHash, out contentModified);

                            documentSet.Folder.Item["DocumentEntityContentsHash"] = contentHash;
                            documentSet.Folder.Item["DocumentEntityContentsLastModified"] = contentModified;
                            documentSet.Folder.Item.UpdateOverwriteVersion();

                            return entityPart;
                        }
                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            //An entity part already exists.
            throw new EntityPartExistsException("An entity part with the specified name already exists.");
        }

        /// <summary>
        /// Gets the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public virtual EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName)
        {
            return GetEntityPart(containerTitle, String.Empty, entityId, partName);
        }

        public virtual EntityPart GetEntityPart(string containerTitle, string path, Guid entityId, string partName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            SPFile entityPartFile;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPartFile) == false)
                return null;

            var result = SPDocumentStoreHelper.MapEntityPartFromSPFile(entityPartFile, null);
            ProcessEntityPartFile(containerTitle, entityId, partName, entityPartFile, result);

            return result;
        }

        /// <summary>
        /// When overridden in a subclass, allows custom processing to occur on the SPFile/EntityPart that is retrieved from SharePoint.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="entityId"></param>
        /// <param name="entityPartFile"></param>
        /// <param name="entity"></param>
        /// <param name="partName"></param>
        protected virtual void ProcessEntityPartFile(string containerTitle, Guid entityId, string partName, SPFile entityPartFile, EntityPart entity)
        {
            //Does nothing in the base implementation.
        }

        /// <summary>
        /// Renames the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="newPartName">New name of the part.</param>
        /// <returns></returns>
        public bool RenameEntityPart(string containerTitle, Guid entityId, string partName, string newPartName)
        {
            return RenameEntityPart(containerTitle, String.Empty, entityId, partName, newPartName);
        }

        public virtual bool RenameEntityPart(string containerTitle, string path, Guid entityId, string partName, string newPartName)
        {
            if (newPartName + Constants.DocumentSetEntityPartExtension == Constants.DocumentStoreDefaultEntityPartFileName)
                throw new InvalidOperationException("Filename is reserved.");

            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return false;

            SPFile entityPartFile;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPartFile) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                entityPartFile.Item["Name"] = newPartName + Constants.DocumentSetEntityPartExtension;
                entityPartFile.Item.UpdateOverwriteVersion();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Updates the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public EntityPart UpdateEntityPart(string containerTitle, Guid entityId, string partName, string category)
        {
            return UpdateEntityPart(containerTitle, String.Empty, entityId, partName, category);
        }

        public virtual EntityPart UpdateEntityPart(string containerTitle, string path, Guid entityId, string partName,
          string category)
        {
            var mutex = SPEntityMutexManager.GrabMutex(this.DocumentStoreUrl, entityId);
            mutex.WaitOne();

            try
            {
                SPSite site;
                var web = GetDocumentStoreWeb(out site);

                SPList list;
                SPFolder folder;
                if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) ==
                    false)
                    return null;

                SPFile entityPartFile;
                if (
                    SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPartFile) ==
                    false)
                    return null;

                web.AllowUnsafeUpdates = true;
                try
                {
                    if (entityPartFile.ParentFolder.Item.DoesUserHavePermissions(SPBasePermissions.EditListItems) == false)
                        throw new InvalidOperationException("Insufficent Permissions.");

                    entityPartFile.Item["Category"] = category;
                    entityPartFile.Item.SystemUpdate(true);


                    var entityPart = SPDocumentStoreHelper.MapEntityPartFromSPFile(entityPartFile, null);

                    //Update the content entity part
                    string contentHash;
                    DateTime contentModified;
                    SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, entityPartFile.ParentFolder, null,
                                                                            entityPart, out contentHash, out contentModified);

                    var documentSetFolder = web.GetFolder(entityPartFile.ParentFolder.UniqueId);
                    documentSetFolder.Item["DocumentEntityContentsHash"] = contentHash;
                    documentSetFolder.Item["DocumentEntityContentsLastModified"] = contentModified;
                    documentSetFolder.Item.UpdateOverwriteVersion();

                    return entityPart;
                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public EntityPart UpdateEntityPartData(string containerTitle, Guid entityId, string partName, string eTag, string data)
        {
            return UpdateEntityPartData(containerTitle, String.Empty, entityId, partName, eTag, data);
        }

        public virtual EntityPart UpdateEntityPartData(string containerTitle, string path, Guid entityId, string partName,
          string eTag, string data)
        {
            var mutex = SPEntityMutexManager.GrabMutex(this.DocumentStoreUrl, entityId);
            mutex.WaitOne();

            try
            {
                SPSite site;
                var web = GetDocumentStoreWeb(out site);

                SPList list;
                SPFolder folder;
                if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) ==
                    false)
                    return null;

                SPFile entityPartFile;
                if (
                    SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPartFile) ==
                    false)
                    return null;

                if (String.IsNullOrEmpty(eTag) == false && entityPartFile.ETag != eTag)
                {
                    throw new InvalidOperationException(
                        string.Format(
                        "Could not update the entity part, the entity part has been updated by another user. New:{0} Existing{1}",
                        eTag, entityPartFile.ETag));
                }

                web.AllowUnsafeUpdates = true;
                try
                {
                    var currentData = entityPartFile.Web.GetFileAsString(entityPartFile.Url);

                    var entityPart = SPDocumentStoreHelper.MapEntityPartFromSPFile(entityPartFile, data);

                    if (data != currentData)
                    {
                        entityPartFile.SaveBinary(String.IsNullOrEmpty(data) == false
                                                    ? System.Text.Encoding.Default.GetBytes(data)
                                                    : System.Text.Encoding.Default.GetBytes(String.Empty));

                        //Update the content entity part
                        string contentHash;
                        DateTime contentModified;
                        SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, entityPartFile.ParentFolder, null,
                                                                                entityPart, out contentHash, out contentModified);

                        var documentSetFolder = web.GetFolder(entityPartFile.ParentFolder.UniqueId);
                        documentSetFolder.Item["DocumentEntityContentsHash"] = contentHash;
                        documentSetFolder.Item["DocumentEntityContentsLastModified"] = contentModified;
                        documentSetFolder.Item.UpdateOverwriteVersion();
                    }

                    return entityPart;
                }
                finally
                {
                    web.AllowUnsafeUpdates = false;
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Deletes the entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public bool DeleteEntityPart(string containerTitle, Guid entityId, string partName)
        {
            return DeleteEntityPart(containerTitle, String.Empty, entityId, partName);
        }

        public virtual bool DeleteEntityPart(string containerTitle, string path, Guid entityId, string partName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return false;

            SPFile entityPartFile;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, entityId, partName, out entityPartFile) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                entityPartFile.Recycle();

                string contentHash;
                SPDocumentStoreHelper.RemoveContentEntityPartKeyValue(web, list, entityPartFile.ParentFolder, partName, out contentHash);

                entityPartFile.ParentFolder.Item["DocumentEntityContentsHash"] = contentHash;
                entityPartFile.ParentFolder.Item["DocumentEntityContentsLastModified"] = DateTime.Now;
                entityPartFile.ParentFolder.Item.UpdateOverwriteVersion();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the entity parts associated with the specified entity in the specified container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public IList<EntityPart> ListEntityParts(string containerTitle, Guid entityId)
        {
            return ListEntityParts(containerTitle, String.Empty, entityId);
        }

        public virtual IList<EntityPart> ListEntityParts(string containerTitle, string path, Guid entityId)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);
                    
            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var entityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
            var listEntityPartContentTypeId = list.ContentTypes.BestMatch(entityPartContentTypeId);
            var entityPartContentType = list.ContentTypes[listEntityPartContentTypeId];

            if (entityPartContentType == null)
                throw new InvalidOperationException("Unable to locate the entity part content type");

            return documentSet.Folder.Files
                .OfType<SPFile>()
                .Where(f =>
                f.Item.ContentTypeId == entityPartContentType.Id &&
                f.Name != Constants.DocumentStoreDefaultEntityPartFileName &&
                f.Name != Constants.DocumentStoreEntityContentsPartFileName
                )
                .Select(f => SPDocumentStoreHelper.MapEntityPartFromSPFile(f, null))
                .ToList();
        }

        #endregion
    }
}
