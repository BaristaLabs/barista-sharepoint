namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPServiceInstanceDependencyCollectionConstructor : ClrFunction
  {
    public SPServiceInstanceDependencyCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServiceInstanceDependencyCollection", new SPServiceInstanceDependencyCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServiceInstanceDependencyCollectionInstance Construct()
    {
      return new SPServiceInstanceDependencyCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPServiceInstanceDependencyCollectionInstance : ObjectInstance
  {
    private readonly SPServiceInstanceDependencyCollection m_serviceInstanceDependencyCollection;

    public SPServiceInstanceDependencyCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPServiceInstanceDependencyCollectionInstance(ObjectInstance prototype, SPServiceInstanceDependencyCollection serviceInstanceDependencyCollection)
      : this(prototype)
    {
      if (serviceInstanceDependencyCollection == null)
        throw new ArgumentNullException("serviceInstanceDependencyCollection");

      m_serviceInstanceDependencyCollection = serviceInstanceDependencyCollection;
    }

    public SPServiceInstanceDependencyCollection SPServiceInstanceDependencyCollection
    {
      get { return m_serviceInstanceDependencyCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_serviceInstanceDependencyCollection.Count;
      }
    }

    [JSFunction(Name = "getServiceInstanceByGuid")]
    public SPServiceInstanceInstance GetServiceInstanceByGuid(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_serviceInstanceDependencyCollection[guid];

      return result == null
        ? null
        : new SPServiceInstanceInstance(Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "getServiceInstanceByName")]
    public SPServiceInstanceInstance GetServerByName(string name)
    {
      var result = m_serviceInstanceDependencyCollection[name];
      return result == null
        ? null
        : new SPServiceInstanceInstance(Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var serviceInstance in m_serviceInstanceDependencyCollection)
      {
        ArrayInstance.Push(result, new SPServiceInstanceInstance(Engine.Object.InstancePrototype, serviceInstance));
      }
      return result;
    }
  }
}
