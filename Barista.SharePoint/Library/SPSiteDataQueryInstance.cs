namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.Office.DocumentManagement.DocumentSets;
  using System.Collections;
  using System.Text;
  using System.Collections.Generic;
  using Microsoft.Office.Server.Utilities;
  using Barista.Library;

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

  public class SPSiteDataQueryInstance : ObjectInstance
  {
    SPSiteDataQuery m_siteDataQuery;

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
      get { return m_siteDataQuery.RowLimit.ToString(); }
      set { m_siteDataQuery.RowLimit = uint.Parse(value); }
    }

    [JSProperty(Name = "rowLimit")]
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
