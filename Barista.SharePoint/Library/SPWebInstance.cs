namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Newtonsoft.Json;
  using Microsoft.SharePoint.Utilities;
  using Microsoft.SharePoint.Administration;
  using Microsoft.Office.Server.Utilities;
  using System.IO;
  using Barista.Library;

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
  public class SPWebInstance : ObjectInstance, IDisposable
  {
    private SPWeb m_web;

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
          var serializedKey = JsonConvert.SerializeObject(key);
          var serializedValue = JsonConvert.SerializeObject(m_web.AllProperties[key]);

          result.SetPropertyValue(serializedKey, serializedValue, false);
        }

        return result;
      }
    }
    
    //TODO: Group properties, fields

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
    public ArrayInstance Features
    {
      get
      {
        var result = this.Engine.Array.Construct();
        foreach (var feature in m_web.Features.OfType<SPFeature>())
        {
          if (feature == null)
            continue;

          ArrayInstance.Push(result, new SPFeatureInstance(this.Engine.Object.InstancePrototype, feature));
        }
        return result;
      }
    }

    //TODO: Effective base permissions, fields,

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_web.ID.ToString(); }
    }

    [JSProperty(Name = "language")]
    public string Language
    {
      get { return m_web.Language.ToString(); }
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
      get { return new SPFolderInstance(this.Engine.Object.InstancePrototype, m_web.RootFolder); }
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
      SPFile result = null;
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
    public SPFeatureInstance ActivateFeature(object feature, [DefaultParameterValue(false)] bool force)
    {
      Guid featureId = Guid.Empty;
      if (feature is string)
      {
        featureId = new Guid(feature as string);
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

      var activatedFeature = m_web.Features.Add(featureId, force);
      return new SPFeatureInstance(this.Engine.Object.InstancePrototype, activatedFeature);
    }

    [JSFunction(Name = "createList")]
    public SPListInstance CreateList(object listCreationInfo)
    {
      Guid createdListId = Guid.Empty;

      var listCreationInstance = listCreationInfo as ObjectInstance;
      var creationInfo = JurassicHelper.Coerce<SPListCreationInformation>(this.Engine, listCreationInfo);
      SPListTemplate.QuickLaunchOptions quickLaunchOptions = (SPListTemplate.QuickLaunchOptions)Enum.Parse(typeof(SPListTemplate.QuickLaunchOptions), creationInfo.QuickLaunchOption);

      //If dataSourceProperties property has a value, create the list instance as a BCS list.
      if (listCreationInstance != null && listCreationInstance.HasProperty("dataSourceProperties"))
      {
        var dataSourceInstance = listCreationInstance.GetPropertyValue("dataSourceProperties") as ObjectInstance;

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
          listTemplate = m_web.ListTemplates.OfType<SPListTemplate>().Where(dt => (int)dt.Type == (int)listTemplateValue).FirstOrDefault();
        }
        else if (listTemplateValue is string)
        {
          listTemplate = m_web.ListTemplates.OfType<SPListTemplate>().Where(dt => dt.Type.ToString() == (string)listTemplateValue).FirstOrDefault();
        }
        else if (listTemplateValue is ObjectInstance)
        {
          listTemplate = JurassicHelper.Coerce<SPListTemplateInstance>(this.Engine, listTemplateValue).ListTemplate;
        }


        if (listCreationInstance.HasProperty("docTemplate"))
        {
          var docTemplate = JurassicHelper.Coerce<SPDocTemplateInstance>(this.Engine, listCreationInstance.GetPropertyValue("docTemplate"));
          createdListId = m_web.Lists.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, listTemplate.FeatureId.ToString(), listTemplate.Type_Client, docTemplate.DocTemplate.Type.ToString(), quickLaunchOptions);
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
      return new SPListInstance(this.Engine.Object.InstancePrototype, createdList);
    }

    [JSFunction(Name = "deactivateFeature")]
    public void DeactivateFeature(object feature, [DefaultParameterValue(false)] bool force)
    {
      Guid featureId = Guid.Empty;
      if (feature is string)
      {
        featureId = new Guid(feature as string);
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

      return new SPFolderInstance(this.Engine.Object.InstancePrototype, folder);
    }

    [JSFunction(Name = "getFolders")]
    public ArrayInstance GetFolders()
    {
      var result = this.Engine.Array.Construct();
      foreach (var folder in m_web.Folders.OfType<SPFolder>())
      {
        ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, folder));
      }

      return result;
    }

    [JSFunction(Name = "getListByServerRelativeUrl")]
    public SPListInstance GetListFromServerRelativeUrl(string serverRelativeUrl)
    {
      SPList list = null;
      try
      {
        string siteRelativeUrl = String.Empty;
        if (string.IsNullOrEmpty(serverRelativeUrl) || SPUrlUtility.IsUrlRelative(serverRelativeUrl))
        {
          serverRelativeUrl = SPUtility.GetFullUrl(BaristaContext.Current.Site, serverRelativeUrl);
        }

        siteRelativeUrl = serverRelativeUrl.Replace(m_web.Url, "");
        siteRelativeUrl = siteRelativeUrl.Replace(Path.GetFileName(siteRelativeUrl), "");

        list = m_web.GetList(siteRelativeUrl);
      }
      catch (FileNotFoundException)
      {
        /* Do Nothing... */
      }

      if (list == null)
        return null;

      return new SPListInstance(this.Engine.Object.InstancePrototype, list);
    }

    [JSFunction(Name = "getLists")]
    public ArrayInstance GetLists()
    {
      List<SPList> lists = new List<SPList>();
      var instance = this.Engine.Array.Construct();

      ContentIterator listsIterator = new ContentIterator();
      listsIterator.ProcessLists(m_web.Lists, (currentList) =>
      {
        lists.Add(currentList);
      }, null);

      foreach (var list in lists)
      {
        ArrayInstance.Push(instance, new SPListInstance(this.Engine.Object.Prototype, list));
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
      SPList list = m_web.Lists.TryGetList(listTitle);
      
      return new SPListInstance(this.Engine.Object.Prototype, list);
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

    [JSFunction(Name = "getSiteGroups")]
    public ArrayInstance GetSiteGroups()
    {
      var result = this.Engine.Array.Construct();
      foreach (var group in m_web.SiteGroups.OfType<SPGroup>())
      {
        ArrayInstance.Push(result, new SPGroupInstance(this.Engine.Object.InstancePrototype, group));
      }
      return result;
    }

    [JSFunction(Name = "getSiteUserInfoList")]
    public SPListInstance GetSiteUserInfoList()
    {
      return new SPListInstance(this.Engine.Object.InstancePrototype, m_web.SiteUserInfoList);
    }

    [JSFunction(Name = "getWebs")]
    public ArrayInstance GetWebs()
    {
      var result = this.Engine.Array.Construct();
      foreach (var web in m_web.Webs.OfType<SPWeb>())
      {
        ArrayInstance.Push(result, new SPWebInstance(this.Engine.Object.InstancePrototype, web));
      }

      return result;
    }

    [JSFunction(Name = "mapToIcon")]
    public string MapToIcon(string fileName, string progId, string iconSize)
    {
      if (String.IsNullOrEmpty(iconSize))
        return SPUtility.MapToIcon(m_web, fileName, progId);
      else
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
