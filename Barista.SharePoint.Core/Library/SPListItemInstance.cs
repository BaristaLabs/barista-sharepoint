namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using Barista.Library;
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
      : base(engine.Function.InstancePrototype, "SPList", new SPListItemInstance(engine, null))
    {
    }

    [JSConstructorFunction]
    public SPListItemInstance Construct(string listItemUrl)
    {
      SPListItem listItem;
      if (SPHelper.TryGetSPListItem(listItemUrl, out listItem))
        return new SPListItemInstance(this.Engine, listItem);

      throw new JavaScriptException(this.Engine, "Error", "A list at the specified url was not found.");
    }

    public SPListItemInstance Construct(SPListItem listItem)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem");

      return new SPListItemInstance(this.Engine, listItem);
    }
  }

  [Serializable]
  public class SPListItemInstance : SPSecurableObjectInstance
  {
    private static readonly Regex DigitRegex = new Regex(@"^\d+;#.*$", RegexOptions.Compiled);

    private readonly SPListItem m_listItem;

    public SPListItemInstance(ScriptEngine engine, SPListItem listItem)
      : base(new SPSecurableObjectInstance(engine))
    {
      this.m_listItem = listItem;
      SecurableObject = this.m_listItem;

      this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
    }

    public SPListItem ListItem
    {
      get
      {
        return m_listItem;
      }
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

    [JSProperty(Name = "fields")]
    public SPFieldCollectionInstance Fields
    {
      get
      {
        return new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_listItem.Fields);
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
      return new SPListInstance(this.Engine, null, null, m_listItem.ParentList);
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

    [JSFunction(Name = "unlinkFromCopySource")]
    public void UnlinkFromCopySource()
    {
      m_listItem.UnlinkFromCopySource();
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
          case SPFieldType.URL:
          {
            SPFieldUrlValue urlValue;
            if (listItem.TryGetSPFieldValue(field.Id, out urlValue))
            {
              var item = engine.Object.Construct();
              item.SetPropertyValue("description", urlValue.Description, false);
              item.SetPropertyValue("url", urlValue.Url, false);

              result.SetPropertyValue(field.InternalName, item, false);
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
          case SPFieldType.Lookup:
          {
            var fieldType = field as SPFieldLookup;
            if (fieldType == null)
              continue;

            if (fieldType.AllowMultipleValues)
            {
              var fieldValue = listItem[field.Id] as SPFieldLookupValueCollection;

              var array = engine.Array.Construct();

              if (fieldValue != null)
              {
                foreach (var lookupValue in fieldValue)
                {
                  var item = engine.Object.Construct();
                  item.SetPropertyValue("lookupId", lookupValue.LookupId, false);
                  item.SetPropertyValue("lookupValue", lookupValue.LookupValue, false);

                  ArrayInstance.Push(array, item);
                }
              }

              result.SetPropertyValue(field.InternalName, array, false);
            }
            else
            {
              var fieldValue = listItem[field.Id];

              if (fieldValue is SPFieldUrlValue)
              {
                var urlValue = fieldValue as SPFieldUrlValue;
                var item = engine.Object.Construct();
                item.SetPropertyValue("description", urlValue.Description, false);
                item.SetPropertyValue("url", urlValue.Url, false);

                result.SetPropertyValue(field.InternalName, item, false);
              }
              else if (fieldValue is DateTime)
              {
                var value = (DateTime)fieldValue;
                result.SetPropertyValue(field.InternalName, JurassicHelper.ToDateInstance(engine, new DateTime(value.Ticks, DateTimeKind.Local)), false);
              }
              else if (fieldValue is SPFieldUserValue)
              {
                var fieldUserValue = (SPFieldUserValue)fieldValue;
                var userInstance = new SPUserInstance(engine.Object.InstancePrototype, fieldUserValue.User);
                result.SetPropertyValue(field.InternalName, userInstance, false);
              }
              else if (fieldValue is Guid)
              {
                var guidValue = (Guid) fieldValue;
                var guidInstance = new GuidInstance(engine.Object.InstancePrototype, guidValue);
                result.SetPropertyValue(field.InternalName, guidInstance, false);
              }
              else if (fieldValue is string)
              {
                //Attempt to create a new SPFieldLookupValue from the string
                if (DigitRegex.IsMatch((string)fieldValue, 0))
                {
                  try
                  {
                    var lookupValue = new SPFieldLookupValue((string) fieldValue);

                    var item = engine.Object.Construct();
                    item.SetPropertyValue("lookupId", lookupValue.LookupId, false);
                    item.SetPropertyValue("lookupValue", lookupValue.LookupValue, false);
                    result.SetPropertyValue(field.InternalName, item, false);
                  }
                  catch (ArgumentException)
                  {
                    result.SetPropertyValue(field.InternalName, fieldValue, false);
                  }
                }
                else
                {
                  result.SetPropertyValue(field.InternalName, fieldValue, false);
                }
              }
              else
              {
                result.SetPropertyValue(field.InternalName, fieldValue, false);
              }
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
        switch (field.Type)
        {
          case SPFieldType.Lookup:
            var fieldType = field as SPFieldLookup;

            if (fieldType == null)
              break;

            if (fieldType.AllowMultipleValues)
            {
              var fieldValue = listItem[field.Id] as SPFieldLookupValueCollection;

              var array = engine.Array.Construct();

              if (fieldValue != null)
              {
                foreach (var lookupValue in fieldValue)
                {
                  ArrayInstance.Push(array, lookupValue.LookupValue);
                }
              }

              result.SetPropertyValue(field.InternalName, array, false);
            }
            else
            {
              var fieldValue = listItem[field.Id];

              if (fieldValue == null)
                result.SetPropertyValue(field.InternalName, Null.Value, false);
              else
              {
                if (fieldValue is string)
                {
                  //Attempt to create a new SPFieldLookupValue from the string
                  if (DigitRegex.IsMatch((string) fieldValue, 0))
                  {
                    try
                    {
                      var lookupValue = new SPFieldLookupValue((string) fieldValue);
                      result.SetPropertyValue(field.InternalName, lookupValue.LookupValue, false);
                    }
                    catch (ArgumentException)
                    {
                      result.SetPropertyValue(field.InternalName, fieldValue.ToString(), false);
                    }
                  }
                  else
                  {
                    result.SetPropertyValue(field.InternalName, fieldValue.ToString(), false);
                  }
                }
              }
            }
            break;
          default:
          {
            object value;
            if (listItem.TryGetSPFieldValue(field.Id, out value))
            {
              var stringValue = field.GetFieldValueAsHtml(value);
              result.SetPropertyValue(field.InternalName, stringValue, false);
            }
            break;
          }
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
