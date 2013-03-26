namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPFileVersionCollectionConstructor : ClrFunction
  {
    public SPFileVersionCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFileVersionCollection", new SPFileVersionCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    public SPFileVersionCollectionInstance Construct(SPFileVersionCollection fileVersionCollection)
    {
      if (fileVersionCollection == null)
        throw new ArgumentNullException("fileVersionCollection");

      return new SPFileVersionCollectionInstance(this.InstancePrototype, fileVersionCollection);
    }
  }

  [Serializable]
  public class SPFileVersionCollectionInstance : ObjectInstance
  {
    private readonly SPFileVersionCollection m_fileVersionCollection;

    public SPFileVersionCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFileVersionCollectionInstance(ObjectInstance prototype, SPFileVersionCollection fileVersionCollection)
      : this(prototype)
    {
      this.m_fileVersionCollection = fileVersionCollection;
    }

    #region Properties
    [JSProperty(Name = "count")]
    public int Count
    {
      get { return m_fileVersionCollection.Count; }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "deleteAll")]
    public void DeleteAll()
    {
      m_fileVersionCollection.DeleteAll();
    }

    [JSFunction(Name = "deleteAllMinorVersions")]
    public void DeleteAllMinorVersions()
    {
      m_fileVersionCollection.DeleteAllMinorVersions();
    }

    [JSFunction(Name = "deleteByLabel")]
    public void DeleteByLabel(string label)
    {
      m_fileVersionCollection.DeleteByLabel(label);
    }

    [JSFunction(Name = "getAllVersions")]
    public ArrayInstance GetAllVersions()
    {
      var result = this.Engine.Array.Construct();
      foreach (var version in m_fileVersionCollection.OfType<SPFileVersion>())
      {
        ArrayInstance.Push(result, new SPFileVersionInstance(this.Engine.Object.InstancePrototype, version));
      }
      return result;
    }

    [JSFunction(Name = "getFile")]
    public SPFileInstance GetFile()
    {
      return new SPFileInstance(this.Engine.Object.InstancePrototype, m_fileVersionCollection.File);
    }

    [JSFunction(Name = "getWeb")]
    public SPWebInstance GetWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_fileVersionCollection.Web);
    }

    [JSFunction(Name = "recycleAll")]
    public void RecycleAll()
    {
      m_fileVersionCollection.RecycleAll();
    }

    [JSFunction(Name = "recycleAllMinorVersions")]
    public void RecycleAllMinorVersions()
    {
      m_fileVersionCollection.RecycleAllMinorVersions();
    }
    #endregion
  }
}
