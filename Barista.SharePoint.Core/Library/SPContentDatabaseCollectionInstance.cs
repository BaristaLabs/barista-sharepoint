namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
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
  }
}
