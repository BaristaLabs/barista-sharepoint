namespace Barista.SharePoint.Library
{
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.Office.Server.Utilities;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  [Serializable]
  public class SPWebConstructor : ClrFunction
  {
    public SPWebConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWeb", new SPWebInstance(engine, null))
    {
    }

    [JSConstructorFunction]
    public SPWebInstance Construct(string webUrl)
    {
      SPWeb web;

      if (SPHelper.TryGetSPWeb(webUrl, out web) == false)
        throw new JavaScriptException(this.Engine, "Error", "A web is not available at the specified url.");

      return new SPWebInstance(this.Engine, web);
    }

    public SPWebInstance Construct(SPWeb web)
    {
      if (web == null)
        throw new ArgumentNullException("web");

      return new SPWebInstance(this.Engine, web);
    }
  }

  [JSDoc("Represents a SharePoint website.")]
  [Serializable]
  public class SPWebInstance : SPSecurableObjectInstance, IDisposable
  {
    private readonly SPWeb m_web;

    public SPWebInstance(ScriptEngine engine, SPWeb web)
      : base(new SPSecurableObjectInstance(engine))
    {
      this.m_web = web;
      SecurableObject = this.m_web;

      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
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

    [JSProperty(Name = "allWebTemplatesAllowed")]
    public bool AllWebTemplatesAllowed
    {
      get
      {
        return m_web.AllWebTemplatesAllowed;
      }
    }

    [JSProperty(Name = "alternateCssUrl")]
    public string AlternateCssUrl
    {
      get
      {
        return m_web.AlternateCssUrl;
      }
      set
      {
        m_web.AlternateCssUrl = value;
      }
    }

    [JSProperty(Name = "alternateHeader")]
    public string AlternateHeader
    {
      get
      {
        return m_web.AlternateHeader;
      }
      set
      {
        m_web.AlternateHeader = value;
      }
    }

    [JSProperty(Name = "associatedGroups")]
    public ArrayInstance AssociatedGroups
    {
      get
      {
        if (m_web.AssociatedGroups == null)
          return null;

        var result = this.Engine.Array.Construct();

        foreach (var group in m_web.AssociatedGroups)
        {
          ArrayInstance.Push(result, new SPGroupInstance(this.Engine.Object.InstancePrototype, group));
        }

        return result;
      }
    }

    [JSProperty(Name = "associatedMemberGroup")]
    public SPGroupInstance AssociatedMemberGroup
    {
      get
      {
        if (m_web.AssociatedMemberGroup == null)
          return null;

        return new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedMemberGroup);
      }
    }

    [JSProperty(Name = "associatedOwnerGroup")]
    public SPGroupInstance AssociatedOwnerGroup
    {
      get
      {
        if (m_web.AssociatedMemberGroup == null)
          return null;

        return new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedOwnerGroup);
      }
    }

    [JSProperty(Name = "associatedVisitorGroup")]
    public SPGroupInstance AssociatedVisitorGroup
    {
      get
      {
        if (m_web.AssociatedMemberGroup == null)
          return null;

        return new SPGroupInstance(this.Engine.Object.InstancePrototype, m_web.AssociatedVisitorGroup);
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
      get
      {

        return m_web.Features == null
          ? null
          : new SPFeatureCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Features);
      }
    }

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

    [JSProperty(Name = "listTemplates")]
    public SPListTemplateCollectionInstance ListTemplates
    {
      get
      {
        if (m_web.ListTemplates == null)
          return null;

        return new SPListTemplateCollectionInstance(this.Engine.Object.InstancePrototype, m_web.ListTemplates);
      }
    }

    [JSProperty(Name = "lists")]
    public SPListCollectionInstance Lists
    {
      get
      {
        if (m_web.Lists == null)
          return null;
        
        return new SPListCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Lists);
      }
    }

    [JSProperty(Name = "masterPageReferenceEnabled")]
    public bool MasterPageReferenceEnabled
    {
      get
      {
        return m_web.MasterPageReferenceEnabled;
      }
    }

    [JSProperty(Name = "masterUrl")]
    public string MasterUrl
    {
      get
      {
        return m_web.MasterUrl;
      }
      set
      {
        m_web.MasterUrl = value;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_web.Name;
      }
      set
      {
        m_web.Name = value;
      }
    }

    //TODO: Navigation

    [JSProperty(Name = "noCrawl")]
    public bool NoCrawl
    {
      get
      {
        return m_web.NoCrawl;
      }
      set
      {
        m_web.NoCrawl = value;
      }
    }

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
    public SPRoleDefinitionCollectionInstance RoleDefinitions
    {
      get
      {
        if (m_web.RoleDefinitions == null)
          return null;

        return new SPRoleDefinitionCollectionInstance(this.Engine.Object.InstancePrototype, m_web.RoleDefinitions);
      }
    }

    //Roles property is Deprecated

    [JSProperty(Name = "rootFolder")]
    public SPFolderInstance RootFolder
    {
      get { return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_web.RootFolder); }
    }

    [JSProperty(Name = "serverRelativeUrl")]
    public string ServerRelativeUrl
    {
      get { return m_web.ServerRelativeUrl; }
      set { m_web.ServerRelativeUrl = value; }
    }

    [JSProperty(Name = "showUrlStructureForCurrentUser")]
    public bool ShowUrlStructureForCurrentUser
    {
      get
      {
        return m_web.ShowUrlStructureForCurrentUser;
      }
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

    [JSProperty(Name = "siteLogoDescription")]
    public string SiteLogoDescription
    {
      get
      {
        return m_web.SiteLogoDescription;
      }
      set
      {
        m_web.SiteLogoDescription = value;
      }
    }

    [JSProperty(Name = "siteLogoUrl")]
    public string SiteLogoUrl
    {
      get
      {
        return m_web.SiteLogoUrl;
      }
      set
      {
        m_web.SiteLogoUrl = value;
      }
    }

    [JSProperty(Name = "siteUserInfoList")]
    public SPListInstance SiteUserInfoList
    {
      get
      {
        if (m_web.SiteUserInfoList == null)
          return null;

        return new SPListInstance(this.Engine, null, null, m_web.SiteUserInfoList);
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

    [JSProperty(Name = "theme")]
    public string Theme
    {
      get { return m_web.Theme; }
    }

    [JSProperty(Name = "themeCssUrl")]
    public string ThemeCssUrl
    {
      get { return m_web.ThemeCssUrl; }
    }

    [JSProperty(Name = "themeCssFolderUrl")]
    public string ThemeCssFolderUrl
    {
      get { return m_web.ThemedCssFolderUrl; }
      set
      {
        m_web.ThemedCssFolderUrl = value;
      }
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

    [JSProperty(Name = "userIsSiteAdmin")]
    public bool UserIsSiteAdmin
    {
      get { return m_web.UserIsSiteAdmin; }
    }

    [JSProperty(Name = "userIsWebAdmin")]
    public bool UserIsWebAdmin
    {
      get { return m_web.UserIsWebAdmin; }
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

    [JSProperty(Name = "webs")]
    public SPWebCollectionInstance Webs
    {
      get { return new SPWebCollectionInstance(this.Engine.Object.InstancePrototype, m_web.Webs); }
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

    [JSFunction(Name = "applyTheme")]
    public void ApplyTheme(string newTheme)
    {
      m_web.ApplyTheme(newTheme);
    }

    [JSFunction(Name = "applyWebTemplate")]
    public void ApplyWebTemplate(object webTemplate)
    {
      if (webTemplate is SPWebTemplateInstance)
        m_web.ApplyWebTemplate((webTemplate as SPWebTemplateInstance).WebTemplate);
      else
        m_web.ApplyWebTemplate(TypeConverter.ToString(webTemplate));
    }

    [JSFunction(Name = "createList")]
    public SPListInstance CreateList(object listCreationInfo)
    {
      return SPListCollectionInstance.CreateList(this.Engine, m_web.Lists, m_web.ListTemplates, listCreationInfo);
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
      return file == null
        ? null
        : new SPFileInstance(this.Engine.Object.InstancePrototype, file);
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

      return new SPListInstance(this.Engine, null, null, list);
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
        ArrayInstance.Push(instance, new SPListInstance(this.Engine, null, null, list));
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
      
      return new SPListInstance(this.Engine, null, null, list);
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
      return new SPListInstance(this.Engine, null, null, m_web.SiteUserInfoList);
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
        ArrayInstance.Push(result, new SPWebInstance(this.Engine, (SPWeb)web));
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

    [JSFunction(Name = "recycle")]
    public void Recycle()
    {
      m_web.Recycle();
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
