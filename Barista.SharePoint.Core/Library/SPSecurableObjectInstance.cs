namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPSecurableObjectInstance : ObjectInstance
  {
    public SPSecurableObjectInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFunctions();
    }

    protected SPSecurableObjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    public SPSecurableObject SecurableObject
    {
      get;
      set;
    }

    #region Properties
    [JSProperty(Name = "allRolesForCurrentUser")]
    public SPRoleDefinitionBindingCollectionInstance AllRolesForCurrentUser
    {
      get
      {
        if (SecurableObject.AllRolesForCurrentUser == null)
          return null;

        return new SPRoleDefinitionBindingCollectionInstance(this.Engine.Object.InstancePrototype,
          SecurableObject.AllRolesForCurrentUser);
      }
    }

    [JSProperty(Name = "roleAssignments")]
    public SPRoleAssignmentCollectionInstance RoleAssignments
    {
      get
      {
        if (SecurableObject.RoleAssignments == null)
          return null;

        return new SPRoleAssignmentCollectionInstance(this.Engine.Object.InstancePrototype,
          SecurableObject.RoleAssignments);
      }
    }

    [JSProperty(Name="hasUniqueRoleAssignments")]
    public bool HasUniqueRoleAssignments
    {
      get { return SecurableObject.HasUniqueRoleAssignments; }
    }
    #endregion

    #region Functions to Add/Remove Users/Groups
    [JSFunction(Name = "addGroup")]
    public void AddGroup(object group, object role)
    {
      if (group == Undefined.Value || group == Null.Value || group == null)
        throw new JavaScriptException(this.Engine, "Error", "First argument contain either a group instance or a group name.");

      if (role == Undefined.Value || role == Null.Value || role == null)
        throw new JavaScriptException(this.Engine, "Error", "Second argument contain either a role definnition instance or a role type.");

      SPGroup groupToAdd;
      SPRoleDefinition roleToAdd;

      if (group is SPGroupInstance)
      {
        var spGroup = group as SPGroupInstance;
        groupToAdd = spGroup.Group;
      }
      else
      {
        var groupName = TypeConverter.ToString(group);
        SPGroup spGroup;
        if (SPHelper.TryGetSPGroupFromGroupName(groupName, out spGroup) == false)
          throw new JavaScriptException(this.Engine, "Error", "A group with the specified name does not exist.");

        groupToAdd = spGroup;
      }

      if (role is SPRoleDefinitionInstance)
      {
        var spRoleDefinition = role as SPRoleDefinitionInstance;

        roleToAdd = spRoleDefinition.RoleDefinition;
      }
      else
      {
        var roleType = TypeConverter.ToString(role);

        var roleTypeValue = (SPRoleType)Enum.Parse(typeof(SPRoleType), roleType);
        var roleDefinition = SPBaristaContext.Current.Web.RoleDefinitions.GetByType(roleTypeValue);

        roleToAdd = roleDefinition;
      }

      AddPrincipal(groupToAdd, roleToAdd);
    }

    [JSFunction(Name = "addUser")]
    public void AddUser(object user, object role)
    {
      if (user == Undefined.Value || user == Null.Value || user == null)
        throw new JavaScriptException(this.Engine, "Error", "First argument must be either a user instance or a login name.");

      if (role == Undefined.Value || role == Null.Value || role == null)
        throw new JavaScriptException(this.Engine, "Error", "SEcond argument contain either a role definnition instance or a role type.");

      SPUser userToAdd;
      SPRoleDefinition roleToAdd;

      if (user is SPUserInstance)
      {
        var spUser = user as SPUserInstance;
        userToAdd = spUser.User;
      }
      else
      {
        var loginName = TypeConverter.ToString(user);

        SPUser spUser;
        if (SPHelper.TryGetSPUserFromLoginName(loginName, out spUser) == false)
          throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

        userToAdd = spUser;
      }

      if (role is SPRoleDefinitionInstance)
      {
        var spRoleDefinition = role as SPRoleDefinitionInstance;

        roleToAdd = spRoleDefinition.RoleDefinition;
      }
      else
      {
        var roleType = TypeConverter.ToString(role);

        var roleTypeValue = (SPRoleType)Enum.Parse(typeof(SPRoleType), roleType);
        var roleDefinition = SPBaristaContext.Current.Web.RoleDefinitions.GetByType(roleTypeValue);

        roleToAdd = roleDefinition;
      }

      AddPrincipal(userToAdd, roleToAdd);
    }

    private void AddPrincipal(SPPrincipal principal, SPRoleDefinition roleDefinition)
    {
      var roleAssignment = new SPRoleAssignment(principal);
      roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

      if (!SecurableObject.HasUniqueRoleAssignments)
        SecurableObject.BreakRoleInheritance(true, false);

      SecurableObject.RoleAssignments.Add(roleAssignment);
    }

    [JSFunction(Name = "removeGroup")]
    public void RemoveGroup(object group)
    {
      if (group == Undefined.Value || group == Null.Value || group == null)
        throw new JavaScriptException(this.Engine, "Error", "First argument contain either a group instance or a group name.");

      SPGroup groupToRemove;

      if (group is SPGroupInstance)
      {
        var spGroup = group as SPGroupInstance;
        groupToRemove = spGroup.Group;
      }
      else
      {
        var groupName = TypeConverter.ToString(group);

        SPGroup spGroup;
        if (SPHelper.TryGetSPGroupFromGroupName(groupName, out spGroup) == false)
          throw new JavaScriptException(this.Engine, "Error", "A group with the specified name does not exist.");

        groupToRemove = spGroup;
      }
      
      RemovePrincipal(groupToRemove);
    }

    [JSFunction(Name = "removeUser")]
    public void RemoveUser(object user)
    {
      if (user == Undefined.Value || user == Null.Value || user == null)
        throw new JavaScriptException(this.Engine, "Error", "First argument must be either a user instance or a login name.");

      SPUser userToRemove;

      if (user is SPUserInstance)
      {
        var spUser = user as SPUserInstance;
        userToRemove = spUser.User;
      }
      else
      {
        var loginName = TypeConverter.ToString(user);
        SPUser spUser;
        if (SPHelper.TryGetSPUserFromLoginName(loginName, out spUser) == false)
          throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

        userToRemove = spUser;
      }

      RemovePrincipal(userToRemove);
    }

    private void RemovePrincipal(SPPrincipal principal)
    {
      SecurableObject.RoleAssignments.Remove(principal);
    }
    #endregion

    #region Functions
    [JSFunction(Name = "breakRoleInheritance")]
    public void BreakRoleInheritance(bool copyRoleAssignments, object clearSubscopes)
    {
      if (clearSubscopes == Undefined.Value || clearSubscopes == Null.Value || clearSubscopes == null)
        SecurableObject.BreakRoleInheritance(copyRoleAssignments);
      else
        SecurableObject.BreakRoleInheritance(copyRoleAssignments, TypeConverter.ToBoolean(clearSubscopes));
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string permissions)
    {
      var mask = (SPBasePermissions)Enum.Parse(typeof(SPBasePermissions), permissions);

      return SecurableObject.DoesUserHavePermissions(mask);
    }

    public bool DoesUserHavePermissions(ArrayInstance permissions)
    {
      SPBasePermissions mask = SPBasePermissions.EmptyMask;

      for (int i = 0; i < permissions.Length; i++)
      {
        var flag = permissions[i] as string;
        if (flag != null)
        {
          mask = mask & (SPBasePermissions)Enum.Parse(typeof(SPBasePermissions), flag);
        }
      }

      return SecurableObject.DoesUserHavePermissions(mask);
    }

    [JSFunction(Name = "resetRoleInheritance")]
    public void ResetRoleInheritance()
    {
      SecurableObject.ResetRoleInheritance();
    }
    #endregion
  }
}
