namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.Office.Server.Utilities;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public partial class SPDocumentStore
    {
        #region Entities

        /// <summary>
        /// Creates a new entity in the document store, contained in the specified container in the specified namespace.
        /// </summary>
        /// <param name="containerTitle">The container title. Required.</param>
        /// <param name="title">The title of the entity. Optional.</param>
        /// <param name="namespace">The namespace of the entity. Optional.</param>
        /// <param name="data">The data to store with the entity. Optional.</param>
        /// <returns></returns>
        public Entity CreateEntity(string containerTitle, string title, string @namespace, string data)
        {
            return CreateEntity(containerTitle, String.Empty, title, @namespace, data);
        }

        /// <summary>
        /// Creates a new entity in the document store, contained in the specified container in the specified folder and namespace.
        /// </summary>
        /// <param name="containerTitle">The container title. Required.</param>
        /// <param name="path">The path. Optional.</param>
        /// <param name="title">The title of the entity. Optional.</param>
        /// <param name="namespace">The namespace of the entity. Optional.</param>
        /// <param name="data">The data to store with the entity. Optional.</param>
        /// <returns></returns>
        public virtual Entity CreateEntity(string containerTitle, string path, string title, string @namespace, string data)
        {
            if (data == null)
                data = String.Empty;

            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                throw new InvalidOperationException("Unable to retrieve the specified folder -- Folder does not exist.");

            var newGuid = Guid.NewGuid();
            var entityTitle = String.IsNullOrEmpty(title)
              ? newGuid.ToString()
              : title;
            var docEntityContentTypeId = list.ContentTypes.BestMatch(new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId));

            var properties = new Hashtable
            {
              {"DocumentSetDescription", "Document Store Entity"},
              {"DocumentEntityGuid", newGuid.ToString()},
              {"Namespace", @namespace}
            };

            web.AllowUnsafeUpdates = true;
            try
            {
                if ((folder.Item == null && list.RootFolder == folder && list.DoesUserHavePermissions(SPBasePermissions.AddListItems) == false) ||
                    (folder.Item != null && (folder.Item.DoesUserHavePermissions(SPBasePermissions.AddListItems) == false)))
                    throw new InvalidOperationException("Insufficient Permissions.");

                DocumentSet documentSet;
                if (PermissionsHelper.IsRunningUnderElevatedPrivledges(site.WebApplication.ApplicationPool))
                {
                    var existingEntity = list.ParentWeb.GetFile(SPUtility.ConcatUrls(folder.Url, entityTitle));

                    //Double check locking
                    if (existingEntity.Exists == false)
                    {
                        var mutex = SPEntityMutexManager.GrabMutex(this.DocumentStoreUrl, newGuid);
                        mutex.WaitOne();

                        try
                        {
                            existingEntity = list.ParentWeb.GetFile(SPUtility.ConcatUrls(folder.Url, entityTitle));
                            if (existingEntity.Exists == false)
                            {
                                var currentUser = web.AllUsers[CurrentUserLoginName];
                                documentSet = DocumentSet.Create(folder, entityTitle, docEntityContentTypeId, properties, false,
                                  currentUser);

                                //Re-retrieve the document set folder, otherwise bad things happen.
                                var documentSetFolder = web.GetFolder(documentSet.Folder.Url);
                                documentSet = DocumentSet.GetDocumentSet(documentSetFolder);

                                var entityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
                                var listEntityPartContentTypeId = list.ContentTypes.BestMatch(entityPartContentTypeId);
                                var entityPartContentType = list.ContentTypes[listEntityPartContentTypeId];

                                var entityPartProperties = new Hashtable
                    {
                      {"ContentTypeId", entityPartContentType.Id.ToString()},
                      {"Content Type", entityPartContentType.Name}
                    };


                                documentSet.Folder.Files.Add(Constants.DocumentStoreDefaultEntityPartFileName,
                                  Encoding.Default.GetBytes(data), entityPartProperties, currentUser, currentUser, DateTime.UtcNow,
                                  DateTime.UtcNow, true);

                                //Update the contents entity part and the modified by user stamp.
                                string contentHash;
                                DateTime contentModified;
                                SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, documentSet.Folder, null, null,
                                  out contentHash, out contentModified);

                                //Set the created/updated fields of the new document set to the current user.
                                var userLogonName = currentUser.ID + ";#" + currentUser.Name;
                                documentSet.Item[SPBuiltInFieldId.Editor] = userLogonName;
                                documentSet.Item["DocumentEntityContentsHash"] = contentHash;
                                documentSet.Item["DocumentEntityContentsLastModified"] = contentModified;
                                documentSet.Item.UpdateOverwriteVersion();

                                return SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, data);
                            }
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
                else
                {
                    var existingEntity = list.ParentWeb.GetFile(SPUtility.ConcatUrls(folder.Url, entityTitle));

                    //Double check locking
                    if (existingEntity.Exists == false)
                    {
                        var mutex = SPEntityMutexManager.GrabMutex(this.DocumentStoreUrl, newGuid);
                        mutex.WaitOne();
                        try
                        {
                            existingEntity = list.ParentWeb.GetFile(SPUtility.ConcatUrls(folder.Url, entityTitle));

                            if (existingEntity.Exists == false)
                            {
                                documentSet = DocumentSet.Create(folder, entityTitle, docEntityContentTypeId, properties, false);

                                //Re-retrieve the document set folder, otherwise bad things happen.
                                var documentSetFolder = web.GetFolder(documentSet.Folder.Url);
                                documentSet = DocumentSet.GetDocumentSet(documentSetFolder);

                                var entityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
                                var listEntityPartContentTypeId = list.ContentTypes.BestMatch(entityPartContentTypeId);
                                var entityPartContentType = list.ContentTypes[listEntityPartContentTypeId];

                                var entityPartProperties = new Hashtable
                    {
                      {"ContentTypeId", entityPartContentType.Id.ToString()},
                      {"Content Type", entityPartContentType.Name}
                    };

                                documentSet.Folder.Files.Add(Constants.DocumentStoreDefaultEntityPartFileName,
                                  Encoding.Default.GetBytes(data), entityPartProperties, true);

                                //Update the contents Entity Part.
                                string contentHash;
                                DateTime contentModified;
                                SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, documentSet.Folder, null, null,
                                  out contentHash, out contentModified);

                                documentSet.Item["DocumentEntityContentsHash"] = contentHash;
                                documentSet.Item["DocumentEntityContentsLastModified"] = contentModified;
                                documentSet.Item.UpdateOverwriteVersion();

                                return SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, data);
                            }
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            throw new EntityExistsException("An entity with the specified title exists.");
        }

        /// <summary>
        /// Gets the specified untyped entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public Entity GetEntity(string containerTitle, Guid entityId)
        {
            return GetEntity(containerTitle, entityId, String.Empty);
        }

        /// <summary>
        /// Gets the specified untyped entity without its data property populated.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Entity GetEntityLight(string containerTitle, Guid entityId)
        {
            return GetEntityLight(containerTitle, entityId, String.Empty);
        }

        /// <summary>
        /// Gets the specified untyped entity in the specified path.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual Entity GetEntity(string containerTitle, Guid entityId, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            DocumentSet entityDocumentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out entityDocumentSet) == false)
                return null;

            var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(entityDocumentSet, null);

            return entity;
        }

        /// <summary>
        /// Gets the specified untyped entity in the specified path without its data property populated.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="entityId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public virtual Entity GetEntityLight(string containerTitle, Guid entityId, string path)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            DocumentSet entityDocumentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out entityDocumentSet) == false)
                return null;

            var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(entityDocumentSet, null, null);

            return entity;
        }

        /// <summary>
        /// Updates the metadata associated with the entity, and not any data.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="namespace"></param>
        /// <returns></returns>
        public virtual Entity UpdateEntity(string containerTitle, Guid entityId, string title, string description, string @namespace)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            if (documentSet.Item.DoesUserHavePermissions(SPBasePermissions.EditListItems) == false)
                throw new InvalidOperationException("Insufficent Permissions.");

            Entity entity;
            web.AllowUnsafeUpdates = true;
            try
            {
                var needsUpdate = false;
                if (String.IsNullOrEmpty(title) == false && documentSet.Item.Title != title)
                {
                    documentSet.Item["Title"] = title;
                    needsUpdate = true;
                }

                if (description != null && (documentSet.Item["DocumentSetDescription"] as string) != description)
                {
                    documentSet.Item["DocumentSetDescription"] = description;
                    needsUpdate = true;
                }

                if (@namespace != null && (documentSet.Item["Namespace"] as string) != @namespace)
                {
                    documentSet.Item["Namespace"] = @namespace;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    documentSet.Item.SystemUpdate(true);

                    entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, null);

                    //Update the contents entity part
                    string contentHash;
                    DateTime contentModified;
                    SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, documentSet.Folder,
                                                                            entity, null, out contentHash, out contentModified);

                    documentSet.Item["DocumentEntityContentsHash"] = contentHash;
                    documentSet.Item["DocumentEntityContentsLastModified"] = contentModified;
                    documentSet.Item.UpdateOverwriteVersion();

                    entity.ContentsETag = contentHash;
                    entity.ContentsModified = contentModified;
                }
                else
                {
                    entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, null);
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return entity;
        }

        /// <summary>
        /// Updates the data of the specified entity.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="entityId"></param>
        /// <param name="eTag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Entity UpdateEntityData(string containerTitle, Guid entityId, string eTag, string data)
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

            if (defaultEntityPart.Item.DoesUserHavePermissions(SPBasePermissions.EditListItems) == false)
                throw new InvalidOperationException("Insufficent Permissions.");

            if (String.IsNullOrEmpty(eTag) == false && defaultEntityPart.ETag != eTag)
            {
                throw new InvalidOperationException(String.Format("Could not update the entity, the Entity has been updated by another user. New: {0} Existing:{1}", eTag, defaultEntityPart.ETag));
            }

            var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(DocumentSet.GetDocumentSet(defaultEntityPart.ParentFolder), data);
            web.AllowUnsafeUpdates = true;
            try
            {
                string currentData = defaultEntityPart.Web.GetFileAsString(defaultEntityPart.Url);

                if (data != currentData)
                {
                    defaultEntityPart.SaveBinary(String.IsNullOrEmpty(data) == false
                                                    ? System.Text.Encoding.Default.GetBytes(data)
                                                    : System.Text.Encoding.Default.GetBytes(String.Empty));

                    //Update the contents Entity Part.
                    string contentHash;
                    DateTime contentModified;
                    SPDocumentStoreHelper.CreateOrUpdateContentEntityPart(web, list, defaultEntityPart.ParentFolder, entity, null, out contentHash, out contentModified);

                    var documentSetFolder = web.GetFolder(defaultEntityPart.ParentFolder.UniqueId);
                    documentSetFolder.Item["DocumentEntityContentsHash"] = contentHash;
                    documentSetFolder.Item["DocumentEntityContentsLastModified"] = contentModified;
                    documentSetFolder.Item.UpdateOverwriteVersion();

                    entity.ContentsETag = contentHash;
                    entity.ContentsModified = contentModified;
                }
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return entity;
        }

        /// <summary>
        /// Deletes the specified entity from the specified container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual bool DeleteEntity(string containerTitle, Guid entityId)
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
                var documentSet = SPDocumentStoreHelper.GetDocumentStoreEntityDocumentSet(list, list.RootFolder, entityId);
                if (documentSet == null)
                    return false;

                documentSet.Folder.Recycle();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return true;
        }

        /// <summary>
        /// Lists the entities.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public IList<Entity> ListEntities(string containerTitle)
        {
            return ListEntities(containerTitle, String.Empty, null);
        }

        /// <summary>
        /// Lists the entities.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public IList<Entity> ListEntities(string containerTitle, string path)
        {
            return ListEntities(containerTitle, path, null);
        }

        /// <summary>
        /// Lists all entities contained in the container with the specified criteria.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IList<Entity> ListEntities(string containerTitle, EntityFilterCriteria criteria)
        {
            return ListEntities(containerTitle, criteria.Path, criteria);
        }

        /// <summary>
        /// Lists all entities contained in the container with the specified criteria.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public int CountEntities(string containerTitle, EntityFilterCriteria criteria)
        {
            return CountEntities(containerTitle, criteria.Path, criteria);
        }

        /// <summary>
        /// Lists the entities.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        public virtual IList<Entity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return null;

            ContentIterator.EnsureContentTypeIndexed(list);

            var documentStoreEntityContentTypeId = list.ContentTypes.BestMatch(new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId));

            var camlQuery = @"<Where>
    <Eq>
      <FieldRef Name=""ContentTypeId"" />
      <Value Type=""ContentTypeId"">" + documentStoreEntityContentTypeId + @"</Value>
    </Eq>
</Where>";

            var viewFields = SPDocumentStoreHelper.GetDocumentStoreEntityViewFieldsXml();

            var filteredListItems = new List<SPListItem>();

            var itemsIterator = new ContentIterator();
            itemsIterator.ProcessListItems(list, camlQuery + ContentIterator.ItemEnumerationOrderByPath + viewFields, ContentIterator.MaxItemsPerQueryWithViewFields, true, folder,
                                            spListItems =>
                                            {
                                                var listItems = FilterListItemEntities(spListItems.OfType<SPListItem>(),
                                                                                        criteria);
                                                // ReSharper disable AccessToModifiedClosure
                                                filteredListItems.AddRange(listItems);
                                                // ReSharper restore AccessToModifiedClosure
                                            },
                                            null);

            var result = new List<Entity>();
            if (criteria == null || criteria.IncludeData)
            {
                result.AddRange(
                    filteredListItems
                    .Select(li =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        var listItemFolder = li.Folder ?? web.GetFolder(SPUtility.GetUrlDirectory(li.Url));
                        // ReSharper restore AccessToDisposedClosure

                        return SPDocumentStoreHelper.MapEntityFromDocumentSet(
                            DocumentSet.GetDocumentSet(listItemFolder), null);
                    })
                    .Where(entity => entity != null)
                    );

                ProcessEntityList(containerTitle, path, criteria, folder, result);
            }
            else
            {
                if (criteria.Skip.HasValue)
                    filteredListItems = filteredListItems.Skip((int)criteria.Skip.Value).ToList();

                if (criteria.Top.HasValue)
                    filteredListItems = filteredListItems.Take((int)criteria.Top.Value).ToList();

                result.AddRange(
                    filteredListItems
                    .Select(
                        li =>
                        SPDocumentStoreHelper.MapEntityFromDocumentSet(DocumentSet.GetDocumentSet(li.Folder), null, null))
                    .Where(entity => entity != null)
                    );

                ProcessEntityList(containerTitle, path, criteria, folder, result);
            }

            return result;
        }

        /// <summary>
        /// Filters the specified collection of Entity List Items according to the specified criteria.
        /// </summary>
        /// <param name="listItems"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected IEnumerable<SPListItem> FilterListItemEntities(IEnumerable<SPListItem> listItems, EntityFilterCriteria criteria)
        {
            if (criteria == null)
                return listItems;

            if (String.IsNullOrEmpty(criteria.Namespace) == false)
            {
                switch (criteria.NamespaceMatchType)
                {
                    case NamespaceMatchType.Equals:
                        listItems = listItems
                          .Where(li => ((string)li[Constants.NamespaceFieldId]) == criteria.Namespace);
                        break;
                    case NamespaceMatchType.StartsWith:
                        listItems = listItems.Where(li => ((string)li[Constants.NamespaceFieldId])
                                                            .StartsWith(criteria.Namespace))
                                             .ToList();
                        break;
                    case NamespaceMatchType.EndsWith:
                        listItems = listItems
                          .Where(li => ((string)li[Constants.NamespaceFieldId]).EndsWith(criteria.Namespace));
                        break;
                    case NamespaceMatchType.Contains:
                        listItems = listItems
                          .Where(li => ((string)li[Constants.NamespaceFieldId]).Contains(criteria.Namespace));
                        break;
                    case NamespaceMatchType.StartsWithMatchAllQueryPairs:
                        {
                            listItems = listItems.Where(li =>
                            {
                                var ns = li[Constants.NamespaceFieldId] as string;
                                Uri namespaceUri;
                                if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                                    return false;

                                var qs = new QueryString(namespaceUri.Query);
                                return
                                  criteria.QueryPairs.All(
                                    qp =>
                                    qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                                    String.Compare(qs[qp.Key], qp.Value, StringComparison.CurrentCultureIgnoreCase) == 0);
                            });
                        }
                        break;
                    case NamespaceMatchType.StartsWithMatchAllQueryPairsContainsValue:
                        {
                            listItems = listItems.Where(li =>
                            {
                                var ns = li[Constants.NamespaceFieldId] as string;
                                Uri namespaceUri;
                                if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                                    return false;

                                var qs = new QueryString(namespaceUri.Query);
                                return
                                  criteria.QueryPairs.All(
                                    qp =>
                                    qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                                    qs[qp.Key].ToLower().Contains(qp.Value.ToLower()));
                            });
                        }
                        break;
                    case NamespaceMatchType.StartsWithMatchAnyQueryPairs:
                        {
                            listItems = listItems.Where(li =>
                            {
                                var ns = li[Constants.NamespaceFieldId] as string;
                                Uri namespaceUri;
                                if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                                    return false;

                                var qs = new QueryString(namespaceUri.Query);
                                return
                                  criteria.QueryPairs.Any(
                                    qp =>
                                    qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                                    String.Compare(qs[qp.Key], qp.Value, StringComparison.CurrentCultureIgnoreCase) == 0);
                            });
                        }
                        break;
                    case NamespaceMatchType.StartsWithMatchAnyQueryPairsContainsValue:
                        {
                            listItems = listItems.Where(li =>
                            {
                                var ns = li[Constants.NamespaceFieldId] as string;
                                Uri namespaceUri;
                                if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri))
                                    return false;

                                var qs = new QueryString(namespaceUri.Query);
                                return
                                  criteria.QueryPairs.Any(
                                    qp =>
                                    qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                                    qs[qp.Key].ToLower().Contains(qp.Value.ToLower()));
                            });
                        }
                        break;
                }
            }

            return listItems;
        }

        /// <summary>
        /// Returns the total number of entities that correspond to the specified filter criteria.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="path"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual int CountEntities(string containerTitle, string path, EntityFilterCriteria criteria)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
                return 0;

            ContentIterator.EnsureContentTypeIndexed(list);

            var documentStoreEntityContentTypeId = list.ContentTypes.BestMatch(new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId));

            var camlQuery = @"<Where>
    <Eq>
      <FieldRef Name=""ContentTypeId"" />
      <Value Type=""ContentTypeId"">" + documentStoreEntityContentTypeId + @"</Value>
    </Eq>
</Where>";

            var viewFields = SPDocumentStoreHelper.GetDocumentStoreEntityViewFieldsXml();

            var filteredListItems = new List<SPListItem>();

            var itemsIterator = new ContentIterator();
            itemsIterator.ProcessListItems(list, camlQuery + ContentIterator.ItemEnumerationOrderByPath + viewFields, ContentIterator.MaxItemsPerQueryWithViewFields, true, folder,
                                            spListItems =>
                                            {
                                                var listItems = FilterListItemEntities(spListItems.OfType<SPListItem>(),
                                                                                        criteria);
                                                // ReSharper disable AccessToModifiedClosure
                                                filteredListItems.AddRange(listItems);
                                                // ReSharper restore AccessToModifiedClosure
                                            },
                                            null);

            return filteredListItems.Count;
        }

        /// <summary>
        /// When overridden in a subclass, allows custom processing to occur on the SPListItemCollection/IList<Entity/> that is retrieved from SharePoint.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="path"></param>
        /// <param name="criteria"></param>
        /// <param name="folder"></param>
        /// <param name="entities"></param>
        protected virtual void ProcessEntityList(string containerTitle, string path, EntityFilterCriteria criteria, SPFolder folder, IList<Entity> entities)
        {
            //Does nothing in the base implementation.
        }

        /// <summary>
        /// Imports an entity from a previous export.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="namespace">The @namespace.</param>
        /// <param name="archiveData">The archive data.</param>
        /// <returns></returns>
        public Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, byte[] archiveData)
        {
            return ImportEntity(containerTitle, String.Empty, entityId, @namespace, archiveData);
        }

        /// <summary>
        /// Imports an entity from a previous export.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="path">The path.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="namespace">The @namespace.</param>
        /// <param name="archiveData">The archive data.</param>
        /// <returns></returns>
        public virtual Entity ImportEntity(string containerTitle, string path, Guid entityId, string @namespace, byte[] archiveData)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, path) == false)
            {
                folder = CreateFolderInternal(web, containerTitle, path);
            }

            var docEntityContentTypeId = list.ContentTypes.BestMatch(new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId));

            var properties = new Hashtable
            {
              {"DocumentSetDescription", "Document Store Entity"},
              {"DocumentEntityGuid", entityId.ToString()},
              {"Namespace", @namespace}
            };

            DocumentSet importedDocSet;

            web.AllowUnsafeUpdates = true;
            try
            {
                var currentUser = web.AllUsers[CurrentUserLoginName];
                importedDocSet = DocumentSet.Import(archiveData, entityId.ToString(), folder, docEntityContentTypeId, properties, currentUser);

                //Update with the specified id.
                importedDocSet.Item[SPBuiltInFieldId.Title] = entityId.ToString();
                importedDocSet.Item[Constants.DocumentEntityGuidFieldId] = entityId.ToString();
                importedDocSet.Item.SystemUpdate(false);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return SPDocumentStoreHelper.MapEntityFromDocumentSet(importedDocSet, null);
        }

        /// <summary>
        /// Returns a stream that contains an export of the entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public virtual Stream ExportEntity(string containerTitle, Guid entityId)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return null;

            var ms = new MemoryStream();
            documentSet.Export(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        /// <summary>
        /// Moves the specified entity to the specified destination folder.
        /// </summary>
        /// <param name="containerTitle"></param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <returns></returns>
        public virtual bool MoveEntity(string containerTitle, Guid entityId, string destinationPath)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                throw new InvalidOperationException("A container with the specified title could not be found.");

            SPList destinationList; // Should be the same....
            SPFolder destinationFolder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out destinationList, out destinationFolder, destinationPath) == false)
                throw new InvalidOperationException("The destination folder is invalid.");

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, entityId, out documentSet) == false)
                return false;

            web.AllowUnsafeUpdates = true;
            try
            {
                documentSet.Folder.MoveTo(destinationFolder.Url + "/" + documentSet.Folder.Name);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
            return true;
        }
        #endregion

    }
}
