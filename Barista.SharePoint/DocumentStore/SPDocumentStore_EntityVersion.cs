namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class SPDocumentStore
    {
        #region EntityVersion

        /// <summary>
        /// Lists the entity versions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public IList<EntityVersion> ListEntityVersions(string containerTitle, Guid entityId)
        {
            var result = new List<EntityVersion>();

            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile defaultEntityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) == false)
                return null;

            result.AddRange(defaultEntityPart.Item.Versions.OfType<SPListItemVersion>().Select(ver => new EntityVersion
            {
                Comment = ver.ListItem.File.CheckInComment,
                Created = ver.Created.ToLocalTime(),
                CreatedByLoginName = ver.CreatedBy.User.LoginName,
                Entity = SPDocumentStoreHelper.MapEntityFromSPListItemVersion(ver),
                IsCurrentVersion = ver.IsCurrentVersion,
                VersionId = ver.VersionId,
                VersionLabel = ver.VersionLabel,
            }));

            return result;
        }

        /// <summary>
        /// Gets the entity version.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="versionId">The version id.</param>
        /// <returns></returns>
        public EntityVersion GetEntityVersion(string containerTitle, Guid entityId, int versionId)
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

            var ver = defaultEntityPart.Item.Versions.OfType<SPListItemVersion>()
                .FirstOrDefault(v => v.VersionId == versionId);

            if (ver == null)
                return null;

            var entityVersion = new EntityVersion
            {
                Comment = ver.ListItem.File.CheckInComment,
                Created = ver.Created.ToLocalTime(),
                CreatedByLoginName = ver.CreatedBy.User.LoginName,
                Entity = SPDocumentStoreHelper.MapEntityFromSPListItemVersion(ver),
                IsCurrentVersion = ver.IsCurrentVersion,
                VersionId = ver.VersionId,
                VersionLabel = ver.VersionLabel,
            };

            return entityVersion;
        }

        /// <summary>
        /// Reverts the entity to version.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="versionId">The version id.</param>
        /// <returns></returns>
        public EntityVersion RevertEntityToVersion(string containerTitle, Guid entityId, int versionId)
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

            var entityVersion = GetEntityVersion(containerTitle, entityId, versionId);

            web.AllowUnsafeUpdates = true;
            try
            {
                string currentData = defaultEntityPart.Web.GetFileAsString(defaultEntityPart.Url);

                if (entityVersion.Entity.Data != currentData)
                {
                    defaultEntityPart.SaveBinary(String.IsNullOrEmpty(entityVersion.Entity.Data) == false
                                                    ? System.Text.Encoding.Default.GetBytes(entityVersion.Entity.Data)
                                                    : System.Text.Encoding.Default.GetBytes(String.Empty));
                }

                var ds = DocumentSet.GetDocumentSet(defaultEntityPart.ParentFolder);
                if (ds.Item.Title != entityVersion.Entity.Title)
                    ds.Item["Title"] = entityVersion.Entity.Title;

                if ((ds.Item["DocumentSetDescription"] as string) != entityVersion.Entity.Description)
                    ds.Item["DocumentSetDescription"] = entityVersion.Entity.Description;

                if ((ds.Item["Namespace"] as string) != entityVersion.Entity.Namespace)
                    ds.Item["Namespace"] = entityVersion.Entity.Namespace;

                ds.Item.SystemUpdate(true);
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            var versions = ListEntityVersions(containerTitle, entityId);

            return versions.FirstOrDefault(v => v.IsCurrentVersion);
        }
        #endregion
    }
}
