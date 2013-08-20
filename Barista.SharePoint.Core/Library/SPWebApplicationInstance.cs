namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPWebApplicationConstructor : ClrFunction
  {
    public SPWebApplicationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWebApplication", new SPWebApplicationInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWebApplicationInstance Construct()
    {
      return new SPWebApplicationInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPWebApplicationInstance : ObjectInstance
  {
    private readonly SPWebApplication m_webApplication;

    public SPWebApplicationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWebApplicationInstance(ObjectInstance prototype, SPWebApplication webApplication)
      : this(prototype)
    {
      if (webApplication == null)
        throw new ArgumentNullException("webApplication");

      m_webApplication = webApplication;
    }

    public SPWebApplication SPWebApplication
    {
      get { return m_webApplication; }
    }

    #region Properties
    [JSProperty(Name = "alertsEnabled")]
    public bool AlertsEnabled
    {
      get
      {
        return m_webApplication.AlertsEnabled;
      }
      set
      {
        m_webApplication.AlertsEnabled = value;
      }
    }

    [JSProperty(Name = "alertsLimited")]
    public bool AlertsLimited
    {
      get
      {
        return m_webApplication.AlertsLimited;
      }
      set
      {
        m_webApplication.AlertsLimited = value;
      }
    }

    [JSProperty(Name = "alertsMaximum")]
    public int AlertsMaximum
    {
      get
      {
        return m_webApplication.AlertsMaximum;
      }
      set
      {
        m_webApplication.AlertsMaximum = value;
      }
    }

    [JSProperty(Name = "alertsMaximumQuerySet")]
    public int AlertsMaximumQuerySet
    {
      get
      {
        return m_webApplication.AlertsMaximumQuerySet;
      }
      set
      {
        m_webApplication.AlertsMaximumQuerySet = value;
      }
    }

    [JSProperty(Name = "allowAccessToWebPartCatalog")]
    public bool AllowAccessToWebPartCatalog
    {
      get
      {
        return m_webApplication.AllowAccessToWebPartCatalog;
      }
      set
      {
        m_webApplication.AllowAccessToWebPartCatalog = value;
      }
    }

    [JSProperty(Name = "allowContributorsToEditScriptableParts")]
    public bool AllowContributorsToEditScriptableParts
    {
      get
      {
        return m_webApplication.AllowContributorsToEditScriptableParts;
      }
      set
      {
        m_webApplication.AllowContributorsToEditScriptableParts = value;
      }
    }

    [JSProperty(Name = "allowDesigner")]
    public bool AllowDesigner
    {
      get
      {
        return m_webApplication.AllowDesigner;
      }
      set
      {
        m_webApplication.AllowDesigner = value;
      }
    }

    [JSProperty(Name = "allowedInlineDownloadedMimeTypes")]
    public ArrayInstance AllowedInlineDownloadedMimeTypes
    {
      get
      {
// ReSharper disable CoVariantArrayConversion
        var result = this.Engine.Array.Construct(m_webApplication.AllowedInlineDownloadedMimeTypes.ToArray());
// ReSharper restore CoVariantArrayConversion
        return result;
      }
      set
      {
        if (value == null)
        {
          m_webApplication.AllowedInlineDownloadedMimeTypes.Clear();
          return;
        }

        m_webApplication.AllowedInlineDownloadedMimeTypes.Clear();
        foreach (var obj in value.ElementValues)
        {
          var strObj = TypeConverter.ToString(obj);
          m_webApplication.AllowedInlineDownloadedMimeTypes.Add(strObj);
        }
      }
    }

    [JSProperty(Name = "allowHighCharacterListFolderNames")]
    public bool AllowHighCharacterListFolderNames
    {
      get
      {
        return m_webApplication.AllowHighCharacterListFolderNames;
      }
      set
      {
        m_webApplication.AllowHighCharacterListFolderNames = value;
      }
    }

    [JSProperty(Name = "allowMasterPageEditing")]
    public bool AllowMasterPageEditing
    {
      get
      {
        return m_webApplication.AllowMasterPageEditing;
      }
      set
      {
        m_webApplication.AllowMasterPageEditing = value;
      }
    }

    [JSProperty(Name = "allowOMCodeOverrideThrottleSettings")]
    public bool AllowOMCodeOverrideThrottleSettings
    {
      get
      {
        return m_webApplication.AllowOMCodeOverrideThrottleSettings;
      }
      set
      {
        m_webApplication.AllowOMCodeOverrideThrottleSettings = value;
      }
    }

    [JSProperty(Name = "allowPartToPartCommunication")]
    public bool AllowPartToPartCommunication
    {
      get
      {
        return m_webApplication.AllowPartToPartCommunication;
      }
      set
      {
        m_webApplication.AllowPartToPartCommunication = value;
      }
    }

    [JSProperty(Name = "allowRevertFromTemplate")]
    public bool AllowRevertFromTemplate
    {
      get
      {
        return m_webApplication.AllowRevertFromTemplate;
      }
      set
      {
        m_webApplication.AllowRevertFromTemplate = value;
      }
    }

    [JSProperty(Name = "allowSilverlightPrompt")]
    public bool AllowSilverlightPrompt
    {
      get
      {
        return m_webApplication.AllowSilverlightPrompt;
      }
      set
      {
        m_webApplication.AllowSilverlightPrompt = value;
      }
    }

    [JSProperty(Name = "alternateUrls")]
    public SPAlternateUrlCollectionInstance AlternateUrls
    {
      get
      {
        if (m_webApplication.AlternateUrls == null)
          return null;

        return new SPAlternateUrlCollectionInstance(this.Engine.Object.InstancePrototype, m_webApplication.AlternateUrls);
      }
    }

    [JSProperty(Name = "alwaysProcessDocuments")]
    public bool AlwaysProcessDocuments
    {
      get
      {
        return m_webApplication.AlwaysProcessDocuments;
      }
      set
      {
        m_webApplication.AlwaysProcessDocuments = value;
      }
    }

    [JSProperty(Name = "automaticallyDeleteUnusedSiteCollections")]
    public bool AutomaticallyDeleteUnusedSiteCollections
    {
      get
      {
        return m_webApplication.AutomaticallyDeleteUnusedSiteCollections;
      }
      set
      {
        m_webApplication.AutomaticallyDeleteUnusedSiteCollections = value;
      }
    }

    //TODO: BlockedFileExtensions

    [JSProperty(Name = "browserCEIPEnabled")]
    public bool BrowserCEIPEnabled
    {
      get
      {
        return m_webApplication.BrowserCEIPEnabled;
      }
      set
      {
        m_webApplication.BrowserCEIPEnabled = value;
      }
    }

    //TODO: BrowserFileHandling

    [JSProperty(Name = "canRenameOnRestore")]
    public bool CanRenameOnRestore
    {
      get
      {
        return m_webApplication.CanRenameOnRestore;
      }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        return m_webApplication.DisplayName;
      }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_webApplication.Id); }
    }

    [JSProperty(Name = "incomingEmailServerAddress")]
    public string IncomingEmailServerAddress
    {
      get
      {
        return m_webApplication.IncomingEmailServerAddress;
      }
      set
      {
        m_webApplication.IncomingEmailServerAddress = value;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_webApplication.Name;
      }
      set
      {
        m_webApplication.Name = value;
      }
    }

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get
      {
        return m_webApplication.TypeName;
      }
    }

    [JSProperty(Name = "useClaimsAuthentication")]
    public bool UseClaimsAuthentication
    {
      get
      {
        return m_webApplication.UseClaimsAuthentication;
      }
      set
      {
        m_webApplication.UseClaimsAuthentication = value;
      }
    }

    //TODO: Tons of other properties..

    #endregion

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_webApplication.Delete();
    }

    [JSFunction(Name = "getApplicationPool")]
    public SPApplicationPoolInstance GetApplicationPool()
    {
      var applicationPool = m_webApplication.ApplicationPool;
      return applicationPool == null
        ? null
        : new SPApplicationPoolInstance(this.Engine.Object.InstancePrototype, applicationPool);
    }

    [JSFunction(Name = "getContentDatabases")]
    public SPContentDatabaseCollectionInstance GetContentDatabases()
    {
      var contentDatabases = m_webApplication.ContentDatabases;
      return contentDatabases == null
        ? null
        : new SPContentDatabaseCollectionInstance(this.Engine.Object.InstancePrototype, contentDatabases);
    }

    [JSFunction(Name = "getDeletedSites")]
    public SPDeletedSiteCollectionInstance GetDeletedSites()
    {
      var collection = m_webApplication.GetDeletedSites();
      return new SPDeletedSiteCollectionInstance(this.Engine.Object.InstancePrototype, collection);
    }

    [JSFunction(Name = "getDeletedSitesByGuid")]
    public SPDeletedSiteCollectionInstance GetDeletedSitesByGuid(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var collection = m_webApplication.GetDeletedSites(guid);
      return new SPDeletedSiteCollectionInstance(this.Engine.Object.InstancePrototype, collection);
    }

    [JSFunction(Name = "getDeletedSitesByPath")]
    public SPDeletedSiteCollectionInstance GetDeletedSitesByPath(string sitePath)
    {
      var collection = m_webApplication.GetDeletedSites(sitePath);
      return new SPDeletedSiteCollectionInstance(this.Engine.Object.InstancePrototype, collection);
    }

    [JSFunction(Name = "getFeatures")]
    public ArrayInstance GetFeatures()
    {
      var result = this.Engine.Array.Construct();
      foreach (var feature in m_webApplication.Features)
      {
        ArrayInstance.Push(result, new SPFeatureInstance(this.Engine.Object.InstancePrototype, feature));
      }
      return result;
    }

    [JSFunction(Name = "getSites")]
    public SPSiteCollectionInstance GetSites()
    {
      var sites = m_webApplication.Sites;
      return sites == null
        ? null
        : new SPSiteCollectionInstance(this.Engine.Object.InstancePrototype, sites);
    }

    [JSFunction(Name = "update")]
    public void Update(object ensure)
    {
      if (ensure == null || ensure == Null.Value || ensure == Undefined.Value)
        m_webApplication.Update();
      else
        m_webApplication.Update(TypeConverter.ToBoolean(ensure));
    }

    //TODO: Tons of other functions.
  }
}
