namespace Barista.DocumentStore
{
  public interface IDSPermissions
  {
    /// <summary>
    /// Adds the specified role to the specified principal
    /// </summary>
    /// <param name="principalName"></param>
    /// <param name="principalType"></param>
    /// <param name="roleName"></param>
    /// <returns></returns>
    IPrincipalRoleInfo AddPrincipalRole(string principalName, string principalType, string roleName);

    /// <summary>
    /// Removes the specified role from the specified principal.
    /// </summary>
    /// <param name="principalName"></param>
    /// <param name="principalType"></param>
    /// <param name="roleName"></param>
    /// <returns></returns>
    bool RemovePrincipalRole(string principalName, string principalType, string roleName);


    /// <summary>
    /// Returns the permissions information for the object.
    /// </summary>
    /// <returns></returns>
    IPermissionsInfo GetPermissions();

    /// <summary>
    /// Returns the permissions roles defined for the specified principal.
    /// </summary>
    /// <param name="principalName"></param>
    /// <param name="principalType"></param>
    /// <returns></returns>
    IPrincipalRoleInfo GetPermissionsForPrincipal(string principalName, string principalType);

    /// <summary>
    /// Reset permissions to their default values.
    /// </summary>
    /// <returns></returns>
    IPermissionsInfo ResetPermissions();
  }
}
