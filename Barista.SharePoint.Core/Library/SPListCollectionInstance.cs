namespace Barista.SharePoint.Library
{
  using System.Globalization;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using System;
  using System.Linq;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPListCollectionConstructor : ClrFunction
  {
    public SPListCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPListCollection", new SPListCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListCollectionInstance Construct()
    {
      return new SPListCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPListCollectionInstance : ObjectInstance
  {
    private readonly SPListCollection m_listCollection;

    public SPListCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListCollectionInstance(ObjectInstance prototype, SPListCollection listCollection)
      : this(prototype)
    {
      if (listCollection == null)
        throw new ArgumentNullException("listCollection");

      m_listCollection = listCollection;
    }

    public SPListCollection SPListCollection
    {
      get { return m_listCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_listCollection.Count;
      }
    }

    [JSProperty(Name = "includeMobileDefaultViewUrl")]
    public bool IncludeMobileDefaultViewUrl
    {
      get
      {
        return m_listCollection.IncludeMobileDefaultViewUrl;
      }
      set
      {
        m_listCollection.IncludeMobileDefaultViewUrl = value;
      }
    }

    [JSProperty(Name = "includeRootFolder")]
    public bool IncludeRootFolder
    {
      get
      {
        return m_listCollection.IncludeRootFolder;
      }
      set
      {
        m_listCollection.IncludeRootFolder = value;
      }
    }

    
    [JSProperty(Name = "listsForCurrentUser")]
    public bool ListsForCurrentUser
    {
      get
      {
        return m_listCollection.ListsForCurrentUser;
      }
      set
      {
        m_listCollection.ListsForCurrentUser = value;
      }
    }

    [JSFunction(Name = "addList")]
    public SPListInstance AddList(object listCreationInfo)
    {
      return SPListCollectionInstance.CreateList(this.Engine, m_listCollection, null, listCreationInfo);
    }

    [JSFunction(Name = "delete")]
    public void Delete(object guid)
    {
      var guidInstance = GuidInstance.ConvertFromJsObjectToGuid(guid);

      m_listCollection.Delete(guidInstance);
    }

    [JSFunction(Name = "ensureSiteAssetsLibrary")]
    public SPListInstance EnsureSiteAssetsLibrary()
    {
      var result = m_listCollection.EnsureSiteAssetsLibrary();

      if (result == null)
        return null;

      return new SPListInstance(this.Engine, null, null, result);
    }

    [JSFunction(Name = "ensureSitePagesLibrary")]
    public SPListInstance EnsureSitePagesLibrary()
    {
      var result = m_listCollection.EnsureSitePagesLibrary();

      if (result == null)
        return null;

      return new SPListInstance(this.Engine, null, null, result);
    }

    [JSFunction(Name = "getListByGuid")]
    public SPListInstance GetListByGuid(object guid, object fetchMetadata, object fetchSecurityData, object fetchRelatedFields)
    {
      var guidInstance = GuidInstance.ConvertFromJsObjectToGuid(guid);

      SPList list;

      if (fetchMetadata != Undefined.Value && fetchSecurityData == Undefined.Value &&
               fetchRelatedFields == Undefined.Value)
      {
        list = m_listCollection.GetList(guidInstance, TypeConverter.ToBoolean(fetchMetadata));
      }
      else if (fetchMetadata != Undefined.Value && fetchSecurityData != Undefined.Value &&
               fetchRelatedFields == Undefined.Value)
      {
        list = m_listCollection.GetList(guidInstance, TypeConverter.ToBoolean(fetchMetadata), TypeConverter.ToBoolean(fetchSecurityData));
      }
      else if (fetchMetadata != Undefined.Value && fetchSecurityData != Undefined.Value &&
               fetchRelatedFields != Undefined.Value)
      {
        list = m_listCollection.GetList(guidInstance, TypeConverter.ToBoolean(fetchMetadata), TypeConverter.ToBoolean(fetchSecurityData), TypeConverter.ToBoolean(fetchRelatedFields));
      }
      else
      {
        list = m_listCollection[guidInstance];
      }

      if (list == null)
        return null;

      return new SPListInstance(this.Engine, null, null, list);
    }

    [JSFunction(Name = "getListByIndex")]
    public SPListInstance GetListByIndex(int index)
    {
      var list = m_listCollection[index];
      if (list == null)
        return null;

      return new SPListInstance(this.Engine, null, null, list);
    }

    [JSFunction(Name = "getListByListName")]
    public SPListInstance GetListByListName(string listName)
    {
      var list = m_listCollection[listName];
      if (list == null)
        return null;

      return new SPListInstance(this.Engine, null, null, list);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var list in m_listCollection.OfType<SPList>())
      {
        ArrayInstance.Push(result, new SPListInstance(this.Engine, null, null, list));
      }
      return result;
    }

    [JSFunction(Name = "tryGetList")]
    public SPListInstance TryGetList(string listName)
    {
      var list = m_listCollection.TryGetList(listName);
      if (list == null)
        return null;

      return new SPListInstance(this.Engine, null, null, list);
    }

    public static SPListInstance CreateList(ScriptEngine engine, SPListCollection collection, SPListTemplateCollection templates, object listCreationInfo)
    {
      Guid createdListId;

      if (listCreationInfo == null || listCreationInfo == Null.Value || listCreationInfo == Undefined.Value)
        throw new JavaScriptException(engine, "Error", "A List Creation Info object must be specified.");

      var listCreationInstance = listCreationInfo as ObjectInstance;
      var creationInfo = JurassicHelper.Coerce<SPListCreationInformation>(engine, listCreationInfo);
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
        
        createdListId = collection.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, dataSource);
      }
      //If listTemplate property has a value, create the list instance using the strongly-typed SPListTemplate, optionally using the docTemplate value.
      else if (listCreationInstance != null && listCreationInstance.HasProperty("listTemplate") && templates != null)
      {
        var listTemplateValue = listCreationInstance.GetPropertyValue("listTemplate");
        
        SPListTemplate listTemplate = null;
        if (listTemplateValue is int)
        {
          listTemplate = templates.OfType<SPListTemplate>().FirstOrDefault(dt => (int)dt.Type == (int)listTemplateValue);
        }
        else
        {
          var s = listTemplateValue as string;

          if (s != null)
          {
            listTemplate = templates.OfType<SPListTemplate>().FirstOrDefault(dt => dt.Type.ToString() == s);
          }
          else if (listTemplateValue is ObjectInstance)
          {
            listTemplate = JurassicHelper.Coerce<SPListTemplateInstance>(engine, listTemplateValue).ListTemplate;
          }
        }

        if (listTemplate == null)
          return null;

        if (listCreationInstance.HasProperty("docTemplate"))
        {
          var docTemplate = JurassicHelper.Coerce<SPDocTemplateInstance>(engine, listCreationInstance.GetPropertyValue("docTemplate"));
          createdListId = collection.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, listTemplate.FeatureId.ToString(), listTemplate.Type_Client, docTemplate.DocTemplate.Type.ToString(CultureInfo.InvariantCulture), quickLaunchOptions);
        }
        else
        {
          createdListId = collection.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, listTemplate.FeatureId.ToString(), listTemplate.Type_Client, String.Empty, quickLaunchOptions);
        }
      }
      //Otherwise attempt to create the list using all properties set on the creation info object.
      else
      {
        SPFeatureDefinition listInstanceFeatureDefinition = null;
        if (listCreationInstance != null && listCreationInstance.HasProperty("listInstanceFeatureDefinition"))
        {
          var featureDefinitionInstance = JurassicHelper.Coerce<SPFeatureDefinitionInstance>(engine, listCreationInstance.GetPropertyValue("listInstanceFeatureDefinition"));
          listInstanceFeatureDefinition = featureDefinitionInstance.FeatureDefinition;
        }

        createdListId = collection.Add(creationInfo.Title, creationInfo.Description, creationInfo.Url, creationInfo.TemplateFeatureId, creationInfo.TemplateType, creationInfo.DocumentTemplateType, creationInfo.CustomSchemaXml, listInstanceFeatureDefinition, quickLaunchOptions);
      }
      
      var createdList = collection[createdListId];
      return new SPListInstance(engine, null, null, createdList);
    }
  }
}
