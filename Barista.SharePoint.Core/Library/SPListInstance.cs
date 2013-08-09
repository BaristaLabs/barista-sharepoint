namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Collections.Generic;
  using Barista.Library;
  using System.Text;
  using Microsoft.SharePoint.Utilities;

  [Serializable]
  public class SPListConstructor : ClrFunction
  {
    public SPListConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPList", new SPListInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListInstance Construct(string listUrl)
    {
      SPSite site;
      SPWeb web;
      SPList list;
      if (SPHelper.TryGetSPList(listUrl, out site, out web, out list))
      {
        var result = new SPListInstance(this.InstancePrototype, site, web, list);

        return result;
      }

      throw new JavaScriptException(this.Engine, "Error", "A list at the specified url was not found.");
    }

    public SPListInstance Construct(SPList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      return new SPListInstance(this.InstancePrototype, null, null, list);
    }
  }

  [Serializable]
  public class SPListInstance : ObjectInstance, IDisposable
  {
    private SPSite m_site;
    private SPWeb m_web;

    private readonly SPList m_list;

    public SPListInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListInstance(ObjectInstance prototype, SPSite site, SPWeb web, SPList list)
      : this(prototype)
    {
      this.m_site = site;
      this.m_web = web;
      this.m_list = list;
    }

    public SPListInstance(ScriptEngine engine, SPList list)
      : this(engine.Object.InstancePrototype, null, null, list)
    {
    }

    public SPList List
    {
      get { return m_list; }
    }

    #region Properties

    [JSProperty(Name = "allowContentTypes")]
    public bool AllowContentTypes
    {
      get
      {
        try
        {
          return m_list.AllowContentTypes;
        }
        catch (Exception)
        {
          return false;
        }
        
      }
    }

    [JSProperty(Name = "baseTemplate")]
    public int BaseTemplate
    {
      get { return (int)m_list.BaseTemplate; }
    }

    //[JSProperty(Name = "baseType")]
    //public object BaseType
    //{
    //  get { return m_list.BaseType; }
    //}

    [JSProperty(Name = "contentTypesEnabled")]
    public bool ContentTypesEnabled
    {
      get { return m_list.ContentTypesEnabled; }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_list.Created); }
    }

    [JSProperty(Name = "defaultDisplayFormUrl")]
    public string DefaultDisplayFormUrl
    {
      get { return m_list.DefaultDisplayFormUrl; }
      set { m_list.DefaultDisplayFormUrl = value; }
    }

    [JSProperty(Name = "defaultEditFormUrl")]
    public string DefaultEditFormUrl 
    {
      get { return m_list.DefaultEditFormUrl; }
      set { m_list.DefaultEditFormUrl = value; }
    }

    [JSProperty(Name = "defaultNewFormUrl")]
    public string DefaultNewFormUrl 
    {
      get { return m_list.DefaultNewFormUrl; }
      set { m_list.DefaultNewFormUrl = value; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_list.Description; }
      set { m_list.Description = value; }
    }

    [JSProperty(Name = "direction")]
    public string Direction
    {
      get { return m_list.Direction; }
      set { m_list.Direction = value; }
    }

    [JSProperty(Name = "draftVersionVisibility")]
    public string DraftVersionVisibility
    {
      get { return m_list.DraftVersionVisibility.ToString(); }
      set { m_list.DraftVersionVisibility = (DraftVisibilityType)Enum.Parse(typeof(DraftVisibilityType), value); }
    }

    [JSProperty(Name = "enableAttachments")]
    public bool EnableAttachments
    {
      get { return m_list.EnableAttachments; }
      set { m_list.EnableAttachments = value; }
    }

    [JSProperty(Name = "enableFolderCreation")]
    public bool EnableFolderCreation
    {
      get { return m_list.EnableFolderCreation; }
      set { m_list.EnableFolderCreation = value; }
    }

    [JSProperty(Name = "enableMinorVersions")]
    public bool EnableMinorVersions
    {
      get { return m_list.EnableMinorVersions; }
      set { m_list.EnableMinorVersions = value; }
    }

    [JSProperty(Name = "enableModeration")]
    public bool EnableModeration 
    {
      get { return m_list.EnableModeration; }
      set { m_list.EnableModeration = value; }
    }

    [JSProperty(Name = "enableVersioning")]
    public bool EnableVersioning
    {
      get { return m_list.EnableVersioning; }
      set { m_list.EnableVersioning = value; }
    }

    [JSProperty(Name = "fields")]
    public SPFieldCollectionInstance Fields
    {
      get
      {
        return m_list.Fields == null
          ? null
          : new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_list.Fields);
      }
    }

    [JSProperty(Name = "forceCheckout")]
    public bool ForceCheckout
    {
      get { return m_list.ForceCheckout; }
      set { m_list.ForceCheckout = value; }
    }

    [JSProperty(Name = "hasExternalDataSource")]
    public bool HasExternalDataSource
    {
      get { return m_list.HasExternalDataSource; }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get { return m_list.Hidden; }
      set { m_list.Hidden = value; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_list.ID.ToString(); }
    }

    [JSProperty(Name = "imageUrl")]
    public string ImageUrl
    {
      get { return m_list.ImageUrl; }
    }

    [JSProperty(Name = "isApplicationList")]
    public bool IsApplicationList
    {
      get { return m_list.IsApplicationList; }
      set { m_list.IsApplicationList = value; }
    }

    //[JSProperty(Name = "isCatalog")]
    //public bool IsCatalog
    //{
    //  get { return m_list; }
    //  set { m_list.IsGallery = value; }
    //}

    [JSProperty(Name = "isSiteAssetsLibrary")]
    public bool IsSiteAssetsLibrary
    {
      get { return m_list.IsSiteAssetsLibrary; }
    }

    [JSProperty(Name = "itemCount")]
    public int ItemCount
    {
      get { return m_list.ItemCount; }
    }

    [JSProperty(Name = "lastItemDeletedDate")]
    public DateInstance LastItemDeletedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_list.LastItemDeletedDate); }
    }

    [JSProperty(Name = "lastItemModifiedDate")]
    public DateInstance LastItemModifiedDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_list.LastItemModifiedDate); }
    }

    [JSProperty(Name = "noCrawl")]
    public bool NoCrawl
    {
      get { return m_list.NoCrawl; }
      set { m_list.NoCrawl = value; }
    }

    [JSProperty(Name = "onQuickLaunch")]
    public bool OnQuickLaunch
    {
      get { return m_list.OnQuickLaunch; }
      set { m_list.OnQuickLaunch = value; }
    }

    [JSProperty(Name = "parentWebUrl")]
    public string ParentWebUrl
    {
      get { return m_list.ParentWebUrl; }
    }

    [JSProperty(Name = "rootFolder")]
    public SPFolderInstance RootFolder
    {
      get { return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_list.RootFolder); }
    }

    [JSProperty(Name = "serverTemplateCanCreateFolders")]
    public bool ServerTemplateCanCreateFolders
    {
      get { return m_list.ServerTemplateCanCreateFolders; }
    }

    [JSProperty(Name = "serverTemplateId")]
    public string ServerTemplateId
    {
      get
      {
        var serverTemplateId = m_list.RootFolder.Properties["vti_listservertemplate"]; //Gotta love SharePoint!!
        return serverTemplateId != null
          ? serverTemplateId.ToString()
          : String.Empty;
      } 
    }

    [JSProperty(Name = "templateFeatureId")]
    public string TemplateFeatureId
    {
      get { return m_list.TemplateFeatureId.ToString(); }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_list.Title; }
    }

    [JSProperty(Name = "validationFormula")]
    public string ValidationFormula
    {
      get
      {
        try
        {
          return m_list.ValidationFormula;
        }
        catch (SPException)
        {
          return String.Empty;
        }
      }
      set { m_list.ValidationFormula = value; }
    }

    [JSProperty(Name = "validationMessage")]
    public string ValidationMessage
    {
      get
      {
        try
        {
          return m_list.ValidationMessage;
        }
        catch (SPException)
        {
          return String.Empty;
        }
      }
      set { m_list.ValidationMessage = value; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return SPUtility.ConcatUrls(m_list.ParentWeb.Url, m_list.RootFolder.Url); }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "addEventReceiver")]
    public SPEventReceiverDefinitionInstance AddEventReceiver(string eventReceiverType, string assembly, string className, object code)
    {
      var receiverType = (SPEventReceiverType) Enum.Parse(typeof (SPEventReceiverType), eventReceiverType);

      var eventReceiversBefore = m_list.EventReceivers.OfType<SPEventReceiverDefinition>().ToList();

      m_list.EventReceivers.Add(receiverType, assembly, className);

      var eventReceiversAfter = m_list.EventReceivers.OfType<SPEventReceiverDefinition>().ToList();

      if (code != null && code != Null.Value && code != Undefined.Value)
      {
        m_list.RootFolder.AddProperty(Constants.BaristaItemEventReceiverCodePropertyBagKey, code.ToString());
        m_list.RootFolder.Update();
      }

      var result = eventReceiversAfter.Except(eventReceiversBefore).FirstOrDefault();
      return result != null
        ? new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, result)
        : null;
    }

    [JSFunction(Name = "addBaristaEventReceiver")]
    public SPEventReceiverDefinitionInstance AddBaristaEventReceiver(string eventReceiverType, object code)
    {
      var receiverType = (SPEventReceiverType)Enum.Parse(typeof(SPEventReceiverType), eventReceiverType);

      var baristaItemEventReceiverType =
        Type.GetType("Barista.SharePoint. BaristaItemEventReceiver, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52");

      if (baristaItemEventReceiverType == null)
        throw new InvalidOperationException("Unable to locate BaristaItemEventReceiver");

      var eventReceiversBefore = m_list.EventReceivers.OfType<SPEventReceiverDefinition>().ToList();

      m_list.EventReceivers.Add(receiverType, baristaItemEventReceiverType.Assembly.FullName, baristaItemEventReceiverType.FullName);

      var eventReceiversAfter = m_list.EventReceivers.OfType<SPEventReceiverDefinition>().ToList();

      if (code != null && code != Null.Value && code != Undefined.Value)
      {
        m_list.RootFolder.AddProperty(Constants.BaristaItemEventReceiverCodePropertyBagKey, code.ToString());
        m_list.RootFolder.Update();
      }

      var result = eventReceiversAfter.Except(eventReceiversBefore).FirstOrDefault();
      return result != null
        ? new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, result)
        : null;
    }

    [JSFunction(Name = "addItem")]
    public SPListItemInstance AddItem()
    {
      var listItem = m_list.Items.Add();
      return new SPListItemInstance(this.Engine.Object.InstancePrototype, listItem);
    }

    [JSFunction(Name = "addFile")]
    public SPFileInstance AddFile(string url, object data, object overwrite)
    {
      bool bOverwrite = false;
      byte[] byteData;

      if (String.IsNullOrEmpty(url))
        throw new JavaScriptException(this.Engine, "Error", "A url of the new file must be specified.");

      if (data == Null.Value || data == Undefined.Value || data == null)
        throw new JavaScriptException(this.Engine, "Error", "Data must be specified.");

      var instance = data as Base64EncodedByteArrayInstance;

      if (instance != null)
        byteData = instance.Data;
      else if (data is ObjectInstance)
        byteData = Encoding.UTF8.GetBytes(JSONObject.Stringify(this.Engine, data, null, null));
      else
        byteData = Encoding.UTF8.GetBytes(JSONObject.Stringify(this.Engine, data.ToString(), null, null));

      if (overwrite != Null.Value && overwrite != Undefined.Value && overwrite != null)
      {
        if (overwrite is Boolean)
          bOverwrite = (bool)overwrite;
        else
          bOverwrite = Boolean.Parse(overwrite.ToString());
      }

      var file = m_list.RootFolder.Files.Add(url, byteData, bOverwrite);
      return new SPFileInstance(this.Engine.Object.InstancePrototype, file);
    }

    [JSFunction(Name = "addItemToFolder")]
    public SPListItemInstance AddItem(string folderUrl)
    {
      var listItem = m_list.Items.Add(folderUrl, SPFileSystemObjectType.File);
      return new SPListItemInstance(this.Engine.Object.InstancePrototype, listItem);
    }

    [JSFunction(Name = "addContentType")]
    public SPContentTypeInstance AddContentType(object contentType)
    {
      SPContentTypeId bestMatch = SPContentTypeId.Empty;

      if (contentType is string)
      {
        bestMatch = m_list.ParentWeb.AvailableContentTypes.BestMatch(new SPContentTypeId(contentType as string));
      }
      else if (contentType is SPContentTypeIdInstance)
      {
        bestMatch = m_list.ParentWeb.AvailableContentTypes.BestMatch((contentType as SPContentTypeIdInstance).ContentTypeId);
      }
      else if (contentType is SPContentTypeInstance)
      {
        bestMatch = m_list.ParentWeb.AvailableContentTypes.BestMatch((contentType as SPContentTypeInstance).ContentType.Id);
      }

      if (bestMatch == SPContentTypeId.Empty)
        return null;

      SPContentType spContentType = m_list.ContentTypes.Add(m_list.ParentWeb.AvailableContentTypes[bestMatch]);

      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, spContentType);
    }

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_list.Delete();
    }

    /// <summary>
    /// Ensures that the specified content type has been defined on the list of content types defined on the list.
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    [JSFunction(Name = "ensureContentType")]
    public SPContentTypeInstance EnsureContentType(object contentType)
    {
      SPContentTypeId contentTypeIdToFind = SPContentTypeId.Empty;

      if (contentType is string)
      {
        contentTypeIdToFind = new SPContentTypeId(contentType as string);
      }
      else if (contentType is SPContentTypeIdInstance)
      {
        contentTypeIdToFind = (contentType as SPContentTypeIdInstance).ContentTypeId;
      }
      else if (contentType is SPContentTypeInstance)
      {
        contentTypeIdToFind = (contentType as SPContentTypeInstance).ContentType.Id;
      }

      if (contentTypeIdToFind == SPContentTypeId.Empty)
        return null;

      var spContentType = m_list.ContentTypes.OfType<SPContentType>().FirstOrDefault(ct => contentTypeIdToFind.IsParentOf(ct.Id));

      if (spContentType == null)
      {
        var bestMatch = m_list.ParentWeb.AvailableContentTypes.BestMatch(contentTypeIdToFind);

        spContentType = m_list.ContentTypes.Add(m_list.ParentWeb.AvailableContentTypes[bestMatch]);
      }

      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, spContentType);
    }

    [JSFunction(Name = "getItemById")]
    public object GetItemById(int id)
    {
      var item = m_list.GetItemById(id);

      SPListItemInstance instance = new SPListItemInstance(this.Engine, item);
      return instance;
    }

    [JSFunction(Name = "getItems")]
    public ArrayInstance GetItems()
    {
      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(m_list, false, (item) =>
      //  {
      //    items.Add(item);
      //  },
      //  (listItem, ex) =>
      //  {
      //    return false; //don't rethrow errors.
      //  });

      SPQuery query = new SPQuery {
        QueryThrottleMode = SPQueryThrottleOption.Override
      };

      var items = m_list.GetItems(query).OfType<SPListItem>().ToList<SPListItem>();

      var listItemInstances = items.Select(item => new SPListItemInstance(this.Engine, item));

// ReSharper disable CoVariantArrayConversion
      var result = Engine.Array.Construct(listItemInstances.ToArray());
// ReSharper restore CoVariantArrayConversion

      return result;
    }

    [JSFunction(Name = "getItemsByQuery")]
    public ArrayInstance GetItemsByQuery(object query)
    {
      SPQuery camlQuery;

      if (query is string)
      {
        camlQuery = new SPQuery {
          Query = query as string
        };
      }
      else if (query is SPCamlQueryInstance)
      {
        var queryInstance = query as SPCamlQueryInstance;
        camlQuery = queryInstance.SPQuery;
      }
      else if (query is SPCamlQueryBuilderInstance)
      {
        camlQuery = new SPQuery {
          Query = query.ToString()
        };
      }
      else
      {
        return null;
      }

      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(m_list, camlQuery, false, (item) =>
      //{
      //  items.Add(item);
      //},
      //  (listItem, ex) =>
      //  {
      //    return false; //don't rethrow errors.
      //  });
      camlQuery.QueryThrottleMode = SPQueryThrottleOption.Override;

      List<SPListItem> items = m_list.GetItems(camlQuery).OfType<SPListItem>().ToList<SPListItem>();

      var listItemInstances = items.Select(item => new SPListItemInstance(this.Engine, item));

// ReSharper disable CoVariantArrayConversion
      var result = Engine.Array.Construct(listItemInstances.ToArray());
// ReSharper restore CoVariantArrayConversion

      return result;
    }

    [JSFunction(Name = "getItemsByView")]
    public ArrayInstance GetItemsByView(object view)
    {
      SPView selectedView;

      if (view is string)
      {
        selectedView = m_list.Views[view as string];
      }
      else if (view is SPViewInstance)
      {
        var viewInstance = view as SPViewInstance;
        selectedView = m_list.Views[viewInstance.Title];
      }
      else
      {
        return null;
      }

      SPQuery query = new SPQuery(selectedView) {
        QueryThrottleMode = SPQueryThrottleOption.Override
      };

      //ContentIterator itemsIterator = new ContentIterator();
      //itemsIterator.ProcessListItems(m_list, query, false, (item) =>
      //  {
      //    items.Add(item);
      //  },
      //  (listItem, ex) =>
      //  {
      //    return false; //don't rethrow errors.
      //  });

      //var result = Engine.Array.Construct();
      //foreach (var item in items)
      //{
      //  SPListItemInstance instance = new SPListItemInstance(this.Engine, item);
      //  ArrayInstance.Push(result, instance);
      //}

      var items = m_list.GetItems(query).OfType<SPListItem>().ToList<SPListItem>();

      var listItemInstances = items.Select(item => new SPListItemInstance(this.Engine, item));

// ReSharper disable CoVariantArrayConversion
      var result = Engine.Array.Construct(listItemInstances.ToArray());
// ReSharper restore CoVariantArrayConversion

      return result;
    }

    [JSFunction(Name = "getParentWeb")]
    public SPWebInstance GetParentWeb()
    {
      return new SPWebInstance(this.Engine.Object.InstancePrototype, m_list.ParentWeb);
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, this.m_list);
    }

    [JSFunction(Name = "getContentTypes")]
    public ArrayInstance GetContentTypes()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentType in m_list.ContentTypes.OfType<SPContentType>())
      {
        ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
      }
      return result;
    }

    [JSFunction(Name = "getContentTypeById")]
    public SPContentTypeInstance GetContentTypeById(object contentType)
    {
      var bestMatch = SPContentTypeId.Empty;

      if (contentType is string)
      {
        bestMatch = m_list.ContentTypes.BestMatch(new SPContentTypeId(contentType as string));
      }
      else if (contentType is SPContentTypeIdInstance)
      {
        bestMatch = m_list.ContentTypes.BestMatch((contentType as SPContentTypeIdInstance).ContentTypeId);
      }
      else if (contentType is SPContentTypeInstance)
      {
        bestMatch = m_list.ContentTypes.BestMatch((contentType as SPContentTypeInstance).ContentType.Id);
      }

      if (bestMatch == SPContentTypeId.Empty)
        return null;

      var bestMatchContentType = m_list.ContentTypes[bestMatch];
      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, bestMatchContentType);
    }

    [JSFunction(Name = "getEventReceivers")]
    public ArrayInstance GetEventReceivers()
    {
      var result = this.Engine.Array.Construct();
      foreach (var eventReceiverDefinition in m_list.EventReceivers.OfType<SPEventReceiverDefinition>())
      {
        ArrayInstance.Push(result, new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, eventReceiverDefinition));
      }
      return result;
    }

    [JSFunction(Name = "getViews")]
    public ArrayInstance GetViews()
    {
      var result = this.Engine.Array.Construct();

      foreach (var view in m_list.Views.OfType<SPView>())
      {
        ArrayInstance.Push(result, new SPViewInstance(this.Engine.Object.InstancePrototype, view));
      }

      return result;
    }

    [JSFunction(Name = "getSchemaXml")]
    public string GetSchemaXml()
    {
      return m_list.SchemaXml;
    }

    [JSFunction(Name = "removeContentType")]
    public void RemoveContentType(object contentType)
    {
      var bestMatch = SPContentTypeId.Empty;

      if (contentType is string)
      {
        bestMatch = m_list.ContentTypes.BestMatch(new SPContentTypeId(contentType as string));
      }
      else if (contentType is SPContentTypeIdInstance)
      {
        bestMatch = m_list.ContentTypes.BestMatch((contentType as SPContentTypeIdInstance).ContentTypeId);
      }
      else if (contentType is SPContentTypeInstance)
      {
        bestMatch = m_list.ContentTypes.BestMatch((contentType as SPContentTypeInstance).ContentType.Id);
      }

      if (bestMatch == SPContentTypeId.Empty)
        return;

      m_list.ContentTypes.Delete(bestMatch);
    }

    [JSFunction(Name = "recycle")]
    public string Recycle()
    {
      return m_list.Recycle().ToString();
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_list.Update();
    }
    #endregion

    public void Dispose()
    {
      if (m_site != null) {
        m_site.Dispose();
        m_site = null;
      }

      if (m_web != null)
      {
        m_web.Dispose();
        m_web = null;
      }
    }
  }
}
