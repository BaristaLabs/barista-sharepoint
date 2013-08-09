namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration.Backup;
  using System;

  [Serializable]
  public class SPDatabaseSnapshotConstructor : ClrFunction
  {
    public SPDatabaseSnapshotConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDatabaseSnapshot", new SPDatabaseSnapshotInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDatabaseSnapshotInstance Construct()
    {
      return new SPDatabaseSnapshotInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPDatabaseSnapshotInstance : ObjectInstance
  {
    private readonly SPDatabaseSnapshot m_databaseSnapshot;

    public SPDatabaseSnapshotInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDatabaseSnapshotInstance(ObjectInstance prototype, SPDatabaseSnapshot databaseSnapshot)
      : this(prototype)
    {
      if (databaseSnapshot == null)
        throw new ArgumentNullException("databaseSnapshot");

      m_databaseSnapshot = databaseSnapshot;
    }

    public SPDatabaseSnapshot SPDatabaseSnapshot
    {
      get { return m_databaseSnapshot; }
    }
  }
}
