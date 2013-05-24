namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPSecurableObjectInstance : ObjectInstance
  {
    private readonly SPSecurableObject m_securableObject;

    public SPSecurableObjectInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPSecurableObjectInstance(ObjectInstance prototype, SPSecurableObject securableObject)
      : this(prototype)
    {
      m_securableObject = securableObject;
    }

    #region Properties
    [JSProperty(Name = "allRolesForCurrentUser")]
    public ArrayInstance AllRolesForCurrentUser
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var role in m_securableObject.AllRolesForCurrentUser.OfType<SPRoleDefinition>())
        {
          ArrayInstance.Push(result, new SPRoleDefinitionInstance(this.Engine.Object.InstancePrototype, role));
        }
        return result;
      }
    }

    [JSProperty(Name="hasUniqueRoleAssignments")]
    public bool HasUniqueRoleAssignments
    {
      get { return m_securableObject.HasUniqueRoleAssignments; }
    }
    #endregion

    #region Functions to Add/Remove Users/Groups
    [JSFunction(Name = "addGroup")]
    public void AddGroup(string groupName, string roleType)
    {
      SPGroup group;
      if (SPHelper.TryGetSPGroupFromGroupName(groupName, out group) == false)
        throw new JavaScriptException(this.Engine, "Error", "A group with the specified name does not exist.");

      var roleTypeValue = (SPRoleType)Enum.Parse(typeof(SPRoleType), roleType);
      var roleDefinition = SPBaristaContext.Current.Web.RoleDefinitions.GetByType(roleTypeValue);

      AddPrincipal(group, roleDefinition);
    }

    public void AddGroup(string groupName, SPRoleDefinitionInstance roleDefinition)
    {
      SPGroup group;
      if (SPHelper.TryGetSPGroupFromGroupName(groupName, out group) == false)
        throw new JavaScriptException(this.Engine, "Error", "A group with the specified name does not exist.");

      AddPrincipal(group, roleDefinition.RoleDefinition);
    }

    public void AddGroup(SPGroupInstance group, SPRoleDefinitionInstance roleDefinition)
    {
      AddPrincipal(group.Group, roleDefinition.RoleDefinition);
    }

    [JSFunction(Name = "addUser")]
    public void AddUser(string loginName, string roleType)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
        throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

      var roleTypeValue = (SPRoleType)Enum.Parse(typeof(SPRoleType), roleType);
      var roleDefinition = SPBaristaContext.Current.Web.RoleDefinitions.GetByType(roleTypeValue);

      AddPrincipal(user, roleDefinition);
    }

    public void AddUser(string loginName, SPRoleDefinitionInstance roleDefinition)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
        throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

      AddPrincipal(user, roleDefinition.RoleDefinition);
    }

    public void AddUser(SPUserInstance user, string roleType)
    {
      var roleTypeValue = (SPRoleType)Enum.Parse(typeof(SPRoleType), roleType);
      var roleDefinition = SPBaristaContext.Current.Web.RoleDefinitions.GetByType(roleTypeValue);

      AddPrincipal(user.User, roleDefinition);
    }

    public void AddUser(SPUserInstance user, SPRoleDefinitionInstance roleDefinition)
    {
      AddPrincipal(user.User, roleDefinition.RoleDefinition);
    }

    private void AddPrincipal(SPPrincipal principal, SPRoleDefinition roleDefinition)
    {
      SPRoleAssignment roleAssignment = new SPRoleAssignment(principal);
      roleAssignment.RoleDefinitionBindings.Add(roleDefinition);

      if (!m_securableObject.HasUniqueRoleAssignments)
          m_securableObject.BreakRoleInheritance(true, false);

      m_securableObject.RoleAssignments.Add(roleAssignment);
    }

    [JSFunction(Name = "removeGroup")]
    public void RemoveGroup(string groupName)
    {
      SPGroup group;
      if (SPHelper.TryGetSPGroupFromGroupName(groupName, out group) == false)
        throw new JavaScriptException(this.Engine, "Error", "A group with the specified name does not exist.");

      RemovePrincipal(group);
    }

    public void RemoveGroup(SPGroupInstance group)
    {
      RemovePrincipal(group.Group);
    }

    [JSFunction(Name = "removeUser")]
    public void RemoveUser(string loginName)
    {
      SPUser user;
      if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
        throw new JavaScriptException(this.Engine, "Error", "A user with the specified login name does not exist.");

      RemovePrincipal(user);
    }

    public void RemoveUser(SPUserInstance user)
    {
      RemovePrincipal(user.User);
    }

    private void RemovePrincipal(SPPrincipal principal)
    {
      m_securableObject.RoleAssignments.Remove(principal);
    }
    #endregion

    #region Functions
    [JSFunction(Name = "breakRoleInheritance")]
    public void BreakRoleInheritance([DefaultParameterValue(true)]bool copyRoleAssignments, [DefaultParameterValue(true)]bool clearSubscopes)
    {
      m_securableObject.BreakRoleInheritance(copyRoleAssignments, clearSubscopes);
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string permissions)
    {
      var mask = (SPBasePermissions)Enum.Parse(typeof(SPBasePermissions), permissions);

      return m_securableObject.DoesUserHavePermissions(mask);
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

      return m_securableObject.DoesUserHavePermissions(mask);
    }

    [JSFunction(Name = "resetRoleInheritance")]
    public void ResetRoleInheritance()
    {
      m_securableObject.ResetRoleInheritance();
    }
    #endregion
  }
}
