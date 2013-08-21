namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPDeletedSiteCollectionConstructor : ClrFunction
  {
    public SPDeletedSiteCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDeletedSiteCollection", new SPDeletedSiteCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDeletedSiteCollectionInstance Construct()
    {
      return new SPDeletedSiteCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPDeletedSiteCollectionInstance : ObjectInstance
  {
    private readonly SPDeletedSiteCollection m_deletedSiteCollection;

    public SPDeletedSiteCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDeletedSiteCollectionInstance(ObjectInstance prototype, SPDeletedSiteCollection deletedSiteCollection)
      : this(prototype)
    {
      if (deletedSiteCollection == null)
        throw new ArgumentNullException("deletedSiteCollection");

      m_deletedSiteCollection = deletedSiteCollection;
    }

    public SPDeletedSiteCollection DeletedSiteCollection
    {
      get { return m_deletedSiteCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_deletedSiteCollection.Count;
      }
    }

    [JSProperty(Name = "rowLimit")]
    public int RowLimit
    {
      get
      {
        return m_deletedSiteCollection.RowLimit;
      }
    }

    [JSFunction(Name = "getDeletedSiteByIndex")]
    public SPDeletedSiteInstance GetSiteByIndex(int index)
    {
      var deletedSite = m_deletedSiteCollection[index];
      return deletedSite == null
        ? null
        : new SPDeletedSiteInstance(this.Engine.Object, deletedSite);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var deletedSite in m_deletedSiteCollection.OfType<SPDeletedSite>())
      {
        ArrayInstance.Push(result, new SPDeletedSiteInstance(this.Engine.Object.InstancePrototype, deletedSite));
      }
      return result;
    }
  }
}
