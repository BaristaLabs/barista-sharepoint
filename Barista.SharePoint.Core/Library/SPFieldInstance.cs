namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPFieldConstructor : ClrFunction
  {
    public SPFieldConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPField", new SPFieldInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFieldInstance Construct(SPFieldCollectionInstance fieldCollection, string arg1, object arg2)
    {
      if (fieldCollection == null)
        throw new JavaScriptException(this.Engine, "Error", "A field collection must be specified to create a new instance of an SPField.");

      var newField = arg2 == Undefined.Value
        ? new SPField(fieldCollection.SPFieldCollection, arg1)
        : new SPField(fieldCollection.SPFieldCollection, arg1, TypeConverter.ToString(arg2));
      
      return new SPFieldInstance(this.Engine.Object.InstancePrototype, newField);
    }
  }

  [Serializable]
  public class SPFieldInstance : ObjectInstance
  {
    private readonly SPField m_field;

    public SPFieldInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFieldInstance(ObjectInstance prototype, SPField field)
      : this(prototype)
    {
      if (field == null)
        throw new ArgumentNullException("field");

      m_field = field;
    }

    public SPField SPField
    {
      get { return m_field; }
    }

    #region Properties
    [JSProperty(Name = "aggregationFunction")]
    public string AggregationFunction
    {
      get
      {
        return m_field.AggregationFunction;
      }
      set
      {
        m_field.AggregationFunction = value;
      }
    }

    [JSProperty(Name = "allowDeletion")]
    public object AllowDeletion
    {
      get
      {
        if (m_field.AllowDeletion.HasValue == false)
          return Null.Value;

        return m_field.AllowDeletion.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.AllowDeletion = null;
          return;
        }

        m_field.AllowDeletion = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "authoringInfo")]
    public string AuthoringInfo
    {
      get
      {
        return m_field.AuthoringInfo;
      }
    }

    [JSProperty(Name = "canBeDeleted")]
    public bool CanBeDeleted
    {
      get
      {
        return m_field.CanBeDeleted;
      }
    }

    [JSProperty(Name = "canBeDisplayedInEditForm")]
    public bool CanBeDisplayedInEditForm
    {
      get
      {
        return m_field.CanBeDisplayedInEditForm;
      }
    }

    [JSProperty(Name = "canToggleHidden")]
    public bool CanToggleHidden
    {
      get
      {
        return m_field.CanToggleHidden;
      }
    }

    //CompositeIndexable

    [JSProperty(Name = "defaultFormula")]
    public string DefaultFormula
    {
      get
      {
        return m_field.DefaultFormula;
      }
      set
      {
        m_field.DefaultFormula = value;
      }
    }

    [JSProperty(Name = "defaultListField")]
    public bool DefaultListField
    {
      get
      {
        return m_field.DefaultListField;
      }
    }

    [JSProperty(Name = "defaultValue")]
    public string DefaultValue
    {
      get
      {
        return m_field.DefaultValue;
      }
      set
      {
        m_field.DefaultValue = value;
      }
    }

    //Default Value Typed...

    [JSProperty(Name = "description")]
    public string Description
    {
      get
      {
        return m_field.Description;
      }
      set
      {
        m_field.Description = value;
      }
    }

    //Description Resource

    [JSProperty(Name = "direction")]
    public string Direction
    {
      get
      {
        return m_field.Direction;
      }
      set
      {
        m_field.Direction = value;
      }
    }

    [JSProperty(Name = "displaySize")]
    public string DisplaySize
    {
      get
      {
        return m_field.DisplaySize;
      }
      set
      {
        m_field.DisplaySize = value;
      }
    }

    [JSProperty(Name = "enforceUniqueValues")]
    public bool EnforceUniqueValues
    {
      get
      {
        return m_field.EnforceUniqueValues;
      }
      set
      {
        m_field.EnforceUniqueValues = value;
      }
    }

    [JSProperty(Name = "fieldReferences")]
    public ArrayInstance FieldReferences
    {
      get
      {
        try
        {
          if (m_field.FieldReferences == null)
            return null;

          // ReSharper disable CoVariantArrayConversion
          return this.Engine.Array.Construct(m_field.FieldReferences);
          // ReSharper restore CoVariantArrayConversion
        }
        catch (ArgumentOutOfRangeException)
        {
          //Do nothing...
          return null;
        }
      }
    }

    //Field Rendering Control
    //Field Rendering Mobile Contrl
    //Field Type Definition
    //Field Value Type

    [JSProperty(Name = "filterable")]
    public bool Filterable
    {
      get
      {
        return m_field.Filterable;
      }
    }

    [JSProperty(Name = "filterableNoRecurrence")]
    public bool FilterableNoRecurrence
    {
      get
      {
        return m_field.FilterableNoRecurrence;
      }
    }

    [JSProperty(Name = "fromBaseType")]
    public bool FromBaseType
    {
      get
      {
        return m_field.FromBaseType;
      }
    }

    [JSProperty(Name = "group")]
    public string Group
    {
      get
      {
        return m_field.Group;
      }
      set
      {
        m_field.Group = value;
      }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get
      {
        return m_field.Hidden;
      }
      set
      {
        m_field.Hidden = value;
      }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get
      {
        return new GuidInstance(this.Engine.Object.InstancePrototype, m_field.Id);
      }
    }

    [JSProperty(Name = "imeMode")]
    public string ImeMode
    {
      get
      {
        return m_field.IMEMode;
      }
      set
      {
        m_field.IMEMode = value;
      }
    }

    [JSProperty(Name = "indexable")]
    public bool Indexable
    {
      get
      {
        return m_field.Indexable;
      }
    }

    [JSProperty(Name = "indexed")]
    public bool Indexed
    {
      get
      {
        return m_field.Indexed;
      }
      set
      {
        m_field.Indexed = value;
      }
    }

    [JSProperty(Name = "internalName")]
    public string InternalName
    {
      get
      {
        return m_field.InternalName;
      }
    }

    [JSProperty(Name = "jumpToField")]
    public string JumpToField
    {
      get
      {
        return m_field.JumpToField;
      }
      set
      {
        m_field.JumpToField = value;
      }
    }

    [JSProperty(Name = "linkToItem")]
    public bool LinkToItem
    {
      get
      {
        return m_field.LinkToItem;
      }
      set
      {
        m_field.LinkToItem = value;
      }
    }

    //LinkToItemAllowed

    [JSProperty(Name = "listItemMenu")]
    public bool ListItemMenu
    {
      get
      {
        return m_field.ListItemMenu;
      }
      set
      {
        m_field.ListItemMenu = value;
      }
    }

    //ListItemMenuAllowed

    [JSProperty(Name = "noCrawl")]
    public bool NoCrawl
    {
      get
      {
        return m_field.NoCrawl;
      }
      set
      {
        m_field.NoCrawl = value;
      }
    }

    //Parent List

    [JSProperty(Name = "piAttribute")]
    public string PiAttribute
    {
      get
      {
        return m_field.PIAttribute;
      }
      set
      {
        m_field.PIAttribute = value;
      }
    }

    [JSProperty(Name = "piTarget")]
    public string PiTarget
    {
      get
      {
        return m_field.PITarget;
      }
      set
      {
        m_field.PITarget = value;
      }
    }

    //Preview Value Typed

    [JSProperty(Name = "primaryPIAttribute")]
    public string PrimaryPiAttribute
    {
      get
      {
        return m_field.PrimaryPIAttribute;
      }
      set
      {
        m_field.PrimaryPIAttribute = value;
      }
    }

    [JSProperty(Name = "primaryPITarget")]
    public string PrimaryPiTarget
    {
      get
      {
        return m_field.PrimaryPITarget;
      }
      set
      {
        m_field.PrimaryPITarget = value;
      }
    }

    [JSProperty(Name = "pushChangesToLists")]
    public bool PushChangesToLists
    {
      get
      {
        return m_field.PushChangesToLists;
      }
      set
      {
        m_field.PushChangesToLists = value;
      }
    }

    [JSProperty(Name = "readOnlyField")]
    public bool ReadOnlyField
    {
      get
      {
        return m_field.ReadOnlyField;
      }
      set
      {
        m_field.ReadOnlyField = value;
      }
    }

    [JSProperty(Name = "relatedField")]
    public string RelatedField
    {
      get
      {
        return m_field.RelatedField;
      }
      set
      {
        m_field.RelatedField = value;
      }
    }

    [JSProperty(Name = "reorderable")]
    public bool Reorderable
    {
      get
      {
        return m_field.Reorderable;
      }
    }

    [JSProperty(Name = "required")]
    public bool Required
    {
      get
      {
        return m_field.Required;
      }
      set
      {
        m_field.Required = value;
      }
    }

    [JSProperty(Name = "schemaXml")]
    public string SchemaXml
    {
      get
      {
        return m_field.SchemaXml;
      }
      set
      {
        m_field.SchemaXml = value;
      }
    }

    [JSProperty(Name = "schemaXmlWithResourceTokens")]
    public string SchemaXmlWithResourceTokens
    {
      get
      {
        return m_field.SchemaXmlWithResourceTokens;
      }
    }

    [JSProperty(Name = "scope")]
    public string Scope
    {
      get
      {
        return m_field.Scope;
      }
    }

    [JSProperty(Name = "sealed")]
    public bool Sealed
    {
      get
      {
        return m_field.Sealed;
      }
      set
      {
        m_field.Sealed = value;
      }
    }

    [JSProperty(Name = "showInDisplayForm")]
    public object ShowInDisplayForm
    {
      get
      {
        if (m_field.ShowInDisplayForm.HasValue == false)
          return Null.Value;

        return m_field.ShowInDisplayForm.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.ShowInDisplayForm = null;
          return;
        }

        m_field.ShowInDisplayForm = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "showInEditForm")]
    public object ShowInEditForm
    {
      get
      {
        if (m_field.ShowInEditForm.HasValue == false)
          return Null.Value;

        return m_field.ShowInEditForm.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.ShowInEditForm = null;
          return;
        }

        m_field.ShowInEditForm = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "showInListSettings")]
    public object ShowInListSettings
    {
      get
      {
        if (m_field.ShowInListSettings.HasValue == false)
          return Null.Value;

        return m_field.ShowInListSettings.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.ShowInListSettings = null;
          return;
        }

        m_field.ShowInListSettings = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "showInNewForm")]
    public object ShowInNewForm
    {
      get
      {
        if (m_field.ShowInNewForm.HasValue == false)
          return Null.Value;

        return m_field.ShowInNewForm.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.ShowInNewForm = null;
          return;
        }

        m_field.ShowInNewForm = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "showInVersionHistory")]
    public bool ShowInVersionHistory
    {
      get
      {
        return m_field.ShowInVersionHistory;
      }
      set
      {
        m_field.ShowInVersionHistory = value;
      }
    }

    [JSProperty(Name = "showInViewForms")]
    public object ShowInViewForms
    {
      get
      {
        if (m_field.ShowInViewForms.HasValue == false)
          return Null.Value;

        return m_field.ShowInViewForms.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value || value == null)
        {
          m_field.ShowInViewForms = null;
          return;
        }

        m_field.ShowInViewForms = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "sortable")]
    public bool Sortable
    {
      get
      {
        return m_field.Sortable;
      }
    }

    [JSProperty(Name = "sourceId")]
    public string SourceId
    {
      get
      {
        return m_field.SourceId;
      }
    }

    [JSProperty(Name = "staticName")]
    public string StaticName
    {
      get
      {
        return m_field.StaticName;
      }
      set
      {
        m_field.StaticName = value;
      }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get
      {
        return m_field.Title;
      }
      set
      {
        m_field.Title = value;
      }
    }
    
    //Title Resource

    [JSProperty(Name = "translationXml")]
    public string TranslationXml
    {
      get
      {
        return m_field.TranslationXml;
      }
      set
      {
        m_field.TranslationXml = value;
      }
    }

    //Type

    [JSProperty(Name = "typeAsString")]
    public string TypeAsString
    {
      get
      {
        return m_field.TypeAsString;
      }
      set
      {
        m_field.TypeAsString = value;
      }
    }

    [JSProperty(Name = "typeAsDisplayName")]
    public string TypeDisplayName
    {
      get
      {
        return m_field.TypeDisplayName;
      }
    }

    [JSProperty(Name = "typeShortDescription")]
    public string TypeShortDescription
    {
      get
      {
        return m_field.TypeShortDescription;
      }
    }

    [JSProperty(Name = "usedInWebContentTypes")]
    public bool UsedInWebContentTypes
    {
      get
      {
        return m_field.UsedInWebContentTypes;
      }
    }

    [JSProperty(Name = "validationEcmaScript")]
    public string ValidationEcmaScript
    {
      get
      {
        return m_field.ValidationEcmaScript;
      }
    }

    [JSProperty(Name = "validationFormula")]
    public string ValidationFormula
    {
      get
      {
        return m_field.ValidationFormula;
      }
      set
      {
        m_field.ValidationFormula = value;
      }
    }

    [JSProperty(Name = "validationMessage")]
    public string ValidationMessage
    {
      get
      {
        return m_field.ValidationMessage;
      }
      set
      {
        m_field.ValidationMessage = value;
      }
    }

    [JSProperty(Name = "version")]
    public int Version
    {
      get
      {
        return m_field.Version;
      }
    }

    [JSProperty(Name = "xPath")]
    public string XPath
    {
      get
      {
        return m_field.XPath;
      }
      set
      {
        m_field.XPath = value;
      }
    }
    #endregion

    #region Functions

    [JSFunction(Name="delete")]
    public void Delete()
    {
      m_field.Delete();
    }

    [JSFunction(Name = "getFieldValueAsHtml")]
    public string GetFieldValueAsHtml(object value)
    {
      return m_field.GetFieldValueAsHtml(value);
    }

    [JSFunction(Name = "getFieldValueAsText")]
    public string GetFieldValueAsText(object value)
    {
      return m_field.GetFieldValueAsText(value);
    }

    [JSFunction(Name = "getFieldValueForEdit")]
    public string GetFieldValueForEdit(object value)
    {
      return m_field.GetFieldValueForEdit(value);
    }

    [JSFunction(Name = "getProperty")]
    public string GetProperty(string propertyName)
    {
      return m_field.GetProperty(propertyName);
    }

    [JSFunction(Name = "parseAndSetValue")]
    public void ParseAndSetValue(SPListItemInstance listItem, string value)
    {
      if (listItem == null)
        throw new JavaScriptException(this.Engine, "Error", "A list item must be specified as the first parameter.");

      m_field.ParseAndSetValue(listItem.ListItem, value);
    }

    [JSFunction(Name = "setCustomProperty")]
    public void SetCustomProperty(string propertyName, string propertyValue)
    {
      m_field.SetCustomProperty(propertyName, propertyValue);
    }

    [JSFunction(Name = "update")]
    public void Update(object pushChangesToLists)
    {
      if (pushChangesToLists == null || pushChangesToLists == Null.Value || pushChangesToLists == Undefined.Value)
        m_field.Update();
      else
        m_field.Update(TypeConverter.ToBoolean(pushChangesToLists));
    }

    #endregion

    public static void ThrowUnknownFieldTypeException(ScriptEngine engine)
    {
      throw new JavaScriptException(engine, "Error", "Unknown or unsupported value for the FieldType parameter. Supported Field Types are:" +
                                                    "AllDayEvent," +
                                                    "Attachments," +
                                                    "Boolean," +
                                                    "Calculated," +
                                                    "Choice," +
                                                    "Computed," +
                                                    "ContentTypeId," +
                                                    "Counter," +
                                                    "CrossProjectLink," +
                                                    "Currency," +
                                                    "DateTime," +
                                                    "Error," +
                                                    "File," +
                                                    "GridChoice," +
                                                    "Guid," +
                                                    "Integer," +
                                                    "Invalid," +
                                                    "Lookup," +
                                                    "MaxItems," +
                                                    "ModStat," +
                                                    "MultiChoice," +
                                                    "Note," +
                                                    "Number," +
                                                    "PageSeperator," +
                                                    "Recurrence," +
                                                    "Text," +
                                                    "ThreadIndex," +
                                                    "Threading," +
                                                    "URL," +
                                                    "User," +
                                                    "WorkflowEventType," +
                                                    "WorkflowStatus");
    }
  }
}
