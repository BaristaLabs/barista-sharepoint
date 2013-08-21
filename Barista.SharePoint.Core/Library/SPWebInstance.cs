namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Newtonsoft.Json;
  using Microsoft.SharePoint.Utilities;
  using Microsoft.SharePoint.Administration;
  using Microsoft.Office.Server.Utilities;
  using System.IO;
  using Barista.Library;

  [Serializable]
  public class SPWebConstructor : ClrFunction
  {
    public SPWebConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWeb", new SPWebInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWebInstance Construct(string webUrl)
    {
      SPWeb web;

      if (SPHelper.TryGetSPWeb(webUrl, out web) == false)
        throw new JavaScriptException(this.Engine, "Error", "A web is not available at the specified url.");

      return new SPWebInstance(this.InstancePrototype, web);
    }

    public SPWebInstance Construct(SPWeb web)
    {
      if (web == null)
        throw new ArgumentNullException("web");

      return new SPWebInstance(this.InstancePrototype, web);
    }
  }

  [JSDoc("Represents a SharePoint website.")]
  [Serializable]
  public class SPWebInstance : ObjectInstance, IDisposable
  {
    private readonly SPWeb m_web;

    public SPWebInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWebInstance(ObjectInstance prototype, SPWeb web)
      : this(prototype)
    {
      this.m_web = web;
    }

    public SPWeb Web
    {
      get { return m_web; }
    }

    #region Properties
    [JSDoc("Gets a Boolean value that specifies whether the current user is allowed to use the designer for this website. The default value is false.")]
    [JSProperty(Name = "allowDesignerForCurrentUser")]
    public bool AllowDesignerForCurrentUser
    {
      get { return m_web.AllowDesignerForCurrentUser; }
    }

    [JSProperty(Name = "allowMasterPageEditingForCurrentUser")]
    public bool AllowMasterPageEditingForCurrentUser
    {
      get { return m_web.AllowMasterPageEditingForCurrentUser; }
    }

    [JSProperty(Name = "allowRevertFromTemplateForCurrentUser")]
    public bool AllowRevertFromTemplateForCurrentUser
    {
      get { return m_web.AllowRevertFromTemplateForCurrentUser; }
    }

    [JSProperty(Name = "allowRssFeeds")]
    public bool AllowRssFeeds
    {
      get { return m_web.AllowRssFeeds; }
    }

    [JSProperty(Name = "allowUnsafeUpdates")]
    public bool AllowUnsafeUpdates
    {
      get { return m_web.AllowUnsafeUpdates; }
      set { m_web.AllowUnsafeUpdates = value; }
    }

    [JSProperty(Name = "allProperties")]
    public ObjectInstance AllProperties
    {
      get
      {
        var result = this.Engine.Object.Construct();

        foreach (var key in m_web.AllProperties.Keys)
        {
          string serializedKey;
          if (key is string)
            serializedKey = key as string;
          else
            serializedKey = JsonConvert.SerializeObject(key);

          var serializedValue = JsonConvert.SerializeObject(m_web.AllProperties[key]);

          result.SetPropertyValue(serializedKey, JSONObject.Parse(this.Engine, serializedValue, null), false);
        }

        return result;
      }
    }

    [JSProperty(Name = "allUsers")]
    public SPUserCollectionInstance AllUsers
    {
      get
      {
        return m_web.AllUsers == null
          ? null
          : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AllUsers);
      }
    }

    [JSProperty(Name = "availableContentTypes")]
    public SPContentTypeCollectionInstance AvailableContentTypes
    {
      get
      {
        return m_web.AvailableContentTypes == null
          ? null
          : new SPContentTypeCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AvailableContentTypes);
      }
    }

    [JSProperty(Name = "availableFields")]
    public SPFieldCollectionInstance AvailableFields
    {
      get
      {
        return m_web.AvailableFields == null
          ? null
          : new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_web.AvailableFields);
      }
    }
    
    //TODO: Group properties

    [JSProperty(Name = "fields")]
    public SPFieldCollectionInstance Fields
    {
      get
      {
        return m_web.Fields == null
          ? null
          : new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Fields);
      }
    }

    [JSProperty(Name = "contentTypes")]
    public SPContentTypeCollectionInstance ContentTypes
    {
      get
      {
        return m_web.ContentTypes == null
          ? null
          : new SPContentTypeCollectionInstance(this.Engine.Object.InstancePrototype, m_web.ContentTypes);
      }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_web.Created); }
    }

    [JSProperty(Name = "currentUser")]
    public SPUserInstance CurrentUser
    {
      get { return new SPUserInstance(this.Engine.Object.InstancePrototype, m_web.CurrentUser); }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_web.Description; }
      set { m_web.Description = value; }
    }

    [JSProperty(Name = "features")]
    public SPFeatureCollectionInstance Features
    {
      get { return new SPFeatureCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Features); }
    }

    [JSProperty(Name = "groups")]
    public SPGroupCollectionInstance Groups
    {
      get
      {
        var result = m_web.Groups;
        return result == null
          ? null
          : new SPGroupCollectionInstance(this.Engine.Object.InstancePrototype, result);
      }
    }

    //TODO: Effective base permissions

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_web.ID.ToString(); }
    }

    [JSProperty(Name = "language")]
    public string Language
    {
      get { return m_web.Language.ToString(CultureInfo.InvariantCulture); }
    }

    [JSProperty(Name = "lastItemModifiedDate")]
    public DateInstance LastItemModifiedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_web.LastItemModifiedDate); }
    }

    //TODO: Navigation

    [JSProperty(Name = "quickLaunchEnabled")]
    public bool QuickLaunchEnabled
    {
      get { return m_web.QuickLaunchEnabled; }
      set { m_web.QuickLaunchEnabled = value; }
    }

    [JSProperty(Name = "recycleBinEnabled")]
    public bool RecycleBinEnabled
    {
      get { return m_web.RecycleBinEnabled; }
    }

    [JSProperty(Name = "roleDefinitions")]
    public ArrayInstance RoleDefinitions
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var roleDefinition in m_web.RoleDefinitions.OfType<SPRoleDefinition>())
        {
          ArrayInstance.Push(result, new SPRoleDefinitionInstance(this.Engine.Object.InstancePrototype, roleDefinition));
        }
        return result;
      }
    }

    [JSProperty(Name = "rootFolder")]
    public SPFolderInstance RootFolder
    {
      get { return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_web.RootFolder); }
    }

    [JSProperty(Name = "serverRelativeUrl")]
    public string ServerRelativeUrl
    {
      get { return m_web.ServerRelativeUrl; }
    }

    [JSProperty(Name = "showUrlStructureForCurrentUser")]
    public bool ShowUrlStructureForCurrentUser
    {
      get { return m_web.ShowUrlStructureForCurrentUser; }
    }

    [JSProperty(Name = "siteGroups")]
    public SPGroupCollectionInstance SiteGroups
    {
      get
      {
        var result = m_web.SiteGroups;
        return result == null
          ? null
          : new SPGroupCollectionInstance(this.Engine.Object.InstancePrototype, result);
      }
    }

    [JSProperty(Name = "siteUsers")]
    public SPUserCollectionInstance SiteUsers
    {
      get
      {
        return m_web.SiteUsers == null
          ? null
          : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.SiteUsers);
      }
    }

    [JSProperty(Name = "syndicationEnabled")]
    public bool SyndicationEnabled
    {
      get { return m_web.SyndicationEnabled; }
      set { m_web.SyndicationEnabled = value; }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_web.Title; }
    }

    [JSProperty(Name = "treeViewEnabled")]
    public bool TreeViewEnabled
    {
      get { return m_web.TreeViewEnabled; }
      set { m_web.TreeViewEnabled = value; }
    }

    [JSProperty(Name = "uiVersion")]
    public int UIVersion
    {
      get { return m_web.UIVersion; }
      set { m_web.UIVersion = value; }
    }

    [JSProperty(Name = "uiVersionConfigurationEnabled")]
    public bool UIVersionConfigurationEnabled
    {
      get { return m_web.UIVersionConfigurationEnabled; }
      set { m_web.UIVersionConfigurationEnabled = value; }
    }

    //TODO: User Custom Actions

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return m_web.Url; }
    }

    [JSProperty(Name = "users")]
    public SPUserCollectionInstance Users
    {
      get
      {
        return m_web.Users == null
          ? null
          : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Users);
      }
    }

    [JSProperty(Name = "webTemplate")]
    public string WebTemplate
    {
      get { return m_web.WebTemplate; }
    }

    [JSProperty(Name = "webTemplateId")]
    public int WebTemplateId
    {
      get { return m_web.WebTemplateId; }
    }

    //TODO: Workflow associations/templates.
    #endregion

    #region Functions
    [JSFunction(Name = "addFileByUrl")]
    public SPFileInstance AddFile(string url, object data, [DefaultParameterValue(true)] bool overwrite)
    {
      SPFile result;
      if (data is Base64EncodedByteArrayInstance)
      {
        var byteArrayInstance = data as Base64EncodedByteArrayInstance;
        result = m_web.Files.Add(url, byteArrayInstance.Data, overwrite);
      }
      else if (data is string)
      {
        result = m_web.Files.Add(url, Encoding.UTF8.GetBytes(data as string), overwrite);
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "Unable to create SPFile: Unsupported data type: " + data.GetType());

      return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "activateFeature")]
    public SPFeatureInstance ActivateFeature(object feature, object force)
    {
      var featureId = Guid.Empty;
      if (feature is string)
      {
        featureId = new Guid(feature as string);
      }
      else if (feature is GuidInstance)
      {
        featureId = (feature as GuidInstance).Value;
      }
      else if (feature is SPFeatureInstance)
      {
        featureId = (feature as SPFeatureInstance).Feature.DefinitionId;
      }
      else if (feature is SPFeatureDefinitionInstance)
      {
        featureId = (feature as SPFeatureDefinitionInstance).FeatureDefinition.Id;
      }

      if (featureId == Guid.Empty)
        return null;

      var forceValue = JurassicHelper.GetTypedArgumentValue(this.Engine, force, false);

      var activatedFeature = m_web.Features.Add(featureId, forceValue);
      return new SPFeatureInstance(this.Engine.Object.InstancePrototype, activatedFeature);
    }

    [JSFunction(Name = "createList")]
    public SPListInstance CreateList(object listCreationInfo)
    {
      Guid createdListId;

      var listCreationInstance = listCreationInfo as ObjectInstance;
      var creationInfo = JurassicHelper.Coerce<SPListCreationInformation>(this.Engine, listCreationInfo);
      SPListTemplate.QuickLaunchOptions quickLaunchOptions = (SPListTemplate.QuickLaunchOptions)Enum.Parse(typeof(SPListTemplate.QuickLaunchOptions), creationInfo.QuickLaunchOption);

      //If dataSourceProperties property has a value, create the list instance as a BCS list.
      if (listCreationInstance != null && listCreationInstance.HasProperty("dataSourceProperties"))
      {
        var dataSourceInstance = listCreationInstance.GetPropertyValue("dataSourceProperties") as ObjectInstance;

        if (dataSourceInstance == null)
          return null;

        var dataSource = new SPListDataSource();
        foreach(var property in dataSourceInstance.Properties)
        {
          dataSource.SetProperty(property.Name, property.Value.ToString());
        }
        
        createdListId = m_web.Lists.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, dataSource);
      }
      //If listTemplate property has a value, create the list instance using the strongly-typed SPListTemplate, optionally using the docTemplate value.
      else if (listCreationInstance != null && listCreationInstance.HasProperty("listTemplate"))
      {
        var listTemplateValue = listCreationInstance.GetPropertyValue("listTemplate");
        
        SPListTemplate listTemplate = null;
        if (listTemplateValue is int)
        {
          listTemplate = m_web.ListTemplates.OfType<SPListTemplate>().FirstOrDefault(dt => (int)dt.Type == (int)listTemplateValue);
        }
        else
        {
          var s = listTemplateValue as string;

          if (s != null)
          {
            listTemplate = m_web.ListTemplates.OfType<SPListTemplate>().FirstOrDefault(dt => dt.Type.ToString() == s);
          }
          else if (listTemplateValue is ObjectInstance)
          {
            listTemplate = JurassicHelper.Coerce<SPListTemplateInstance>(this.Engine, listTemplateValue).ListTemplate;
          }
        }

        if (listTemplate == null)
          return null;

        if (listCreationInstance.HasProperty("docTemplate"))
        {
          var docTemplate = JurassicHelper.Coerce<SPDocTemplateInstance>(this.Engine, listCreationInstance.GetPropertyValue("docTemplate"));
          createdListId = m_web.Lists.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, listTemplate.FeatureId.ToString(), listTemplate.Type_Client, docTemplate.DocTemplate.Type.ToString(CultureInfo.InvariantCulture), quickLaunchOptions);
        }
        else
        {
          createdListId = m_web.Lists.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, listTemplate.FeatureId.ToString(), listTemplate.Type_Client, String.Empty, quickLaunchOptions);
        }
      }
      //Otherwise attempt to create the list using all properties set on the creation info object.
      else
      {
        SPFeatureDefinition listInstanceFeatureDefinition = null;
        if (listCreationInstance != null && listCreationInstance.HasProperty("listInstanceFeatureDefinition"))
        {
          var featureDefinitionInstance = JurassicHelper.Coerce<SPFeatureDefinitionInstance>(this.Engine, listCreationInstance.GetPropertyValue("listInstanceFeatureDefinition"));
          listInstanceFeatureDefinition = featureDefinitionInstance.FeatureDefinition;
        }

        createdListId = m_web.Lists.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, creationInfo.TemplateFeatureId, creationInfo.TemplateType, creationInfo.DocumentTemplateType, creationInfo.CustomSchemaXml, listInstanceFeatureDefinition, quickLaunchOptions);
      }
      
      var createdList = m_web.Lists[createdListId];
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, createdList);
    }

    [JSFunction(Name = "deactivateFeature")]
    public void DeactivateFeature(object feature)
    {
      var featureId = Guid.Empty;
      if (feature is string)
      {
        featureId = new Guid(feature as string);
      }
      else if (feature is GuidInstance)
      {
        featureId = (feature as GuidInstance).Value;
      }
      else if (feature is SPFeatureInstance)
      {
        featureId = (feature as SPFeatureInstance).Feature.DefinitionId;
      }
      else if (feature is SPFeatureDefinitionInstance)
      {
        featureId = (feature as SPFeatureDefinitionInstance).FeatureDefinition.Id;
      }

      if (featureId == Guid.Empty)
        return;

      m_web.Features.Remove(featureId);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_web.Delete();
    }

    [JSFunction(Name = "deleteFileIfExists")]
    public bool DeleteFileIfExists(string serverRelativeUrl)
    {
      var file = m_web.GetFile(serverRelativeUrl);
      if (file != null && file.Exists)
      {
        m_web.AllowUnsafeUpdates = true;
        try
        {
          file.Delete();
        }
        finally
        {
          m_web.AllowUnsafeUpdates = false;
        }
      }
      return false;
    }

    [JSFunction(Name = "deleteFolderIfExists")]
    public bool DeleteFolderIfExists(string serverRelativeUrl)
    {
      var folder = m_web.GetFolder(serverRelativeUrl);
      if (folder != null && folder.Exists)
      {
        m_web.AllowUnsafeUpdates = true;
        try
        {
          folder.Delete();
        }
        finally
        {
          m_web.AllowUnsafeUpdates = false;
        }
      }
      return false;
    }

    [JSFunction(Name = "doesUserHavePermissions")]
    public bool DoesUserHavePermissions(string loginName, string permissions)
    {
      SPBasePermissions basePermissions;
      if (permissions.TryParseEnum(true, out basePermissions))
      {
        this.m_web.DoesUserHavePermissions(loginName, basePermissions);
      }
      return false;
    }

    [JSFunction(Name = "ensureUser")]
    public SPUserInstance EnsureUser(string logonName)
    {
      var user = m_web.EnsureUser(logonName);
      return new SPUserInstance(this.Engine.Object.InstancePrototype, user);
    }

    [JSFunction(Name = "getAvailableContentTypes")]
    public ArrayInstance GetAvailableContentTypes()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentType in m_web.AvailableContentTypes.OfType<SPContentType>())
      {
        ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
      }
      return result;
    }

    [JSFunction(Name = "getContentTypes")]
    public ArrayInstance GetContentTypes()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentType in m_web.ContentTypes.OfType<SPContentType>())
      {
        ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
      }
      return result;
    }

    [JSFunction(Name = "getDocTemplates")]
    public ArrayInstance GetDocTemplates()
    {
      var result = this.Engine.Array.Construct();
      foreach (var docTemplate in m_web.DocTemplates.OfType<SPDocTemplate>())
      {
        ArrayInstance.Push(result, new SPDocTemplateInstance(this.Engine.Object.InstancePrototype, docTemplate));
      }
      
      return result;
    }

    [JSFunction(Name = "getFileByServerRelativeUrl")]
    public SPFileInstance GetFileFromServerRelativeUrl(string serverRelativeUrl)
    {
      var file = m_web.GetFile(serverRelativeUrl);
      if (file == null)
        return null;

      return new SPFileInstance(this.Engine.Object.InstancePrototype, file);
    }

    [JSFunction(Name = "getFiles")]
    public ArrayInstance GetFiles()
    {
      var result = this.Engine.Array.Construct();

      var files = m_web.Files;
      foreach (var file in files.OfType<SPFile>())
      {
        ArrayInstance.Push(result, new SPFileInstance(this.Engine.Object.InstancePrototype, file));
      }

      return result;
    }

    [JSFunction(Name = "getFolderByServerRelativeUrl")]
    public SPFolderInstance GetFolderFromServerRelativeUrl(string serverRelativeUrl)
    {
      var folder = m_web.GetFolder(serverRelativeUrl);

      if (folder == null || folder.Exists == false)
        return null;

      return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, folder);
    }

    [JSFunction(Name = "getFolders")]
    public ArrayInstance GetFolders()
    {
      var result = this.Engine.Array.Construct();
      foreach (var folder in m_web.Folders.OfType<SPFolder>())
      {
        ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, folder));
      }

      return result;
    }

    [JSFunction(Name = "getListByServerRelativeUrl")]
    public object GetListFromServerRelativeUrl(string serverRelativeUrl)
    {
      SPList list = null;
      try
      {
        if (string.IsNullOrEmpty(serverRelativeUrl) || SPUrlUtility.IsUrlRelative(serverRelativeUrl))
        {
          serverRelativeUrl = SPUtility.GetFullUrl(SPBaristaContext.Current.Site, serverRelativeUrl);
        }

        list = m_web.GetList(serverRelativeUrl);
      }
      catch (FileNotFoundException)
      {
        /* Do Nothing... */
      }

      if (list == null)
        return Null.Value;

      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, list);
    }

    [JSFunction(Name = "getLists")]
    public ArrayInstance GetLists()
    {
      List<SPList> lists = new List<SPList>();
      var instance = this.Engine.Array.Construct();

      ContentIterator listsIterator = new ContentIterator();
      listsIterator.ProcessLists(m_web.Lists, lists.Add, null);

      foreach (var list in lists)
      {
        ArrayInstance.Push(instance, new SPListInstance(this.Engine.Object.InstancePrototype, null, null, list));
      }

      return instance;
    }

    [JSFunction(Name = "getListTemplates")]
    public ArrayInstance GetListTemplates()
    {
      var result = this.Engine.Array.Construct();
      foreach (var listTemplate in m_web.ListTemplates.OfType<SPListTemplate>())
      {
        ArrayInstance.Push(result, new SPListTemplateInstance(this.Engine.Object.InstancePrototype, listTemplate));
      }

      return result;
    }

    [JSFunction(Name = "getListByTitle")]
    public SPListInstance GetListByTitle(string listTitle)
    {
      var list = m_web.Lists.TryGetList(listTitle);
      
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, list);
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, this.m_web);
    }

    [JSFunction(Name = "getSiteData")]
    public object GetSiteData(SPSiteDataQueryInstance query)
    {
      var result = m_web.GetSiteData(query.SiteDataQuery);
      var jsonResult = JsonConvert.SerializeObject(result);
      return JSONObject.Parse(this.Engine, jsonResult, null);
    }

    [JSFunction(Name = "getSiteUserInfoList")]
    public SPListInstance GetSiteUserInfoList()
    {
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_web.SiteUserInfoList);
    }

    [JSFunction(Name = "getUser")]
    public object GetUser(string loginName)
    {
       var user = m_web.Users[loginName];
       if (user != null)
         return new SPUserInstance(this.Engine.Object.InstancePrototype, user);
       return Null.Value;
    }

    [JSFunction(Name = "getWebs")]
    public ArrayInstance GetWebs()
    {
      var result = this.Engine.Array.Construct();
      foreach (var web in m_web.Webs)
      {
        ArrayInstance.Push(result, new SPWebInstance(this.Engine.Object.InstancePrototype, (SPWeb)web));
      }

      return result;
    }

    [JSFunction(Name = "mapToIcon")]
    public string MapToIcon(string fileName, string progId, string iconSize)
    {
      if (String.IsNullOrEmpty(iconSize))
        return SPUtility.MapToIcon(m_web, fileName, progId);

      return SPUtility.MapToIcon(m_web, fileName, progId, (IconSize)Enum.Parse(typeof(IconSize), iconSize));
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_web.Update();
    }
    #endregion

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      if (m_web != null)
        m_web.Dispose();
    }
  }
}
