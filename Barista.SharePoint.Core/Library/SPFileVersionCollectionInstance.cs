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
    [JSFunction(Name = "delete")]
    public void Delete(int index)
    {
      m_fileVersionCollection.Delete(index);
    }

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

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
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

    [JSFunction(Name = "getVersionFromId")]
    public object GetVersionByLabel(int id)
    {
      var version = m_fileVersionCollection.GetVersionFromID(id);
      if (version == null)
        return Null.Value;

      return new SPFileVersionInstance(this.Engine.Object.InstancePrototype, version);
    }

    [JSFunction(Name = "getVersionFromLabel")]
    public object GetVersionByLabel(string versionLabel)
    {
      var version = m_fileVersionCollection.GetVersionFromLabel(versionLabel);
      if (version == null)
        return Null.Value;

      return new SPFileVersionInstance(this.Engine.Object.InstancePrototype, version);
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

    [JSFunction(Name = "restore")]
    public void Restore(int index)
    {
      m_fileVersionCollection.Restore(index);
    }

    [JSFunction(Name = "restoreById")]
    public void RestoreById(int versionId, object bypassSharedLockId)
    {
      if (bypassSharedLockId == Undefined.Value || bypassSharedLockId == Null.Value || bypassSharedLockId == null)
        m_fileVersionCollection.RestoreByID(versionId);
      else
       m_fileVersionCollection.RestoreByID(versionId, TypeConverter.ToString(bypassSharedLockId));
    }

    [JSFunction(Name = "restoreByLabel")]
    public void RestoreByLabel(string versionLabel)
    {
      m_fileVersionCollection.RestoreByLabel(versionLabel);
    }
    #endregion
  }
}
