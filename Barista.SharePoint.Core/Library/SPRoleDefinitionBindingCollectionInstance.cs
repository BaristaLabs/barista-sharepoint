namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Linq;
  using System;

  [Serializable]
  public class SPRoleDefinitionBindingCollectionConstructor : ClrFunction
  {
    public SPRoleDefinitionBindingCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPRoleDefinitionBindingCollection", new SPRoleDefinitionBindingCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPRoleDefinitionBindingCollectionInstance Construct()
    {
      return new SPRoleDefinitionBindingCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPRoleDefinitionBindingCollectionInstance : ObjectInstance
  {
    private readonly SPRoleDefinitionBindingCollection m_roleDefinitionBindingCollection;

    public SPRoleDefinitionBindingCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPRoleDefinitionBindingCollectionInstance(ObjectInstance prototype, SPRoleDefinitionBindingCollection roleDefinitionBindingCollection)
      : this(prototype)
    {
      if (roleDefinitionBindingCollection == null)
        throw new ArgumentNullException("roleDefinitionBindingCollection");

      m_roleDefinitionBindingCollection = roleDefinitionBindingCollection;
    }

    public SPRoleDefinitionBindingCollection SPRoleDefinitionBindingCollection
    {
      get { return m_roleDefinitionBindingCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_roleDefinitionBindingCollection.Count;
      }
    }

    [JSFunction(Name = "add")]
    public void Add(SPRoleDefinitionInstance roleDefinition)
    {
      if (roleDefinition == null)
        return;

      m_roleDefinitionBindingCollection.Add(roleDefinition.RoleDefinition);
    }

    [JSFunction(Name = "contains")]
    public bool Contains(SPRoleDefinitionInstance roleDefinition)
    {
      if (roleDefinition == null)
        return false;

      return m_roleDefinitionBindingCollection.Contains(roleDefinition.RoleDefinition);
    }

    [JSFunction(Name = "getXml")]
    public string GetXml()
    {
      return m_roleDefinitionBindingCollection.Xml;
    }

    [JSFunction(Name = "removeRoleDefinition")]
    public void Remove(SPRoleDefinitionInstance roleDefinition)
    {
      if (roleDefinition == null)
        return;

      m_roleDefinitionBindingCollection.Remove(roleDefinition.RoleDefinition);
    }

    [JSFunction(Name = "removeByIndex")]
    public void RemoveByIndex(int index)
    {
      m_roleDefinitionBindingCollection.Remove(index);
    }

    [JSFunction(Name = "removeAll")]
    public void RemoveAll(int index)
    {
      m_roleDefinitionBindingCollection.RemoveAll();
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+SPRoleDefinition]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var roleDefinition in m_roleDefinitionBindingCollection.OfType<SPRoleDefinition>())
      {
        ArrayInstance.Push(result, new SPRoleDefinitionInstance(Engine.Object.InstancePrototype, roleDefinition));
      }
      return result;
    }
  }
}
