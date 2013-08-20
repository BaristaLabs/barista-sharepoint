namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPDeletedSiteConstructor : ClrFunction
  {
    public SPDeletedSiteConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDeletedSite", new SPDeletedSiteInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDeletedSiteInstance Construct()
    {
      return new SPDeletedSiteInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPDeletedSiteInstance : ObjectInstance
  {
    private readonly SPDeletedSite m_deletedSite;

    public SPDeletedSiteInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDeletedSiteInstance(ObjectInstance prototype, SPDeletedSite deletedSite)
      : this(prototype)
    {
      if (deletedSite == null)
        throw new ArgumentNullException("deletedSite");

      m_deletedSite = deletedSite;
    }

    public SPDeletedSite SPDeletedSite
    {
      get { return m_deletedSite; }
    }

    [JSProperty(Name = "databaseId")]
    public GuidInstance DatabaseId
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_deletedSite.DatabaseId);
      }
    }

    [JSProperty(Name = "deletionTime")]
    public DateInstance DeletionTime
    {
      get
      {
        return JurassicHelper.ToDateInstance(this.Engine, m_deletedSite.DeletionTime);
      }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get
      {
        return m_deletedSite.Path;
      }
    }

    [JSProperty(Name = "siteId")]
    public GuidInstance SiteId
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_deletedSite.SiteId);
      }
    }

    [JSProperty(Name = "siteSubscriptionId")]
    public GuidInstance SiteSubscriptionId
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_deletedSite.SiteSubscriptionId);
      }
    }

    [JSProperty(Name = "webApplicationId")]
    public GuidInstance WebApplicationId
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_deletedSite.WebApplicationId);
      }
    }

    [JSFunction(Name ="delete")]
    public void Delete()
    {
      m_deletedSite.Delete();
    }

    [JSFunction(Name = "restore")]
    public void Restore()
    {
      m_deletedSite.Restore();
    }
  }
}
