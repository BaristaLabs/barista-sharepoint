namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPListTemplateCollectionConstructor : ClrFunction
  {
    public SPListTemplateCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPListTemplateCollection", new SPListTemplateCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListTemplateCollectionInstance Construct()
    {
      return new SPListTemplateCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPListTemplateCollectionInstance : ObjectInstance
  {
    private readonly SPListTemplateCollection m_listTemplateCollection;

    public SPListTemplateCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListTemplateCollectionInstance(ObjectInstance prototype, SPListTemplateCollection listTemplateCollection)
      : this(prototype)
    {
      if (listTemplateCollection == null)
        throw new ArgumentNullException("listTemplateCollection");

      m_listTemplateCollection = listTemplateCollection;
    }

    public SPListTemplateCollection SPListTemplateCollection
    {
      get { return m_listTemplateCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_listTemplateCollection.Count;
      }
    }

    [JSFunction(Name = "getListTemplateByName")]
    public SPListTemplateInstance GetListTemplateByName(string name)
    {
      var listTemplate = m_listTemplateCollection[name];
      if (listTemplate == null)
        return null;

      return new SPListTemplateInstance(this.Engine.Object.InstancePrototype, listTemplate);
    }

    [JSFunction(Name = "getListTemplateByIndex")]
    public SPListTemplateInstance GetListTemplateByIndex(int index)
    {
      var listTemplate = m_listTemplateCollection[index];
      if (listTemplate == null)
        return null;

      return new SPListTemplateInstance(this.Engine.Object.InstancePrototype, listTemplate);
    }

    [JSFunction(Name = "getSchemaXml")]
    public string GetSchemaXml()
    {
      return m_listTemplateCollection.SchemaXml;
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var listTemplate in m_listTemplateCollection.OfType<SPListTemplate>())
      {
        ArrayInstance.Push(result, new SPListTemplateInstance(this.Engine.Object.InstancePrototype, listTemplate));
      }
      return result;
    }
  }
}
