namespace Barista.SharePoint.DocumentStore
{
    using Barista.DocumentStore;
    using Microsoft.Office.DocumentManagement.DocumentSets;
    using Microsoft.SharePoint;
    using System;
    using System.Diagnostics;
    using Microsoft.SharePoint.Utilities;

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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                        throw new InvalidOperationException("A Container with the specified title does not exist.");

                    list.CheckPermissions(SPBasePermissions.ManagePermissions);
                    SPPrincipal currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

                    if (currentPrincipal == null)
                        return false;

                    SPRoleType roleType = PermissionsHelper.GetRoleType(web, roleName);

                    return PermissionsHelper.RemoveListPermissionsForPrincipal(web, list, currentPrincipal, roleType);
                }
            }
        }

        /// <summary>
        /// Gets the container permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetContainerPermissions(string containerTitle)
        {
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                        throw new InvalidOperationException("A Container with the specified title does not exist.");

                    return PermissionsHelper.MapPermissionsFromSPSecurableObject(list);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    if (SPDocumentStoreHelper.TryGetListForContainer(web, containerTitle, out list) == false)
                        throw new InvalidOperationException("A Container with the specified title does not exist.");

                    var currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

                    if (currentPrincipal == null)
                        return null;

                    return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(list, currentPrincipal);
                }
            }
        }

        /// <summary>
        /// Resets the container permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetContainerPermissions(string containerTitle)
        {
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return false;

                    DocumentSet documentSet;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                        return false;

                    SPPrincipal currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

                    if (currentPrincipal == null)
                        return false;

                    SPRoleType roleType = PermissionsHelper.GetRoleType(web, roleName);

                    return PermissionsHelper.RemoveListItemPermissionsForPrincipal(web, documentSet.Item, currentPrincipal, roleType, false);
                }
            }
        }

        /// <summary>
        /// Gets the entity permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual PermissionsInfo GetEntityPermissions(string containerTitle, Guid guid)
        {
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    DocumentSet documentSet;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                        return null;

                    return PermissionsHelper.MapPermissionsFromSPSecurableObject(documentSet.Item);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    DocumentSet documentSet;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityDocumentSet(list, folder, guid, out documentSet) == false)
                        return null;

                    var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

                    if (currentPrincipal == null)
                        return null;

                    return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(documentSet.Item, currentPrincipal);
                }
            }
        }

        /// <summary>
        /// Resets the entity permissions.
        /// </summary>
        /// <param name="containerTitle">The container title.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public virtual PermissionsInfo ResetEntityPermissions(string containerTitle, Guid guid)
        {
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return false;

                    SPFile entityPart;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                        return false;

                    SPPrincipal currentPrincipal = PermissionsHelper.GetPrincipal(web, principalName, principalType);

                    if (currentPrincipal == null)
                        return false;

                    SPRoleType roleType = PermissionsHelper.GetRoleType(web, roleName);

                    return PermissionsHelper.RemoveListItemPermissionsForPrincipal(web, entityPart.Item, currentPrincipal, roleType, false);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    SPFile entityPart;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                        return null;

                    return PermissionsHelper.MapPermissionsFromSPSecurableObject(entityPart.Item);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    SPFile entityPart;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreEntityPart(list, folder, guid, partName, out entityPart) == false)
                        return null;

                    var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

                    if (currentPrincipal == null)
                        return null;

                    return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(entityPart.Item, currentPrincipal);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    SPFile attachment;
                    return SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false
                      ? null
                      : PermissionsHelper.MapPermissionsFromSPSecurableObject(attachment.Item);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
                    SPList list;
                    SPFolder folder;
                    if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) == false)
                        return null;

                    SPFile attachment;
                    if (SPDocumentStoreHelper.TryGetDocumentStoreAttachment(list, folder, guid, fileName, out attachment) == false)
                        return null;

                    var currentPrincipal = PermissionsHelper.GetPrincipal(list.ParentWeb, principalName, principalType);

                    if (currentPrincipal == null)
                        return null;

                    return PermissionsHelper.MapPrincipalRoleInfoFromSPSecurableObject(attachment.Item, currentPrincipal);
                }
            }
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
            //Get a new web in case we're executing in elevated permissions.
            using (var site = new SPSite(this.DocumentStoreUrl))
            {
                using (var web = site.OpenWeb())
                {
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
            }
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
                            m_elevatedDocumentStoreSite = new SPSite(this.DocumentStoreUrl);
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
                        m_documentStoreSite = new SPSite(this.DocumentStoreUrl);
                        m_documentStoreWeb = m_elevatedDocumentStoreSite.OpenWeb();
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
