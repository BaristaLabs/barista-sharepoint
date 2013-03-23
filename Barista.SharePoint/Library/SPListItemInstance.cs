namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Taxonomy;
  using Barista.SharePoint.Taxonomy.Library;
  using System.Text;

  [Serializable]
  public class SPListItemConstructor : ClrFunction
  {
    public SPListItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPList", new SPListItemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPListItemInstance Construct(string listItemUrl)
    {
      SPListItem listItem;
      if (SPHelper.TryGetSPListItem(listItemUrl, out listItem))
        return new SPListItemInstance(this.InstancePrototype, listItem);

      throw new JavaScriptException(this.Engine, "Error", "A list at the specified url was not found.");
    }

    public SPListItemInstance Construct(SPListItem listItem)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem");

      return new SPListItemInstance(this.InstancePrototype, listItem);
    }
  }

  [Serializable]
  public class SPListItemInstance : ObjectInstance
  {
    private readonly SPListItem m_listItem;

    public SPListItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPListItemInstance(ObjectInstance prototype, SPListItem listItem)
      : this(prototype)
    {
      this.m_listItem = listItem;
    }

    public SPListItemInstance(ScriptEngine engine, SPListItem listItem)
      : this(engine.Object.InstancePrototype, listItem)
    {
    }

    #region Properties

    [JSProperty(Name = "contentTypeId")]
    public SPContentTypeIdInstance ContentTypeId
    {
      get { return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, m_listItem.ContentTypeId); }
    }

    [JSProperty(Name = "displayName")]
    public string DisplayName
    {
      get
      {
        //This is asinine, but when retriving listitems through a filtered view, the display name might not be in the results.
        var result = String.Empty;
        try
        {
          result = m_listItem.DisplayName;
          return result;
        }
        catch { /* do nothing... */ }

        return result;
      }
    }

    [JSProperty(Name = "fieldValues")]
    public ObjectInstance FieldValues
    {
      get
      {
        return GetFieldValuesAsObject(this.Engine, m_listItem);
      }
    }

    [JSProperty(Name = "fieldValuesAsHtml")]
    public ObjectInstance FieldValuesAsHtml
    {
      get
      {
        return GetFieldValuesAsHtml(this.Engine, m_listItem);
      }
    }

    [JSProperty(Name = "fieldValuesAsText")]
    public ObjectInstance FieldValuesAsText
    {
      get
      {
        return GetFieldValuesAsText(this.Engine, m_listItem);
      }
    }

    [JSProperty(Name = "fieldValuesForEdit")]
    public ObjectInstance FieldValuesForEdit
    {
      get
      {
        return GetFieldValuesForEdit(this.Engine, m_listItem);
      }
    }

    [JSProperty(Name = "fileSystemObjectType")]
    public string FileSystemObjectType
    {
      get { return m_listItem.FileSystemObjectType.ToString(); }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_listItem.ID; }
    }

    [JSProperty(Name = "moderationInformation")]
    public SPModerationInformationInstance ModerationInformation
    {
      get
      {
        if (m_listItem.ModerationInformation == null)
          return null;

        return new SPModerationInformationInstance(this.Engine.Object.InstancePrototype,
                                                   m_listItem.ModerationInformation);
      }
    }

    #endregion

    #region Functions
    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_listItem.Delete();
    }

    [JSFunction(Name = "getContentType")]
    public SPContentTypeInstance GetContentType()
    {
      var contentType = this.m_listItem.ContentType;
      if (this.m_listItem.ContentType == null)
        contentType = m_listItem.ParentList.ContentTypes[m_listItem.ContentTypeId];

      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType);
    }

    [JSFunction(Name = "getFile")]
    public SPFileInstance GetFile()
    {
      if (m_listItem.File == null)
        return null;

      return new SPFileInstance(this.Engine.Object.InstancePrototype, m_listItem.File);
    }

    [JSFunction(Name = "getFileContentsAsJson")]
    public object GetFileContentsAsJson()
    {
      if (m_listItem.File == null)
        return null;

      var fileContents = m_listItem.File.OpenBinary(SPOpenBinaryOptions.None);
      return JSONObject.Parse(this.Engine, Encoding.UTF8.GetString(fileContents), null);
    }

    [JSFunction(Name = "getParentList")]
    public SPListInstance GetParentList()
    {
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_listItem.ParentList);
    }

    [JSFunction(Name = "getPermissions")]
    public SPSecurableObjectInstance GetPermissions()
    {
      return new SPSecurableObjectInstance(this.Engine.Object.InstancePrototype, m_listItem);
    }

    [JSFunction(Name = "parseAndSetValue")]
    public void ParseAndSetValue(string fieldName, string value)
    {
      var field = m_listItem.Fields[fieldName];

      field.ParseAndSetValue(m_listItem, value);
    }

    [JSFunction(Name = "setFieldValues")]
    public void SetFieldValues(object fieldValues)
    {
      var ht = SPHelper.GetFieldValuesHashtableFromPropertyObject(fieldValues);
      foreach (var key in ht.Keys)
      {
        var fieldName = TypeConverter.ToString(key);
        if (m_listItem.Fields.ContainsField(fieldName))
        {
          m_listItem[fieldName] = ht[key];
        }
      }
    }

    [JSFunction(Name = "setFieldValue")]
    public void SetFieldValue(string fieldName, TermInstance fieldValue)
    {
      var taxonomyField = m_listItem.Fields[fieldName] as TaxonomyField;
      if (taxonomyField != null)
      {
        var term = fieldValue;
        taxonomyField.SetFieldValue(m_listItem, term.Term);
      }
    }

    public void SetFieldValue(string fieldName, string fieldValue)
    {
      var field = m_listItem.Fields[fieldName];

      field.ParseAndSetValue(m_listItem, fieldValue);
    }

    [JSFunction(Name = "systemUpdate")]
    public void SystemUpdate([DefaultParameterValue(false)] bool incrementListItemVersion)
    {
      m_listItem.SystemUpdate(incrementListItemVersion);
    }

    [JSFunction(Name = "recycle")]
    public string Recycle()
    {
      return m_listItem.Recycle().ToString();
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_listItem.Update();
    }

    [JSFunction(Name = "updateOverwriteVersion")]
    public void UpdateOverwriteVersion()
    {
      m_listItem.UpdateOverwriteVersion();
    }
    #endregion

    #region Static Functions
    public static ObjectInstance GetFieldValuesAsObject(ScriptEngine engine, SPListItem listItem)
    {
      var result = engine.Object.Construct();

      var fields = listItem.Fields;

      foreach (var field in fields.OfType<SPField>())
      {
        switch (field.Type)
        {
          case SPFieldType.Integer:
            {
              int value;
              if (listItem.TryGetSPFieldValue(field.Id, out value))
              {
                result.SetPropertyValue(field.InternalName, value, false);
                //ArrayInstance.Push(result, Engine.Array.Construct(field.Title, value));
              }
            }
            break;
          case SPFieldType.Boolean:
            {
              bool value;
              if (listItem.TryGetSPFieldValue(field.Id, out value))
              {
                result.SetPropertyValue(field.InternalName, value, false);
              }
            }
            break;
          case SPFieldType.Number:
            {
              double value;
              if (listItem.TryGetSPFieldValue(field.Id, out value))
              {
                result.SetPropertyValue(field.InternalName, value, false);
              }
            }
            break;
          case SPFieldType.DateTime:
            {
              DateTime value;
              if (listItem.TryGetSPFieldValue(field.Id, out value))
              {
                result.SetPropertyValue(field.InternalName, JurassicHelper.ToDateInstance(engine, new DateTime(value.Ticks, DateTimeKind.Local)), false);
              }
            }
            break;
          case SPFieldType.User:
            {
              string userToken;
              if (listItem.TryGetSPFieldValue(field.Id, out userToken))
              {
                var fieldUserValue = new SPFieldUserValue(listItem.ParentList.ParentWeb, userToken);
                var userInstance = new SPUserInstance(engine.Object.InstancePrototype, fieldUserValue.User);
                result.SetPropertyValue(field.InternalName, userInstance, false);
              }
            }
            break;
          default:
            {
              object value;
              if (listItem.TryGetSPFieldValue(field.Id, out value))
              {
                var stringValue = field.GetFieldValueAsText(value);

                if (result.HasProperty(field.InternalName) == false)
                  result.SetPropertyValue(field.InternalName, stringValue, false);
              }
            }
            break;
        }
      }

      return result;
    }

    public static ObjectInstance GetFieldValuesAsHtml(ScriptEngine engine, SPListItem listItem)
    {
      var result = engine.Object.Construct();

      var fields = listItem.Fields;

      foreach (var field in fields.OfType<SPField>())
      {
        object value;
        if (listItem.TryGetSPFieldValue(field.Id, out value))
        {
          var stringValue = field.GetFieldValueAsHtml(value);
          result.SetPropertyValue(field.InternalName, stringValue, false);
        }
      }

      return result;
    }

    public static ObjectInstance GetFieldValuesAsText(ScriptEngine engine, SPListItem listItem)
    {
      var result = engine.Object.Construct();

      var fields = listItem.Fields;

      foreach (var field in fields.OfType<SPField>())
      {
        object value;
        if (listItem.TryGetSPFieldValue(field.Id, out value))
        {
          var stringValue = field.GetFieldValueAsText(value);
          result.SetPropertyValue(field.InternalName, stringValue, false);
        }
      }

      return result;
    }

    public static ObjectInstance GetFieldValuesForEdit(ScriptEngine engine, SPListItem listItem)
    {
      var result = engine.Object.Construct();

      var fields = listItem.Fields;

      foreach (var field in fields.OfType<SPField>())
      {
        object value;
        if (listItem.TryGetSPFieldValue(field.Id, out value))
        {
          var stringValue = field.GetFieldValueForEdit(value);
          result.SetPropertyValue(field.InternalName, stringValue, false);
        }
      }

      return result;
    }
    #endregion
  }
}
