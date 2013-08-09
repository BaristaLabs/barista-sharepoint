namespace Barista.SharePoint.Library
{
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
  }
}
