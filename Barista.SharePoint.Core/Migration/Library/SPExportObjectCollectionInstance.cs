namespace Barista.SharePoint.Migration.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint.Deployment;

  [Serializable]
  public class SPExportObjectCollectionConstructor : ClrFunction
  {
    public SPExportObjectCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPExportObjectCollection", new SPExportObjectCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPExportObjectCollectionInstance Construct()
    {
      return new SPExportObjectCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPExportObjectCollectionInstance : ObjectInstance
  {
    private readonly SPExportObjectCollection m_exportObjectCollection;

    public SPExportObjectCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPExportObjectCollectionInstance(ObjectInstance prototype, SPExportObjectCollection exportObjectCollection)
      : this(prototype)
    {
      if (exportObjectCollection == null)
        throw new ArgumentNullException("exportObjectCollection");

      m_exportObjectCollection = exportObjectCollection;
    }

    public SPExportObjectCollection SPExportObjectCollection
    {
      get { return m_exportObjectCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_exportObjectCollection.Count;
      }
    }

    [JSFunction(Name = "add")]
    public void Add(SPExportObjectInstance exportObj)
    {
      if (exportObj == null)
        throw new JavaScriptException(this.Engine, "Error", "Cannot add a null Export Object object to the collection.");

      m_exportObjectCollection.Add(exportObj.SPExportObject);
    }

    [JSFunction(Name = "clear")]
    public void Clear()
    {
      m_exportObjectCollection.Clear();
    }

    [JSFunction(Name = "getExportObjectInstanceByIndex")]
    public SPExportObjectInstance GetSPExportObjectInstanceByIndex(int index)
    {
      var exportObjectInstance = m_exportObjectCollection[index];
      return exportObjectInstance == null
        ? null
        : new SPExportObjectInstance(this.Engine.Object.InstancePrototype, exportObjectInstance);
    }

    [JSFunction(Name = "getAllExportObjects")]
    public ArrayInstance GetAllTermSets()
    {
      var result = this.Engine.Array.Construct();
      foreach (var exportObject in m_exportObjectCollection.OfType<SPExportObject>())
      {
        ArrayInstance.Push(result, new SPExportObjectInstance(this.Engine.Object.InstancePrototype, exportObject));
      }
      return result;
    }
  }
}
