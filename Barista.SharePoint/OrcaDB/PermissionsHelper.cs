namespace Barista.SharePoint.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Microsoft.SharePoint;
  using Barista.OrcaDB;
  using Microsoft.SharePoint.Administration;
  using System.Security.Principal;

  public static class PermissionsHelper
  {
    public static PermissionsInfo MapPermissionsFromSPSecurableObject(SPSecurableObject securableObject)
    {
      var result = new PermissionsInfo();

      result.HasUniqueRoleAssignments = securableObject.HasUniqueRoleAssignments;


      result.Principals = new List<PrincipalRoleInfo>();
      foreach (var roleAssignment in securableObject.RoleAssignments.OfType<SPRoleAssignment>())
      {
        result.Principals.Add(MapPrincipalRoleInfoFromSPRoleAssignment(roleAssignment));
      }

      return result;
    }

    public static PrincipalRoleInfo MapPrincipalRoleInfoFromSPSecurableObject(SPSecurableObject securableObject, SPPrincipal principal)
    {
      PrincipalRoleInfo result = null;
      try
      {
        var roleAssignment = securableObject.RoleAssignments.GetAssignmentByPrincipal(principal);
        result = MapPrincipalRoleInfoFromSPRoleAssignment(roleAssignment);
      }
      catch
      {
        //Do nothing. This assumes that catching the exception is less performance intensive than doing the following:
        //SPRoleAssignment roleAssignment = securableObject.RoleAssignments.OfType<SPRoleAssignment>()
        //                                              .Where(sp => sp.Member.ID == principal.ID)
        //                                              .FirstOrDefault();
      }

      return result;
    }

    public static PrincipalRoleInfo MapPrincipalRoleInfoFromSPRoleAssignment(SPRoleAssignment roleAssignment)
    {
      var result = new PrincipalRoleInfo();

      if (roleAssignment.Member is SPUser)
      {
        var spUser = roleAssignment.Member as SPUser;
        result.Principal = new User()
        {
          Email = spUser.Email,
          LoginName = spUser.LoginName,
          Name = spUser.Name,
        };
      }
      else if (roleAssignment.Member is SPGroup)
      {
        var spGroup = roleAssignment.Member as SPGroup;
        result.Principal = new Group()
        {
          LoginName = spGroup.LoginName,
          Name = spGroup.Name,
          DistributionGroupEmail = spGroup.DistributionGroupEmail,
        };
      }
      else
      {
        throw new NotSupportedException("Unknown or Unsupported Principal Type...");
      }

      result.Roles = new List<Role>();
      foreach (var role in roleAssignment.RoleDefinitionBindings.OfType<SPRoleDefinition>())
      {
        var r = new Role()
        {
          Name = role.Name,
          Order = role.Order,
          Description = role.Description,
        };

        string permissionsString = Enum.Format(typeof(SPBasePermissions), role.BasePermissions, "F");
        r.BasePermissions = permissionsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        result.Roles.Add(r);
      }

      return result;
    }

    public static SPPrincipal GetPrincipal(SPWeb web, string principalName, string principalType)
    {
      if (principalType.ToLowerInvariant() == "user")
      {
        SPUser user = web.AllUsers.OfType<SPUser>().Where(u => u.LoginName == principalName).FirstOrDefault();
        if (user == null)
        {
          try
          {
            user = web.EnsureUser(principalName);
          }
          catch
          {
            //Do Nothing...
          }
          if (user == null)
            return null;
        }
        return user;
      }
      else
      {
        SPGroup group = web.Groups[principalName];
        if (group == null)
        {
          throw new InvalidOperationException("The specified group could not be found.");
        }
        return group;
      }
    }

    public static SPRoleType GetRoleType(SPWeb web, string roleName)
    {
      return (SPRoleType)Enum.Parse(typeof(SPRoleType), roleName);
    }

    /// <summary>
    /// Adds the specified user to the specified role for the specified list. Breaks Role Inheritance on the List Item if it hasn't already been broken.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="currentPrincipal"></param>
    /// <param name="roleName"></param>
    /// <param name="doSystemUpdate"></param>
    /// <param name="eventFiringEnabled"></param>
    public static void AddListPermissionsForPrincipal(SPWeb sourceWeb, SPList list, SPPrincipal principal, SPRoleType roleTypeValue)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      SPRoleAssignment roleAssignment = new SPRoleAssignment(principal);

      SPRoleDefinition roleDefinition = sourceWeb.RoleDefinitions.GetByType(roleTypeValue);
      roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

      sourceWeb.AllowUnsafeUpdates = true;

      try
      {
        if (!list.HasUniqueRoleAssignments)
          list.BreakRoleInheritance(true);

        sourceWeb.AllowUnsafeUpdates = true;

        list.RoleAssignments.Add(roleAssignment);
        list.Update();
      }
      finally
      {
        sourceWeb.AllowUnsafeUpdates = false;
      }
    }

    public static bool RemoveListPermissionsForPrincipal(SPWeb sourceWeb, SPList list, SPPrincipal principal, SPRoleType roleTypeValue)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      sourceWeb.AllowUnsafeUpdates = true;
      bool result = false;

      try
      {
        if (!list.HasUniqueRoleAssignments)
          list.BreakRoleInheritance(true);

        sourceWeb.AllowUnsafeUpdates = true;

        var roleAssignment = list.RoleAssignments.GetAssignmentByPrincipal(principal);
        var roleDefinition = sourceWeb.RoleDefinitions.GetByType(roleTypeValue);

        if (roleAssignment.RoleDefinitionBindings.Contains(roleDefinition))
        {
          roleAssignment.RoleDefinitionBindings.Remove(roleDefinition);
          roleAssignment.Update();

          if (roleAssignment.RoleDefinitionBindings.OfType<SPRoleDefinition>().All(rd => rd.Name == "Limited Access"))
          {
            list.RoleAssignments.Remove(principal);
          }

          list.Update();
          result = true;
        }
      }
      finally
      {
        sourceWeb.AllowUnsafeUpdates = false;
      }

      return result;
    }

    /// <summary>
    /// Adds the specified user to the specified role for the specified list item. Breaks Role Inheritance on the List Item if it hasn't already been broken.
    /// </summary>
    /// <param name="sourceSite"></param>
    /// <param name="sourceWeb"></param>
    /// <param name="listItem"></param>
    /// <param name="principal"></param>
    /// <param name="roleTypeValue"></param>
    public static void AddListItemPermissionsForPrincipal(SPWeb sourceWeb, SPListItem listItem, SPPrincipal principal, SPRoleType roleTypeValue, bool doSystemUpdate)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      SPRoleAssignment roleAssignment = new SPRoleAssignment(principal);

      SPRoleDefinition roleDefinition = sourceWeb.RoleDefinitions.GetByType(roleTypeValue);
      roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

      sourceWeb.AllowUnsafeUpdates = true;

      try
      {
        if (!listItem.HasUniqueRoleAssignments)
          listItem.BreakRoleInheritance(true);

        sourceWeb.AllowUnsafeUpdates = true;

        listItem.RoleAssignments.Add(roleAssignment);

        if (doSystemUpdate)
          listItem.SystemUpdate();
        else
          listItem.Update();

        if (listItem.File != null)
          listItem.File.Update();
      }
      finally
      {
        sourceWeb.AllowUnsafeUpdates = false;
      }
    }

    /// <summary>
    /// Removes the specified user from the specified list item. Breaks Role Inheritance on the List Item if it hasn't already been broken.
    /// </summary>
    /// <param name="sourceSite"></param>
    /// <param name="sourceWeb"></param>
    /// <param name="listItem"></param>
    /// <param name="principal"></param>
    public static bool RemoveListItemPermissionsForPrincipal(SPWeb sourceWeb, SPListItem listItem, SPPrincipal principal, SPRoleType roleTypeValue, bool doSystemUpdate)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      bool result = false;

      sourceWeb.AllowUnsafeUpdates = true;
      try
      {
        if (!listItem.HasUniqueRoleAssignments)
          listItem.BreakRoleInheritance(true);

        sourceWeb.AllowUnsafeUpdates = true;

        var roleAssignment = listItem.RoleAssignments.GetAssignmentByPrincipal(principal);
        var roleDefinition = sourceWeb.RoleDefinitions.GetByType(roleTypeValue);

        if (roleAssignment.RoleDefinitionBindings.Contains(roleDefinition))
        {
          if (roleAssignment.RoleDefinitionBindings.Count > 1)
          {
            roleAssignment.RoleDefinitionBindings.Remove(roleDefinition);

            roleAssignment.Update();

            if (roleAssignment.RoleDefinitionBindings.OfType<SPRoleDefinition>().All(rd => rd.Name == "Limited Access"))
            {
              listItem.RoleAssignments.Remove(principal);
            }

          }
          else
          {
            listItem.RoleAssignments.Remove(principal);
          }

          if (doSystemUpdate)
            listItem.SystemUpdate();
          else
            listItem.Update();

          if (listItem.File != null)
            listItem.File.Update();

          if (listItem.Folder != null)
            listItem.Folder.Update();

          result = true;
        }
      }
      finally
      {
        sourceWeb.AllowUnsafeUpdates = false;
      }

      return result;
    }

    /// <summary>
    /// Turns event firing on/off on the thread
    /// </summary>
    /// <param name="enabled">Indicates whether event firing is enabled (true) or disabled (false)</param>
    public static void ToggleEventFiring(bool enabled)
    {
      HandleEventFiring eventFiring = new HandleEventFiring();

      if (enabled)
        eventFiring.CustomEnableEventFiring();
      else
        eventFiring.CustomDisableEventFiring();
    }

    public static bool IsRunningUnderElevatedPrivledges()
    {
      if (SPContext.Current == null)
        throw new InvalidOperationException("SPContext is null.");

      return IsRunningUnderElevatedPrivledges(SPContext.Current.Site.WebApplication.ApplicationPool);
    }

    public static bool IsRunningUnderElevatedPrivledges(SPApplicationPool applicationPool)
    {
      return WindowsIdentity.GetCurrent().User == applicationPool.ManagedAccount.Sid;
    }
  }
}
