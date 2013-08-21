namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint.Administration.Backup;

  [Serializable]
  public class SPDatabaseSnapshotCollectionConstructor : ClrFunction
  {
    public SPDatabaseSnapshotCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDatabaseSnapshotCollection", new SPDatabaseSnapshotCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDatabaseSnapshotCollectionInstance Construct()
    {
      return new SPDatabaseSnapshotCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPDatabaseSnapshotCollectionInstance : ObjectInstance
  {
    private readonly SPDatabaseSnapshotCollection m_databaseSnapshotCollection;

    public SPDatabaseSnapshotCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDatabaseSnapshotCollectionInstance(ObjectInstance prototype, SPDatabaseSnapshotCollection databaseSnapshotCollection)
      : this(prototype)
    {
      if (databaseSnapshotCollection == null)
        throw new ArgumentNullException("databaseSnapshotCollection");

      m_databaseSnapshotCollection = databaseSnapshotCollection;
    }

    public SPDatabaseSnapshotCollection SPDatabaseSnapshotCollection
    {
      get { return m_databaseSnapshotCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_databaseSnapshotCollection.Count;
      }
    }

    [JSProperty(Name = "creationInterval")]
    public int CreationInterval
    {
      get
      {
        return m_databaseSnapshotCollection.CreationInterval;
      }
      set
      {
        m_databaseSnapshotCollection.CreationInterval = (short)value;
      }
    }

    [JSProperty(Name = "enabledManagement")]
    public bool EnabledManagement
    {
      get
      {
        return m_databaseSnapshotCollection.EnabledManagement;
      }
      set
      {
        m_databaseSnapshotCollection.EnabledManagement = value;
      }
    }

    [JSProperty(Name = "isSnapshotSupported")]
    public bool IsSnapshotSupported
    {
      get
      {
        return m_databaseSnapshotCollection.IsSnapshotSupported;
      }
    }

    [JSProperty(Name = "maximumRetention")]
    public int MaximumRetention
    {
      get
      {
        return m_databaseSnapshotCollection.MaximumRetention;
      }
      set
      {
        m_databaseSnapshotCollection.MaximumRetention = (short)value;
      }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get
      {
        return m_databaseSnapshotCollection.Path;
      }
      set
      {
        m_databaseSnapshotCollection.Path = value;
      }
    }

    [JSProperty(Name = "snapshotLimit")]
    public int SnapshotLimit
    {
      get
      {
        return m_databaseSnapshotCollection.SnapshotLimit;
      }
      set
      {
        m_databaseSnapshotCollection.SnapshotLimit = (short)value;
      }
    }

    [JSFunction(Name = "createSnapshot")]
    public SPDatabaseSnapshotInstance CreateSnapshot()
    {
      var snapshot = m_databaseSnapshotCollection.CreateSnapshot();
      return snapshot == null
        ? null
        : new SPDatabaseSnapshotInstance(this.Engine.Object.InstancePrototype, snapshot);
    }

    [JSFunction(Name = "deleteSnapshots")]
    public void DeleteSnapshots()
    {
      m_databaseSnapshotCollection.DeleteSnapshots();
    }

    [JSFunction(Name = "getSnapshotByName")]
    public SPDatabaseSnapshotInstance GetSnapshotByName(string snapshotName)
    {
      var snapshot = m_databaseSnapshotCollection[snapshotName];
      return snapshot == null
        ? null
        : new SPDatabaseSnapshotInstance(this.Engine.Object.InstancePrototype, snapshot);
    }

    [JSFunction(Name = "getSnapshotByIndex")]
    public SPDatabaseSnapshotInstance GetSnapshotByIndex(int index)
    {
      var snapshot = m_databaseSnapshotCollection[index];
      return snapshot == null
        ? null
        : new SPDatabaseSnapshotInstance(this.Engine.Object.InstancePrototype, snapshot);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      object[] snapshotInstances = this.m_databaseSnapshotCollection
        .Select(ss => new SPDatabaseSnapshotInstance(this.Engine.Object.InstancePrototype, ss))
        .ToArray();

      return this.Engine.Array.Construct(snapshotInstances);
    }

    [JSFunction(Name = "refreshSnapshots")]
    public void RefreshSnapshots()
    {
      m_databaseSnapshotCollection.RefreshSnapshots();
    }
  }
}
