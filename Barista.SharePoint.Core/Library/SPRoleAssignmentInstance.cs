namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPRoleAssignmentConstructor : ClrFunction
  {
    public SPRoleAssignmentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRoleAssignment", new SPRoleAssignmentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPRoleAssignmentInstance Construct(object arg1, object arg2, object arg3, object arg4)
    {

      if (arg1 is SPPrincipalInstance)
        return new SPRoleAssignmentInstance(this.Engine.Object.InstancePrototype,
          new SPRoleAssignment((arg1 as SPPrincipalInstance).SPPrincipal));

      string loginName = null;
      if (arg1 != Undefined.Value && arg1 != Null.Value && arg1 != null)
        loginName = TypeConverter.ToString(arg1);

      string email = null;
      if (arg2 != Undefined.Value && arg2 != Null.Value && arg2 != null)
        email = TypeConverter.ToString(arg2);

      string name = null;
      if (arg3 != Undefined.Value && arg3 != Null.Value && arg3 != null)
        name = TypeConverter.ToString(arg3);

      string notes = null;
      if (arg4 != Undefined.Value && arg4 != Null.Value && arg4 != null)
        notes = TypeConverter.ToString(arg4);

      return new SPRoleAssignmentInstance(this.Engine.Object.InstancePrototype, new SPRoleAssignment(loginName, email, name, notes));
    }
  }

  [Serializable]
  public class SPRoleAssignmentInstance : ObjectInstance
  {
    private readonly SPRoleAssignment m_roleAssignment;

    public SPRoleAssignmentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPRoleAssignmentInstance(ObjectInstance prototype, SPRoleAssignment roleAssignment)
      : this(prototype)
    {
      if (roleAssignment == null)
        throw new ArgumentNullException("roleAssignment");

      m_roleAssignment = roleAssignment;
    }

    public SPRoleAssignment SPRoleAssignment
    {
      get { return m_roleAssignment; }
    }

    [JSProperty(Name = "member")]
    public object Member
    {
      get
      {
        if (m_roleAssignment.Member == null)
          return null;

        if (m_roleAssignment.Member is SPUser)
          return new SPUserInstance(this.Engine, m_roleAssignment.Member as SPUser);
        
        if (m_roleAssignment.Member is SPGroup)
          return new SPGroupInstance(this.Engine, m_roleAssignment.Member as SPGroup);

        return new SPPrincipalInstance(this.Engine, m_roleAssignment.Member);
      }
    }

    [JSProperty(Name = "roleDefinitionBindings")]
    public SPRoleDefinitionBindingCollectionInstance RoleDefinitionBindings
    {
      get
      {
        if (m_roleAssignment.RoleDefinitionBindings == null)
          return null;

        return new SPRoleDefinitionBindingCollectionInstance(this.Engine.Object.InstancePrototype, m_roleAssignment.RoleDefinitionBindings);
      }
    }

    [JSFunction(Name = "getParentSecurableObject")]
    public SPSecurableObjectInstance ParentSecurableObject()
    {
      if (m_roleAssignment.ParentSecurableObject == null)
        return null;

      return new SPSecurableObjectInstance(this.Engine)
      {
        SecurableObject = m_roleAssignment.ParentSecurableObject
      };
    }

    [JSFunction(Name = "importRoleDefinitionBindings")]
    public void ImportRoleDefinitionBindings(SPRoleDefinitionBindingCollectionInstance roleDefinitionBindings)
    {
      if (roleDefinitionBindings == null)
        return;

      m_roleAssignment.ImportRoleDefinitionBindings(roleDefinitionBindings.SPRoleDefinitionBindingCollection);
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_roleAssignment.Update();
    }
  }
}
