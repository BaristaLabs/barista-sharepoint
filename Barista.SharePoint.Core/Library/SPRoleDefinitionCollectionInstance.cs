namespace Barista.SharePoint.Library
{
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint;
  using System;
  using System.Linq;

  [Serializable]
  public class SPRoleDefinitionCollectionConstructor : ClrFunction
  {
    public SPRoleDefinitionCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRoleDefinitionCollection", new SPRoleDefinitionCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPRoleDefinitionCollectionInstance Construct()
    {
      return new SPRoleDefinitionCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPRoleDefinitionCollectionInstance : ObjectInstance
  {
    private readonly SPRoleDefinitionCollection m_roleDefinitionCollection;

    public SPRoleDefinitionCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPRoleDefinitionCollectionInstance(ObjectInstance prototype, SPRoleDefinitionCollection roleDefinitionCollection)
      : this(prototype)
    {
      if (roleDefinitionCollection == null)
        throw new ArgumentNullException("roleDefinitionCollection");

      m_roleDefinitionCollection = roleDefinitionCollection;
    }

    public SPRoleDefinitionCollection SPRoleDefinitionCollection
    {
      get { return m_roleDefinitionCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_roleDefinitionCollection.Count;
      }
    }

    [JSFunction(Name = "add")]
    public void Add(SPRoleDefinitionInstance role)
    {
      if (role == null)
        return;

      m_roleDefinitionCollection.Add(role.RoleDefinition);
    }

    [JSFunction(Name = "breakInheritance")]
    public void BreakInheritance(bool copyRoleDefinitions, bool keepRoleAssignments)
    {
      m_roleDefinitionCollection.BreakInheritance(copyRoleDefinitions, keepRoleAssignments);
    }

    [JSFunction(Name = "deleteByRoleName")]
    public void DeleteByRoleName(string roleName)
    {
      m_roleDefinitionCollection.Delete(roleName);
    }

    [JSFunction(Name = "deleteByIndex")]
    public void DeleteByIndex(int index)
    {
      m_roleDefinitionCollection.Delete(index);
    }

    [JSFunction(Name = "deleteById")]
    public void DeleteById(int id)
    {
      m_roleDefinitionCollection.DeleteById(id);
    }

    [JSFunction(Name = "getById")]
    public SPRoleDefinitionInstance GetById(int id)
    {
      var result = m_roleDefinitionCollection.GetById(id);

      return result == null
        ? null
        : new SPRoleDefinitionInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getByType")]
    public SPRoleDefinitionInstance GetByType(string roleType)
    {
      SPRoleType eRoleType;

      if (!roleType.TryParseEnum(true, out eRoleType))
        return null;

      var result = m_roleDefinitionCollection.GetByType(eRoleType);

      return result == null
        ? null
        : new SPRoleDefinitionInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getXml")]
    public string GetXml()
    {
      return m_roleDefinitionCollection.Xml;
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+SPRoleDefinition]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var roleDefinition in m_roleDefinitionCollection.OfType<SPRoleDefinition>())
      {
        ArrayInstance.Push(result, new SPRoleDefinitionInstance(Engine.Object.InstancePrototype, roleDefinition));
      }
      return result;
    }
  }
}
