namespace Barista.SharePoint.Migration.Library
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Barista.SharePoint.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Deployment;

  [JSDoc("Contains helper methods dealing with migrating SharePoint objects.")]
  public class SPMigrationInstance : ObjectInstance
  {
    public SPMigrationInstance(ObjectInstance prototype)
      : base(prototype)
    {

      PopulateFields();
      PopulateFunctions();
    }

    [JSFunction(Name = "exportFile")]
    [JSDoc("Exports the specified file.")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportFile(
      [JSDoc("The SharePoint file to export. Can be a SPFileInstance, guid, uri or url string")]
      object file,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPFile spFile;

      if (file is SPFileInstance)
        spFile = (file as SPFileInstance).File;
      else if (file is GuidInstance)
      {
        var guid = (file as GuidInstance).Value;
        spFile = SPBaristaContext.Current.Web.GetFile(guid);
      }
      else if (file is UriInstance)
      {
        var uri = (file as UriInstance).Uri;
        spFile = SPBaristaContext.Current.Web.GetFile(uri.ToString());
      }
      else
      {
        var fileUrl = TypeConverter.ToString(file);
        spFile = SPBaristaContext.Current.Web.GetFile(fileUrl);
      }

      if (spFile == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a file with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spFile.UniqueId,
        Type = SPDeploymentObjectType.File,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> {exportObject}, dropLocation);
    }

    [JSFunction(Name = "exportFolder")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportFolder(
      [JSDoc("The SharePoint folder to export. Can be a SPFolderInstance, guid, uri or url string")]
      object folder,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPFolder spFolder;

      if (folder is SPFolderInstance)
        spFolder = (folder as SPFolderInstance).Folder;
      else if (folder is GuidInstance)
      {
        var guid = (folder as GuidInstance).Value;
        spFolder = SPBaristaContext.Current.Web.GetFolder(guid);
      }
      else if (folder is UriInstance)
      {
        var uri = (folder as UriInstance).Uri;
        spFolder = SPBaristaContext.Current.Web.GetFolder(uri.ToString());
      }
      else
      {
        var folderUri = TypeConverter.ToString(folder);
        spFolder = SPBaristaContext.Current.Web.GetFolder(folderUri);
      }

      if (spFolder == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a file with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spFolder.UniqueId,
        Type = SPDeploymentObjectType.Folder,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> { exportObject }, dropLocation);
    }

    [JSFunction(Name = "exportListItem")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportListItem(
      [JSDoc("The SharePoint list item to export. Can be a SPListItemInstance, guid, uri or url string")]
      object listItem,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPListItem spListItem;

      if (listItem is SPListItemInstance)
        spListItem = (listItem as SPListItemInstance).ListItem;
      else if (listItem is GuidInstance)
      {
        var guid = (listItem as GuidInstance).Value;
        spListItem = SPBaristaContext.Current.List.Items[guid];
      }
      else if (listItem is UriInstance)
      {
        var uri = (listItem as UriInstance).Uri;
        spListItem = SPBaristaContext.Current.Web.GetListItem(uri.ToString());
      }
      else
      {
        var listItemUrl = TypeConverter.ToString(listItem);
        spListItem = SPBaristaContext.Current.Web.GetListItem(listItemUrl);
      }

      if (spListItem == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a list item with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spListItem.UniqueId,
        Type = SPDeploymentObjectType.ListItem,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> { exportObject }, dropLocation);
    }

    [JSFunction(Name = "exportList")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportList(
      [JSDoc("The SharePoint list to export. Can be a SPListInstance, guid, uri or url string")]
      object list,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPList spList;

      if (list is SPListInstance)
        spList = (list as SPListInstance).List;
      else if (list is GuidInstance)
      {
        var guid = (list as GuidInstance).Value;
        spList = SPBaristaContext.Current.Web.Lists[guid];
      }
      else if (list is UriInstance)
      {
        var uri = (list as UriInstance).Uri;
        spList = SPBaristaContext.Current.Web.GetList(uri.ToString());
      }
      else
      {
        var listTitle = TypeConverter.ToString(list);
        spList = SPBaristaContext.Current.Web.Lists.TryGetList(listTitle);
      }

      if (spList == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a list with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spList.ID,
        Type = SPDeploymentObjectType.List,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> { exportObject }, dropLocation);
    }

    [JSFunction(Name = "exportWeb")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportWeb(
      [JSDoc("The SharePoint web to export. Can be a SPWebInstance, guid, uri or url string")]
      object web,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPWeb spWeb;

      if (web is SPWebInstance)
        spWeb = (web as SPWebInstance).Web;
      else if (web is GuidInstance)
      {
        var guid = (web as GuidInstance).Value;
        spWeb = SPBaristaContext.Current.Site.OpenWeb(guid);
      }
      else if (web is UriInstance)
      {
        var uri = (web as UriInstance).Uri;
        spWeb = SPBaristaContext.Current.Site.OpenWeb(uri.ToString());
      }
      else
      {
        var webTitle = TypeConverter.ToString(web);
        spWeb = SPBaristaContext.Current.Site.OpenWeb(webTitle);
      }

      if (spWeb == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a web with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spWeb.ID,
        Type = SPDeploymentObjectType.Web,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> { exportObject }, dropLocation);
    }

    [JSFunction(Name = "exportSite")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance ExportSite(
      [JSDoc("The SharePoint site to export. Can be a SPSiteInstance, guid, uri or url string")]
      object site,
      [JSDoc("The target file path or null to autogenerate.")]
      object fileLocation,
      [JSDoc("The base file name or null to autogenerate.")]
      object baseFileName,
      [JSDoc("The log file path or null to use the default.")]
      object logFilePath,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      SPSite spSite;

      if (site is SPSiteInstance)
        spSite = (site as SPSiteInstance).Site;
      else if (site is GuidInstance)
      {
        var guid = (site as GuidInstance).Value;
        spSite = new SPSite(guid, SPBaristaContext.Current.Site.UserToken);
      }
      else if (site is UriInstance)
      {
        var uri = (site as UriInstance).Uri;
        spSite = new SPSite(uri.ToString(), SPBaristaContext.Current.Site.UserToken);
      }
      else
      {
        var siteUrl = TypeConverter.ToString(site);
        spSite = new SPSite(siteUrl, SPBaristaContext.Current.Site.UserToken);
      }

      if (spSite == null)
        throw new JavaScriptException(Engine, "Error", "Could not locate a site with the specified argument.");

      var exportObject = new SPExportObject
      {
        Id = spSite.ID,
        Type = SPDeploymentObjectType.Site,
        IncludeDescendants = SPIncludeDescendants.All
      };

      return ExportInternal(fileLocation, baseFileName, logFilePath, new List<SPExportObject> { exportObject }, dropLocation);
    }

    [JSFunction(Name = "export")]
    [JSDoc("ternReturnType", "[string]")]
    public ArrayInstance Export(
      [JSDoc("An object that contains export settings properties or an instance of SPExportSettings")]
      object exportSettings,
      [JSDoc("The target drop location or null to keep in target folder.")]
      object dropLocation)
    {
      var export = new SPExport();

      if (exportSettings is SPExportSettingsInstance)
        export.Settings = (exportSettings as SPExportSettingsInstance).SPExportSettings;
      else if (exportSettings is ObjectInstance)
      {
        var settings = JurassicHelper.Coerce<SPExportSettingsInstance>(Engine, exportSettings as ObjectInstance);
        export.Settings = settings.SPExportSettings;
      }
      else
      {
        throw new JavaScriptException(Engine, "Error", "Expected the first argument to be a export settings object.");
      }

      export.Run();
      if (dropLocation != Null.Value && dropLocation != Undefined.Value)
        SPExportInstance.CopyFilesToDropLocation(export, TypeConverter.ToString(dropLocation));

      var result = Engine.Array.Construct();
      foreach (var dataFile in export.Settings.DataFiles.OfType<string>())
      {
        ArrayInstance.Push(result, dataFile);
      }
      return result;
    }
    
    [JSFunction(Name = "import")]
    public void Import(
      [JSDoc("An object that contains import settings properties or an instance of SPImportSettings")] object
      importSettings,
      [JSDoc("The target drop location from where the files should be copied from or null to use files on the files system.")]
      object dropLocation)
    {
      var import = new SPImport();

      if (importSettings is SPImportSettingsInstance)
        import.Settings = (importSettings as SPImportSettingsInstance).SPImportSettings;
      else if (importSettings is ObjectInstance)
      {
        var settings = JurassicHelper.Coerce<SPImportSettingsInstance>(Engine, importSettings as ObjectInstance);
        import.Settings = settings.SPImportSettings;
      }
      else
      {
        throw new JavaScriptException(Engine, "Error", "Expected the first argument to be an import settings object.");
      }

      import.Run();
    }

    [JSFunction(Name = "importToSite")]
    [JSDoc("Provides an alternative interface for import.")]
    public void ImportToSite(
      [JSDoc("Provides the SPSite of the import target. Can be a SPSite instance a uri or a string url.")]
      object site,
      object fileLocation,
      object baseFileName,
      object logFilePath,
      object isDropFileLocation)
    {
      var importSettings = new SPImportSettings
      {
        SiteUrl = SPBaristaContext.Current.Site.Url,
        IncludeSecurity = Microsoft.SharePoint.Deployment.SPIncludeSecurity.All,
        RetainObjectIdentity = false,
        CommandLineVerbose = true,
      };

      if (site != Null.Value && site != Undefined.Value)
      {
        if (site is SPSiteInstance)
          importSettings.SiteUrl = (site as SPSiteInstance).Site.Url;
        else if (site is GuidInstance)
        {
          var guid = (site as GuidInstance).Value;
          using (var spSite = new SPSite(guid))
          {
            importSettings.SiteUrl = spSite.Url;
          }
        }
        else if (site is UriInstance)
        {
          var uri = (site as UriInstance).Uri;
          using (var spSite = new SPSite(uri.ToString()))
          {
            importSettings.SiteUrl = spSite.Url;
          }
        }
        else
        {
          var siteUrl = TypeConverter.ToString(site);
          using (var spSite = new SPSite(siteUrl))
          {
            importSettings.SiteUrl = spSite.Url;
          }
        }
      }

      if (fileLocation != Null.Value && fileLocation != Undefined.Value)
      {
        var strFileLocation = TypeConverter.ToString(fileLocation);
        importSettings.FileLocation = strFileLocation;
      }

      if (baseFileName != Null.Value && baseFileName != Undefined.Value)
      {
        var strBaseFileName = TypeConverter.ToString(baseFileName);
        importSettings.BaseFileName = strBaseFileName;
      }

      if (logFilePath != Null.Value && logFilePath != Undefined.Value)
      {
        var strLogFilePath = TypeConverter.ToString(logFilePath);
        importSettings.LogFilePath = strLogFilePath;
      }

      var import = new SPImport {
        Settings = importSettings
      };

      if (isDropFileLocation != Null.Value && isDropFileLocation != Undefined.Value)
      {
        var bIsDropFileLocation = TypeConverter.ToBoolean(isDropFileLocation);
        if (bIsDropFileLocation)
          SPImportInstance.CopyFilesFromDropLocationToTempLocation(import, importSettings.FileLocation, importSettings.BaseFileName);
      }

      import.Run();

    }

    private ArrayInstance ExportInternal(object fileLocation, object baseFileName, object logFilePath, IEnumerable<SPExportObject> exportObjects, object dropLocation)
    {
      var exportSettings = new SPExportSettings
      {
        ExportMethod = Microsoft.SharePoint.Deployment.SPExportMethodType.ExportAll,
        SiteUrl = SPBaristaContext.Current.Site.Url,
        IncludeSecurity = Microsoft.SharePoint.Deployment.SPIncludeSecurity.All,
        IncludeVersions = Microsoft.SharePoint.Deployment.SPIncludeVersions.All,
        OverwriteExistingDataFile = true
      };

      if (fileLocation != Null.Value && fileLocation != Undefined.Value)
      {
        var strFileLocation = TypeConverter.ToString(fileLocation);
        if (strFileLocation.IsNullOrWhiteSpace() == false)
        {
          exportSettings.AutoGenerateDataFileName = false;
          exportSettings.FileLocation = strFileLocation;
        }
      }

      if (baseFileName != Null.Value && baseFileName != Undefined.Value)
      {
        var strBaseFileName = TypeConverter.ToString(baseFileName);
        if (strBaseFileName.IsNullOrWhiteSpace() == false)
        {
          exportSettings.AutoGenerateDataFileName = false;
          exportSettings.BaseFileName = strBaseFileName;
        }
      }

      if (logFilePath != Null.Value && logFilePath != Undefined.Value)
      {
        var strLogFilePath = TypeConverter.ToString(logFilePath);
        if (strLogFilePath.IsNullOrWhiteSpace() == false)
          exportSettings.LogFilePath = strLogFilePath;
      }

      foreach (var obj in exportObjects)
      {
        exportSettings.ExportObjects.Add(obj);
      }

      var export = new SPExport(exportSettings);
      export.Run();

      if (dropLocation != Null.Value && dropLocation != Undefined.Value)
        SPExportInstance.CopyFilesToDropLocation(export, TypeConverter.ToString(dropLocation));

      var result = Engine.Array.Construct();
      foreach (var dataFile in exportSettings.DataFiles.OfType<string>())
      {
        ArrayInstance.Push(result, Path.Combine(exportSettings.FileLocation, dataFile));
      }
      return result;
    }
  }
}
