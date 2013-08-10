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

    [JSProperty(Name = "age")]
    public int Age
    {
      get { return m_databaseSnapshot.Age; }
    }

    [JSProperty(Name = "connectionString")]
    public string ConnectionString
    {
      get
      {
        return m_databaseSnapshot.ConnectionString == null
          ? String.Empty
          : m_databaseSnapshot.ConnectionString.ToString();
      }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_databaseSnapshot.Created); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_databaseSnapshot.Name; }
    }

    [JSFunction(Name = "delete")]
    public void Delete(object force)
    {
      if (force == null || force == Null.Value || force == Undefined.Value)
        m_databaseSnapshot.Delete();
      else
        m_databaseSnapshot.Delete(TypeConverter.ToBoolean(force));
    }

    [JSFunction(Name = "restore")]
    public void Restore(object force)
    {
      if (force == null || force == Null.Value || force == Undefined.Value)
        m_databaseSnapshot.Restore();
      else
        m_databaseSnapshot.Restore(TypeConverter.ToBoolean(force));
    }
  }
}
