namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPServiceApplicationCollectionConstructor : ClrFunction
  {
    public SPServiceApplicationCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServiceApplicationCollection", new SPServiceApplicationCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServiceApplicationCollectionInstance Construct()
    {
      return new SPServiceApplicationCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPServiceApplicationCollectionInstance : ObjectInstance
  {
    private readonly SPServiceApplicationCollection m_serviceApplicationCollection;

    public SPServiceApplicationCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPServiceApplicationCollectionInstance(ObjectInstance prototype, SPServiceApplicationCollection serviceApplicationCollection)
      : this(prototype)
    {
      if (serviceApplicationCollection == null)
        throw new ArgumentNullException("serviceApplicationCollection");

      m_serviceApplicationCollection = serviceApplicationCollection;
    }

    public SPServiceApplicationCollection SPServiceApplicationCollection
    {
      get { return m_serviceApplicationCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_serviceApplicationCollection.Count;
      }
    }

    [JSFunction(Name = "getServiceApplicationById")]
    public ObjectInstance GetServiceApplicationById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_serviceApplicationCollection[guid];

      if (result == null)
        return null;

      if (result is SPIisWebServiceApplication)
        return new SPIisWebServiceApplicationInstance(Engine.Object.Prototype,
        (result as SPIisWebServiceApplication));

      return new SPServiceApplicationInstance(Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "getServiceApplicationByName")]
    public ObjectInstance GetServerByName(string name)
    {
      var result = m_serviceApplicationCollection[name];
      if (result == null)
        return null;

      if (result is SPIisWebServiceApplication)
        return new SPIisWebServiceApplicationInstance(Engine.Object.Prototype,
        (result as SPIisWebServiceApplication));

      return new SPServiceApplicationInstance(Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var serviceApplication in m_serviceApplicationCollection)
      {
        if (serviceApplication is SPIisWebServiceApplication)
          ArrayInstance.Push(result, new SPIisWebServiceApplicationInstance(Engine.Object.Prototype, serviceApplication as SPIisWebServiceApplication));
        else
        {
          ArrayInstance.Push(result, new SPServiceApplicationInstance(Engine.Object.InstancePrototype, serviceApplication));
        }
      }
      return result;
    }
  }
}
