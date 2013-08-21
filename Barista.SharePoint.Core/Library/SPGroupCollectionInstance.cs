namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPGroupCollectionConstructor : ClrFunction
  {
    public SPGroupCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPGroupCollection", new SPGroupCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPGroupCollectionInstance Construct()
    {
      return new SPGroupCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPGroupCollectionInstance : ObjectInstance
  {
    private readonly SPGroupCollection m_groupCollection;

    public SPGroupCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPGroupCollectionInstance(ObjectInstance prototype, SPGroupCollection groupCollection)
      : this(prototype)
    {
      if (groupCollection == null)
        throw new ArgumentNullException("groupCollection");

      m_groupCollection = groupCollection;
    }

    public SPGroupCollection SPGroupCollection
    {
      get { return m_groupCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count 
    {
      get { return m_groupCollection.Count; }
    }

    [JSFunction(Name = "getGroupById")]
    public SPGroupInstance GetGroupById(int id)
    {
      var result = m_groupCollection.GetByID(id);
      return result == null
        ? null
        : new SPGroupInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getGroupByIndex")]
    public SPGroupInstance GetGroupByIndex(int index)
    {
      var result = m_groupCollection[index];
      return result == null
        ? null
        : new SPGroupInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getGroupByName")]
    public SPGroupInstance GetGroupByName(string name)
    {
      var result = m_groupCollection[name];
      return result == null
        ? null
        : new SPGroupInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();

      foreach (var group in m_groupCollection.OfType<SPGroup>())
      {
        ArrayInstance.Push(result, new SPGroupInstance(this.Engine.Object.InstancePrototype, group));
      }
      return result;
    }
  }
}
