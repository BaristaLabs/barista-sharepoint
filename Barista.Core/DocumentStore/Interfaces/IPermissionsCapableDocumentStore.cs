namespace Barista.DocumentStore
{
  using System;

  public interface IPermissionsCapableDocumentStore
  {
    #region Permissions
    PrincipalRoleInfo AddPrincipalRoleToContainer(string containerTitle, string principalName, string principalType, string roleName);
    bool RemovePrincipalRoleFromContainer(string containerTitle, string principalName, string principalType, string roleName);
    PermissionsInfo GetContainerPermissions(string containerTitle);
    PrincipalRoleInfo GetContainerPermissionsForPrincipal(string containerTitle, string principalName, string principalType);
    PermissionsInfo ResetContainerPermissions(string containerTitle);

    PrincipalRoleInfo AddPrincipalRoleToEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName);
    bool RemovePrincipalRoleFromEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName);
    PermissionsInfo GetEntityPermissions(string containerTitle, Guid guid);
    PrincipalRoleInfo GetEntityPermissionsForPrincipal(string containerTitle, Guid guid, string principalName, string principalType);
    PermissionsInfo ResetEntityPermissions(string containerTitle, Guid guid);

    PrincipalRoleInfo AddPrincipalRoleToEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName);
    bool RemovePrincipalRoleFromEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName);
    PermissionsInfo GetEntityPartPermissions(string containerTitle, Guid guid, string partName);
    PrincipalRoleInfo GetEntityPartPermissionsForPrincipal(string containerTitle, Guid guid, string partName, string principalName, string principalType);
    PermissionsInfo ResetEntityPartPermissions(string containerTitle, Guid guid, string partName);

    PrincipalRoleInfo AddPrincipalRoleToAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName);
    bool RemovePrincipalRoleFromAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName);
    PermissionsInfo GetAttachmentPermissions(string containerTitle, Guid guid, string fileName);
    PrincipalRoleInfo GetAttachmentPermissionsForPrincipal(string containerTitle, Guid guid, string fileName, string principalName, string principalType);
    PermissionsInfo ResetAttachmentPermissions(string containerTitle, Guid guid, string fileName);
    #endregion
  }
}
