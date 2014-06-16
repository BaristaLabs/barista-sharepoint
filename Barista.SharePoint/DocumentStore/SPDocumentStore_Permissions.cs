namespace Barista.SharePoint.DocumentStore
{
    using System;
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;

    public partial class SPDocumentStore
    {

        #region Container Permissions
        /// <summary>
        /// Adds the principal role to container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo AddPrincipalRoleToContainer(string containerTitle, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A Container with the specified title does not exist.");

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return null;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            PermissionsHelper.AddListPermissionsForPrincipal(web, list, currentPrincipal, roleType);

            return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(list, currentPrincipal);
        }

        /// <summary>
        /// Removes the principal role from container.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual bool RemovePrincipalRoleFromContainer(string containerTitle, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A Container with the specified title does not exist.");

            list.CheckPermissions(SPBasePermissions.ManagePermissions);
            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return false;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            return PermissionsHelper.RemoveListPermissionsForPrincipal(web, list, currentPrincipal, roleType);
        }

        /// <summary>
        /// Gets the container permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetContainerPermissions(string containerTitle)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A Container with the specified title does not exist.");

            return PermissionsHelper.MapPermissionsFromSPSecurableObject(list);
        }

        /// <summary>
        /// Gets the container permissions for principal.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo GetContainerPermissionsForPrincipal(string containerTitle, string principalName, string principalType)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A Container with the specified title does not exist.");

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            return currentPrincipal == null
                ? null
                : PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(list, currentPrincipal);
        }

        /// <summary>
        /// Resets the container permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetContainerPermissions(string containerTitle)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                throw new InvalidOperationException("A Container with the specified title does not exist.");

            web.AllowUnsafeUpdates = true;

            try
            {
                list.ResetRoleInheritance();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return PermissionsHelper.MapPermissionsFromSPSecurableObject(list);
        }
        #endregion

        #region Entity Permissions

        /// <summary>
        /// Adds the principal role to entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo AddPrincipalRoleToEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet entityDocumentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out entityDocumentSet) == false)
                return null;

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return null;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            PermissionsHelper.AddListItemPermissionsForPrincipal(web, entityDocumentSet.Item, currentPrincipal, roleType);

            return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(entityDocumentSet.Item, currentPrincipal);
        }

        /// <summary>
        /// Removes the principal role from entity.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual bool RemovePrincipalRoleFromEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                return false;

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return false;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            return PermissionsHelper.RemoveListItemPermissionsForPrincipal(web, documentSet.Item, currentPrincipal, roleType, false);
        }

        /// <summary>
        /// Gets the entity permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetEntityPermissions(string containerTitle, Guid guid)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            return SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false
                ? null
                : PermissionsHelper.MapPermissionsFromSPSecurableObject(documentSet.Item);
        }

        /// <summary>
        /// Gets the entity permissions for principal.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo GetEntityPermissionsForPrincipal(string containerTitle, Guid guid, string principalName, string principalType)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                return null;

            var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

            return currentPrincipal == null
                ? null
                : PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(documentSet.Item, currentPrincipal);
        }

        /// <summary>
        /// Resets the entity permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetEntityPermissions(string containerTitle, Guid guid)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            DocumentSet documentSet;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                return null;

            web.AllowUnsafeUpdates = true;

            try
            {
                documentSet.Item.ResetRoleInheritance();
                documentSet.Item.Update();
                documentSet.Folder.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return PermissionsHelper.MapPermissionsFromSPSecurableObject(documentSet.Item);
        }
        #endregion

        #region EntityPart Permissions

        /// <summary>
        /// Adds the principal role to entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo AddPrincipalRoleToEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                return null;

            SPPrincipal currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return null;

            SPRoleType roleType = PermissionsHelper.GetRoleType(web, roleName);

            PermissionsHelper.AddListItemPermissionsForPrincipal(web, entityPart.Item, currentPrincipal, roleType);

            return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(entityPart.Item, currentPrincipal);
        }

        /// <summary>
        /// Removes the principal role from entity part.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual bool RemovePrincipalRoleFromEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                return false;

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return false;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            return PermissionsHelper.RemoveListItemPermissionsForPrincipal(web, entityPart.Item, currentPrincipal, roleType, false);
        }

        /// <summary>
        /// Gets the entity part permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetEntityPartPermissions(string containerTitle, Guid guid, string partName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            return SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false
                ? null
                : PermissionsHelper.MapPermissionsFromSPSecurableObject(entityPart.Item);
        }

        /// <summary>
        /// Gets the entity part permissions for principal.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="partName">Name of the part.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo GetEntityPartPermissionsForPrincipal(string containerTitle, Guid guid, string partName, string principalName, string principalType)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                return null;

            var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

            return currentPrincipal == null
                ? null
                : PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(entityPart.Item, currentPrincipal);
        }

        /// <summary>
        /// Resets the entity part permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="partName">Name of the part.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetEntityPartPermissions(string containerTitle, Guid guid, string partName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile entityPart;
            if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                entityPart.Item.ResetRoleInheritance();
                entityPart.Item.Update();
                entityPart.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return PermissionsHelper.MapPermissionsFromSPSecurableObject(entityPart.Item);
        }
        #endregion

        #region Attachment Permissions
        /// <summary>
        /// Adds the principal role to attachment.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo AddPrincipalRoleToAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false)
                return null;

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return null;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            PermissionsHelper.AddListItemPermissionsForPrincipal(web, attachment.Item, currentPrincipal, roleType);

            return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(attachment.Item, currentPrincipal);
        }

        /// <summary>
        /// Removes the principal role from attachment.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <returns></returns>
        public virtual bool RemovePrincipalRoleFromAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return false;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false)
                return false;

            var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

            if (currentPrincipal == null)
                return false;

            var roleType = PermissionsHelper.GetRoleType(web, roleName);

            return PermissionsHelper.RemoveListItemPermissionsForPrincipal(web, attachment.Item, currentPrincipal, roleType, false);
        }

        /// <summary>
        /// Gets the attachment permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetAttachmentPermissions(string containerTitle, Guid guid, string fileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            return SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false
                ? null
                : PermissionsHelper.MapPermissionsFromSPSecurableObject(attachment.Item);
        }

        /// <summary>
        /// Gets the attachment permissions for principal.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="principalName">Name of the principal.</param>
        /// <param name="principalType">Type of the principal.</param>
        /// <returns></returns>
        public virtual PrincipalRoleInfo GetAttachmentPermissionsForPrincipal(string containerTitle, Guid guid, string fileName, string principalName, string principalType)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false)
                return null;

            var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

            return currentPrincipal == null
                ? null
                : PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(attachment.Item, currentPrincipal);
        }

        /// <summary>
        /// Resets the attachment permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetAttachmentPermissions(string containerTitle, Guid guid, string fileName)
        {
            SPSite site;
            var web = GetDocumentStoreWeb(out site);

            SPList list;
            SPFolder folder;
            if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                return null;

            SPFile attachment;
            if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false)
                return null;

            web.AllowUnsafeUpdates = true;
            try
            {
                attachment.Item.ResetRoleInheritance();
                attachment.Item.Update();
                attachment.Update();
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }

            return PermissionsHelper.MapPermissionsFromSPSecurableObject(attachment.Item);
        }
        #endregion

    }
}
