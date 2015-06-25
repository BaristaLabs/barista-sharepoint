namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPFarmManagedAccountCollectionConstructor : ClrFunction
  {
    public SPFarmManagedAccountCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFarmManagedAccountCollection", new SPFarmManagedAccountCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFarmManagedAccountCollectionInstance Construct()
    {
      return new SPFarmManagedAccountCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPFarmManagedAccountCollectionInstance : ObjectInstance
  {
    private readonly SPFarmManagedAccountCollection m_farmManagedAccountCollection;

    public SPFarmManagedAccountCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFarmManagedAccountCollectionInstance(ObjectInstance prototype, SPFarmManagedAccountCollection farmManagedAccountCollection)
      : this(prototype)
    {
      if (farmManagedAccountCollection == null)
        throw new ArgumentNullException("farmManagedAccountCollection");

      m_farmManagedAccountCollection = farmManagedAccountCollection;
    }

    public SPFarmManagedAccountCollection SPFarmManagedAccountCollection
    {
      get { return m_farmManagedAccountCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_farmManagedAccountCollection.Count;
      }
    }

    [JSFunction(Name = "getManagedAccountByUserName")]
    public SPManagedAccountInstance GetServiceInstanceByUserName(string userName)
    {
      var result = m_farmManagedAccountCollection[userName];

      return result == null
        ? null
        : new SPManagedAccountInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var managedAccount in m_farmManagedAccountCollection)
      {
        ArrayInstance.Push(result, new SPManagedAccountInstance(this.Engine.Object.InstancePrototype, managedAccount));
      }
      return result;
    }
  }
}
