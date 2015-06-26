namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPServiceCollectionConstructor : ClrFunction
  {
    public SPServiceCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServiceCollection", new SPServiceCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServiceCollectionInstance Construct()
    {
      return new SPServiceCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPServiceCollectionInstance : ObjectInstance
  {
    private readonly SPServiceCollection m_serviceCollection;

    public SPServiceCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPServiceCollectionInstance(ObjectInstance prototype, SPServiceCollection serviceCollection)
      : this(prototype)
    {
      if (serviceCollection == null)
        throw new ArgumentNullException("serviceCollection");

      m_serviceCollection = serviceCollection;
    }

    public SPServiceCollection SPServiceCollection
    {
      get { return m_serviceCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get { return m_serviceCollection.Count; }
    }

    [JSFunction(Name = "getServiceById")]
    public SPServiceInstance GetServiceById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_serviceCollection[guid];
      return result == null
        ? null
        : new SPServiceInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getServiceByName")]
    public SPServiceInstance GetServiceByName(string name)
    {
      var result = m_serviceCollection[name];
      return result == null
        ? null
        : new SPServiceInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+SPService]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var service in m_serviceCollection)
      {
        ArrayInstance.Push(result, new SPServiceInstance(Engine.Object.InstancePrototype, service));
      }
      return result;
    }
  }
}
