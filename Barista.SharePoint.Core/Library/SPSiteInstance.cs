using System.Globalization;

namespace Barista.SharePoint.Library
{
  using Barista.SharePoint.Taxonomy.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.Office.Server.Utilities;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Taxonomy;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  [Serializable]
  public class SPSiteConstructor : ClrFunction
  {
    public SPSiteConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPSite", new SPSiteInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPSiteInstance Construct(string siteUrl)
    {
      SPSite site;

      if (SPHelper.TryGetSPSite(siteUrl, out site) == false)
        throw new JavaScriptException(this.Engine, "Error", "A site is not available at the specified url.");

      return new SPSiteInstance(this.InstancePrototype, site);
    }

    public SPSiteInstance Construct(SPSite site)
    {
      if (site == null)
        throw new ArgumentNullException("site");

      return new SPSiteInstance(this.InstancePrototype, site);
    }
  }

  [Serializable]
  public class SPSiteInstance : ObjectInstance, IDisposable
  {
    private readonly SPSite m_site;

    public SPSiteInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPSiteInstance(ObjectInstance prototype, SPSite site)
      : this(prototype)
    {
      this.m_site = site;
    }

    internal SPSite Site
    {
      get { return m_site; }
    }

    #region Properties
    [JSProperty(Name = "allowDesigner")]
    public bool AllowDesigner
    {
      get { return m_site.AllowDesigner; }
      set { m_site.AllowDesigner = value; }
    }

    [JSProperty(Name = "allowMasterPageEditing")]
    public bool AllowMasterPageEditing
    {
      get { return m_site.AllowMasterPageEditing; }
      set { m_site.AllowMasterPageEditing = value; }
    }

    [JSProperty(Name = "allowUnsafeUpdates")]
    public bool AllowUnsafeUpdates
    {
      get { return m_site.AllowUnsafeUpdates; }
      set { m_site.AllowUnsafeUpdates = value; }
    }

    [JSProperty(Name = "features")]
    public ArrayInstance Features
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var feature in m_site.Features)
        {
          ArrayInstance.Push(result, new SPFeatureInstance(this.Engine.Object.InstancePrototype, feature));
        }
        return result;
      }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_site.ID.ToString(); }
    }

    [JSProperty(Name = "maxItemsPerThrottledOperation")]
    public string MaxItemsPerThrottledOperation
    {
      get { return m_site.WebApplication.MaxItemsPerThrottledOperation.ToString(CultureInfo.InvariantCulture); }
    }

    [JSProperty(Name = "rootWeb")]
    public SPWebInstance RootWeb
    {
      get { return new SPWebInstance(this.Engine.Object.InstancePrototype, m_site.RootWeb); }
    }

    [JSProperty(Name = "serverRelativeUrl")]
    public string ServerRelativeUrl
    {
      get { return m_site.ServerRelativeUrl; }
    }

    [JSProperty(Name = "showUrlStructure")]
    public bool ShowUrlStructure
    {
      get { return m_site.ShowURLStructure; }
      set { m_site.ShowURLStructure = value; }
    }

    [JSProperty(Name = "uiVersionConfigurationEnabled")]
    public bool UIVersionConfigurationEnabled
    {
      get { return m_site.UIVersionConfigurationEnabled; }
      set { m_site.UIVersionConfigurationEnabled = value; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return m_site.Url; }
    }

    //TODO: Usage, UserCustomActions
    #endregion

    #region Functions
    //TODO: GetCatalog, GetChanges, GetCustomListTemplates
    [JSFunction(Name = "createWeb")]
    public SPWebInstance CreateWeb(object webCreationInfo)
    {
      SPWeb createdWeb;

      if (webCreationInfo is string)
        createdWeb = m_site.AllWebs.Add(webCreationInfo as string);
      else
      {
        var creationInfo = JurassicHelper.Coerce<SPWebCreationInformation>(this.Engine, webCreationInfo);

        if (creationInfo.WebTemplate is string)
          createdWeb = m_site.AllWebs.Add(creationInfo.Url, creationInfo.Title, creationInfo.Description, (uint)creationInfo.Language, creationInfo.WebTemplate as string, !creationInfo.UseSamePermissionsAsParentSite, creationInfo.ConvertIfThere);
        else
        {
          //attempt to get an instance of a web template from the original object.
          var webCreationInstance = webCreationInfo as ObjectInstance;
          if (webCreationInstance == null)
            throw new JavaScriptException(this.Engine, "Error", "Unable to create a web from the specified template. Could not determine the value of the web template.");

          if (webCreationInstance.HasProperty("webTemplate") == false)
            throw new JavaScriptException(this.Engine, "Error", "Unable to create a web from the specified template. Web Template property was null.");

          var webTemplate = JurassicHelper.Coerce<SPWebTemplateInstance>(this.Engine, webCreationInstance.GetPropertyValue("webTemplate"));
          createdWeb = m_site.AllWebs.Add(creationInfo.Url, creationInfo.Title, creationInfo.Description, (uint)creationInfo.Language, webTemplate.WebTemplate, !creationInfo.UseSamePermissionsAsParentSite, creationInfo.ConvertIfThere);
        }
      }
      
      return new SPWebInstance(this.Engine.Object.InstancePrototype, createdWeb);
    }

    [JSFunction(Name = "getAllWebs")]
    public ArrayInstance GetAllWebs()
    {
      List<SPWeb> webs = new List<SPWeb>();

      ContentIterator ci = new ContentIterator();
      ci.ProcessSite(m_site, true, webs.Add,
        (web, ex) => false);
      
      var result = this.Engine.Array.Construct();
      foreach (var web in webs)
      {
        ArrayInstance.Push(result, new SPWebInstance(this.Engine.Object.InstancePrototype, web));
      }
      return result;
    }

    [JSFunction(Name = "getFeatureDefinitions")]
    public ArrayInstance GetFeatureDefinitions()
    {
      //SPSite.FeatureDefinitions always returns null... nice, SharePoint, nice...

      var result = this.Engine.Array.Construct();
      foreach (SPFeatureDefinition featureDefinition in SPFarm.Local.FeatureDefinitions)
      {
        if (featureDefinition.Scope == SPFeatureScope.Site)
       {
         ArrayInstance.Push(result, new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, featureDefinition));
        }
      }
      return result;
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, this.m_site.RootWeb);
    }

    [JSFunction(Name = "getRecycleBin")]
    public SPRecycleBinItemCollectionInstance GetRecycleBin()
    {
      return new SPRecycleBinItemCollectionInstance(this.Engine.Object.InstancePrototype, m_site.RecycleBin);
    }

    [JSFunction(Name = "getTaxonomySession")]
    public TaxonomySessionInstance GetTaxonomySession()
    {
      var session = new TaxonomySession(m_site);
      return new TaxonomySessionInstance(this.Engine.Object.InstancePrototype, session);
    }

    [JSFunction(Name = "getWebTemplates")]
    public ArrayInstance GetWebTemplates(object language)
    {
      uint lcid = (uint)System.Threading.Thread.CurrentThread.CurrentCulture.LCID;

// ReSharper disable PossibleInvalidCastException
      if (language is int)
        lcid = (uint)language;
// ReSharper restore PossibleInvalidCastException

      var result = this.Engine.Array.Construct();
      var webTemplates = m_site.GetWebTemplates(lcid).OfType<SPWebTemplate>();
      foreach (var webTemplate in webTemplates)
      {
        ArrayInstance.Push(result, new SPWebTemplateInstance(this.Engine.Object.InstancePrototype, webTemplate));
      }
      return result;
    }

    [JSFunction(Name = "getUsageInfo")]
    public UsageInfoInstance GetUsage()
    {
      return new UsageInfoInstance(this.Engine.Object.InstancePrototype, m_site.Usage);
    }

    [JSFunction(Name = "openWeb")]
    public SPWebInstance OpenWeb(string url)
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_site.OpenWeb(url, false));
    }

    [JSFunction(Name = "openWebById")]
    public SPWebInstance OpenWebById(string id)
    {
      Guid webId = new Guid(id);

      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_site.OpenWeb(webId));
    }

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      if (m_site != null)
        m_site.Dispose();
    }
    #endregion
  }
}
