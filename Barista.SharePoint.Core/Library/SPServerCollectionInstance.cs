namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPServerCollectionConstructor : ClrFunction
  {
    public SPServerCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPServerCollection", new SPServerCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPServerCollectionInstance Construct()
    {
      return new SPServerCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class SPServerCollectionInstance : ObjectInstance
  {
    private readonly SPServerCollection m_serverCollection;

    public SPServerCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPServerCollectionInstance(ObjectInstance prototype, SPServerCollection serverCollection)
      : this(prototype)
    {
      if (serverCollection == null)
        throw new ArgumentNullException("serverCollection");

      m_serverCollection = serverCollection;
    }

    public SPServerCollection SPServerCollection
    {
      get { return m_serverCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get { return m_serverCollection.Count; }
    }

    [JSFunction(Name = "getServerById")]
    public SPServerInstance GetServerById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_serverCollection[guid];
      return result == null
        ? null
        : new SPServerInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getServerByName")]
    public SPServerInstance GetServerByName(string name)
    {
      var result = m_serverCollection[name];
      return result == null
        ? null
        : new SPServerInstance(Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+SPServer]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var server in m_serverCollection)
      {
        ArrayInstance.Push(result, new SPServerInstance(Engine.Object.InstancePrototype, server));
      }
      return result;
    }
  }
}
