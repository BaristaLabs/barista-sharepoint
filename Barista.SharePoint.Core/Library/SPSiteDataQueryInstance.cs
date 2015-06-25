namespace Barista.SharePoint.Library
{
  using System;
  using System.Globalization;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPSiteDataQueryConstructor : ClrFunction
  {
    public SPSiteDataQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPSiteDataQuery", new SPSiteDataQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPSiteDataQueryInstance Construct()
    {
      return new SPSiteDataQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPSiteDataQueryInstance : ObjectInstance
  {
    private readonly SPSiteDataQuery m_siteDataQuery;

    public SPSiteDataQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();

      m_siteDataQuery = new SPSiteDataQuery();
    }

    internal SPSiteDataQuery SiteDataQuery
    {
      get { return m_siteDataQuery; }
    }

    #region Properties
    [JSProperty(Name = "lists")]
    public string Lists
    {
      get { return m_siteDataQuery.Lists; }
      set { m_siteDataQuery.Lists = value; }
    }

    [JSProperty(Name = "query")]
    public string Query
    {
      get { return m_siteDataQuery.Query; }
      set { m_siteDataQuery.Query = value; }
    }

    [JSProperty(Name = "queryThrottleMode")]
    public string QueryThrottleMode
    {
      get { return m_siteDataQuery.QueryThrottleMode.ToString(); }
      set { m_siteDataQuery.QueryThrottleMode = (SPQueryThrottleOption)Enum.Parse(typeof(SPQueryThrottleOption), value); }
    }

    [JSProperty(Name = "rowLimit")]
    public string RowLimit
    {
      get { return m_siteDataQuery.RowLimit.ToString(CultureInfo.InvariantCulture); }
      set { m_siteDataQuery.RowLimit = uint.Parse(value); }
    }

    [JSProperty(Name = "viewFields")]
    public string ViewFields
    {
      get { return m_siteDataQuery.ViewFields; }
      set { m_siteDataQuery.ViewFields = value; }
    }

    [JSProperty(Name = "webs")]
    public string Webs
    {
      get { return m_siteDataQuery.Webs; }
      set { m_siteDataQuery.Webs = value; }
    }
    #endregion
  }
}
