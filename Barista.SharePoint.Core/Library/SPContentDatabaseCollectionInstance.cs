namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPContentDatabaseCollectionConstructor : ClrFunction
  {
    public SPContentDatabaseCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPContentDatabaseCollection", new SPContentDatabaseCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPContentDatabaseCollectionInstance Construct()
    {
      return new SPContentDatabaseCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPContentDatabaseCollectionInstance : ObjectInstance
  {
    private readonly SPContentDatabaseCollection m_contentDatabaseCollection;

    public SPContentDatabaseCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPContentDatabaseCollectionInstance(ObjectInstance prototype, SPContentDatabaseCollection contentDatabaseCollection)
      : this(prototype)
    {
      if (contentDatabaseCollection == null)
        throw new ArgumentNullException("contentDatabaseCollection");

      m_contentDatabaseCollection = contentDatabaseCollection;
    }

    public SPContentDatabaseCollection SPContentDatabaseCollection
    {
      get { return m_contentDatabaseCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_contentDatabaseCollection.Count;
      }
    }

    [JSFunction(Name = "getContentDatabaseByGuid")]
    public SPContentDatabaseInstance GetServiceInstanceByGuid(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_contentDatabaseCollection[guid];

      return result == null
        ? null
        : new SPContentDatabaseInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "getContentDatabaseByIndex")]
    public SPContentDatabaseInstance GetServerByIndex(int index)
    {
      var result = m_contentDatabaseCollection[index];
      return result == null
        ? null
        : new SPContentDatabaseInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentDatabase in m_contentDatabaseCollection)
      {
        ArrayInstance.Push(result, new SPContentDatabaseInstance(this.Engine.Object.InstancePrototype, contentDatabase));
      }
      return result;
    }
  }
}
