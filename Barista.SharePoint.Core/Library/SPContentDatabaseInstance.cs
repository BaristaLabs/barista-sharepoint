namespace Barista.SharePoint.Library
{
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPContentDatabaseConstructor : ClrFunction
  {
    public SPContentDatabaseConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPContentDatabase", new SPContentDatabaseInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPContentDatabaseInstance Construct()
    {
      return new SPContentDatabaseInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPContentDatabaseInstance : ObjectInstance
  {
    private readonly SPContentDatabase m_contentDatabase;

    public SPContentDatabaseInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPContentDatabaseInstance(ObjectInstance prototype, SPContentDatabase sPContentDatabase)
      : this(prototype)
    {
      if (sPContentDatabase == null)
        throw new ArgumentNullException("sPContentDatabase");

      m_contentDatabase = sPContentDatabase;
    }

    public SPContentDatabase SPContentDatabase
    {
      get { return m_contentDatabase; }
    }

    #region Properties
    [JSProperty(Name = "canRenameOnRestore")]
    public bool CanRenameOnRestore
    {
      get
      {
        return m_contentDatabase.CanRenameOnRestore;
      }
    }

    [JSProperty(Name = "canSelectForBackup")]
    public bool CanSelectForBackup
    {
      get
      {
        return m_contentDatabase.CanSelectForBackup;
      }
    }

    [JSProperty(Name = "canSelectForRestore")]
    public bool CanSelectForRestore
    {
      get
      {
        return m_contentDatabase.CanSelectForRestore;
      }
    }

    [JSProperty(Name = "canUpgrade")]
    public bool CanUpgrade
    {
      get
      {
        return m_contentDatabase.CanUpgrade;
      }
    }
    
    //CurrentChangeToken

    [JSProperty(Name = "currentSiteCount")]
    public int CurrentSiteCount
    {
      get
      {
        return m_contentDatabase.CurrentSiteCount;
      }
    }

    [JSProperty(Name = "databaseConnectionString")]
    public string DatabaseConnectionString
    {
      get
      {
        return m_contentDatabase.DisplayName;
      }
    }

    [JSProperty(Name = "diskSizeRequired")]
    public string DiskSizeRequired
    {
      get
      {
        return m_contentDatabase.DiskSizeRequired.ToString(CultureInfo.InvariantCulture);
      }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        return m_contentDatabase.DisplayName;
      }
    }

    [JSProperty(Name = "exists")]
    public bool Exists
    {
      get
      {
        return m_contentDatabase.Exists;
      }
    }

    [JSProperty(Name = "existsInFarm")]
    public bool ExistsInFarm
    {
      get
      {
        return m_contentDatabase.ExistsInFarm;
      }
    }

    [JSProperty(Name = "failoverServer")]
    public SPServerInstance FailoverServer
    {
      get
      {
        if (m_contentDatabase.FailoverServer == null)
          return null;
        return new SPServerInstance(this.Engine.Object.InstancePrototype, m_contentDatabase.FailoverServer);
      }
    }

    [JSProperty(Name = "id")]
    public object Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_contentDatabase.Id);
      }
      set
      {
        m_contentDatabase.Id = GuidInstance.ConvertFromJsObjectToGuid(value);
      }
    }

    [JSProperty(Name = "includeInVssBackup")]
    public bool IncludeInVssBackup
    {
      get
      {
        return m_contentDatabase.IncludeInVssBackup;
      }
    }

    [JSProperty(Name = "isAttachedToFarm")]
    public bool IsAttachedToFarm
    {
      get
      {
        return m_contentDatabase.IsAttachedToFarm;
      }
    }

    [JSProperty(Name = "isBackwardsCompatible")]
    public object IsBackwardsCompatible
    {
      get
      {
        if (m_contentDatabase.IsBackwardsCompatible == TriState.True)
          return true;
        
        if (m_contentDatabase.IsBackwardsCompatible == TriState.False)
          return false;

        return null;
      }
    }

    [JSProperty(Name = "isReadOnly")]
    public bool IsReadOnly
    {
      get
      {
        return m_contentDatabase.IsReadOnly;
      }
    }

    [JSProperty(Name = "legacyDatabaseConnectionString")]
    public string LegacyDatabaseConnectionString
    {
      get
      {
        return m_contentDatabase.LegacyDatabaseConnectionString;
      }
    }

    [JSProperty(Name = "maximumSiteCount")]
    public int MaximumSiteCount
    {
      get
      {
        return m_contentDatabase.MaximumSiteCount;
      }
      set
      {
        m_contentDatabase.MaximumSiteCount = value;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_contentDatabase.Name;
      }
      set
      {
        m_contentDatabase.Name = value;
      }
    }

    [JSProperty(Name = "needsUpgrade")]
    public bool NeedsUpgrade
    {
      get
      {
        return m_contentDatabase.NeedsUpgrade;
      }
      set
      {
        m_contentDatabase.NeedsUpgrade = value;
      }
    }

    [JSProperty(Name = "needsUpgradeIncludeChildren")]
    public bool NeedsUpgradeIncludeChildren
    {
      get
      {
        return m_contentDatabase.NeedsUpgradeIncludeChildren;
      }
    }

    [JSProperty(Name = "normalizedDataSource")]
    public string NormalizedDataSource
    {
      get
      {
        return m_contentDatabase.NormalizedDataSource;
      }
    }

    [JSProperty(Name = "password")]
    public string Password
    {
      get
      {
        return m_contentDatabase.Password;
      }
      set
      {
        m_contentDatabase.Password = value;
      }
    }

    [JSProperty(Name = "schemaVersionXml")]
    public string SchemaVersionXml
    {
      get
      {
        return m_contentDatabase.SchemaVersionXml;
      }
    }

    [JSProperty(Name = "server")]
    public string Server
    {
      get
      {
        return m_contentDatabase.Server;
      }
    }

    [JSProperty(Name = "snapshots")]
    public object Snapshots
    {
      get
      {
        if (m_contentDatabase.Snapshots == null)
          return null;

        return new SPDatabaseSnapshotCollectionInstance(this.Engine.Object.InstancePrototype,
          m_contentDatabase.Snapshots);
      }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get
      {
        return m_contentDatabase.Status.ToString();
      }
    }

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get
      {
        return m_contentDatabase.TypeName;
      }
    }

    [JSProperty(Name = "userName")]
    public string UserName
    {
      get
      {
        return m_contentDatabase.Username;
      }
      set
      {
        m_contentDatabase.Username = value;
      }
    }

    [JSProperty(Name = "warningSiteCount")]
    public int WarningSiteCount
    {
      get
      {
        return m_contentDatabase.WarningSiteCount;
      }
      set
      {
        m_contentDatabase.WarningSiteCount = value;
      }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_contentDatabase.Delete();
    }

    [JSFunction(Name = "getFarm")]
    public SPFarmInstance GetFarm()
    {
      var farm = m_contentDatabase.Farm;
      return farm == null
        ? null
        : new SPFarmInstance(this.Engine.Object.InstancePrototype, m_contentDatabase.Farm);
    }

    [JSFunction(Name = "getSites")]
    public SPSiteCollectionInstance GetSites()
    {
      var sites = m_contentDatabase.Sites;
      return sites == null
        ? null
        : new SPSiteCollectionInstance(this.Engine.Object.InstancePrototype, sites);
    }

    [JSFunction(Name = "getWebApplication")]
    public SPWebApplicationInstance GetWebApplication()
    {
      var wa = m_contentDatabase.WebApplication;
      return wa == null
        ? null
        : new SPWebApplicationInstance(this.Engine.Object.InstancePrototype, m_contentDatabase.WebApplication);
    }

    [JSFunction(Name = "invalidate")]
    public void Invalidate()
    {
      m_contentDatabase.Invalidate();
    }

    [JSFunction(Name = "move")]
    public object Move(SPContentDatabaseInstance destinationDatabase, ArrayInstance sitesToMove)
    {
      var sitesToMoveList = new List<SPSite>();
      foreach (var site in sitesToMove.ElementValues.OfType<SPSiteInstance>())
      {
        if (site != null)
          sitesToMoveList.Add(site.Site);
      }

      Dictionary<SPSite, string> failedSites;
      m_contentDatabase.Move(destinationDatabase.SPContentDatabase, sitesToMoveList, out failedSites);

      var result = this.Engine.Array.Construct();
      foreach (var site in failedSites)
      {
        var resultObj = this.Engine.Object.Construct();
        resultObj.SetPropertyValue("site", new SPSiteInstance(this.Engine.Object.InstancePrototype, site.Key), false);
        resultObj.SetPropertyValue("status", site.Value, false);
        ArrayInstance.Push(result, resultObj);
      }
      return result;
    }

    [JSFunction(Name = "repair")]
    public string Repair(bool deleteCorruption)
    {
      return m_contentDatabase.Repair(deleteCorruption);
    }

    [JSFunction(Name = "update")]
    public void Update(object ensure)
    {
      if (ensure != Null.Value && ensure != Undefined.Value && ensure is bool)
        m_contentDatabase.Update((bool)ensure);
      else
        m_contentDatabase.Update();
    }

    [JSFunction(Name = "upgrade")]
    public void Upgrade(object recursively)
    {
      if (recursively != Null.Value && recursively != Undefined.Value && recursively is bool)
        m_contentDatabase.Upgrade((bool)recursively);
      else
        m_contentDatabase.Upgrade();
    }

    #endregion
  }
}
