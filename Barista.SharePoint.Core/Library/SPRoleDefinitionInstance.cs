namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPRoleDefinitionConstructor : ClrFunction
  {
    public SPRoleDefinitionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRoleDefinition", new SPRoleDefinitionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPRoleDefinitionInstance Construct(string name)
    {
      var roleDefinition = BaristaContext.Current.Web.RoleDefinitions
                                         .OfType<SPRoleDefinition>().FirstOrDefault(rd => rd.Name == name);

      if (roleDefinition == null)
        throw new JavaScriptException(this.Engine, "Error", "A role definition with the specified name does not exist on the current web.");

      return new SPRoleDefinitionInstance(this.InstancePrototype, roleDefinition);
    }

    public SPRoleDefinitionInstance Construct(SPRoleDefinition roleDefinition)
    {
      if (roleDefinition == null)
        throw new ArgumentNullException("roleDefinition");

      return new SPRoleDefinitionInstance(this.InstancePrototype, roleDefinition);
    }
  }

  [Serializable]
  public class SPRoleDefinitionInstance : ObjectInstance
  {
    private readonly SPRoleDefinition m_roleDefinition;

    public SPRoleDefinitionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPRoleDefinitionInstance(ObjectInstance prototype, SPRoleDefinition roleDefinition)
      : this(prototype)
    {
      this.m_roleDefinition = roleDefinition;
    }

    internal SPRoleDefinition RoleDefinition
    {
      get { return m_roleDefinition; }
    }
    #region Properties
    [JSProperty(Name="basePermissions")]
    public string BasePermissions
    {
      get { return Enum.Format(typeof(SPBasePermissions), m_roleDefinition.BasePermissions, "G"); }
      set { m_roleDefinition.BasePermissions = (SPBasePermissions)Enum.Parse(typeof(SPBasePermissions), value); }
    }

    [JSProperty(Name="description")]
    public string Description
    {
      get { return m_roleDefinition.Description; }
      set { m_roleDefinition.Description = value; }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get { return m_roleDefinition.Hidden; }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_roleDefinition.Id; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_roleDefinition.Name; }
      set { m_roleDefinition.Name = value; }
    }

    [JSProperty(Name = "order")]
    public int Order
    {
      get { return m_roleDefinition.Order; }
      set { m_roleDefinition.Order = value; }
    }

    [JSDoc("Gets the type of the role definition.")]
    [JSProperty(Name = "type")]
    public string Type
    {
      get { return m_roleDefinition.Type.ToString(); }
    }

    #endregion

    #region Functions
    [JSFunction(Name = "getParentWeb")]
    public SPWebInstance GetParentWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_roleDefinition.ParentWeb);
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_roleDefinition.Update();
    }

    [JSFunction(Name = "getXml")]
    public string GetXml()
    {
      return m_roleDefinition.Xml;
    }
    #endregion
  }
}
