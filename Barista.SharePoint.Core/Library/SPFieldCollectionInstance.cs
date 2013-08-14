namespace Barista.SharePoint.Library
{
  using System.Collections.Specialized;
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPFieldCollectionConstructor : ClrFunction
  {
    public SPFieldCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFieldCollection", new SPFieldCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFieldCollectionInstance Construct()
    {
      return new SPFieldCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPFieldCollectionInstance : ObjectInstance
  {
    private readonly SPFieldCollection m_fieldCollection;

    public SPFieldCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFieldCollectionInstance(ObjectInstance prototype, SPFieldCollection fieldCollection)
      : this(prototype)
    {
      if (fieldCollection == null)
        throw new ArgumentNullException("fieldCollection");

      m_fieldCollection = fieldCollection;
    }

    public SPFieldCollection SPFieldCollection
    {
      get { return m_fieldCollection; }
    }

    #region Properties
    [JSProperty(Name = "count")]
    public int Count
    {
      get { return m_fieldCollection.Count; }
    }

    [JSProperty(Name = "schemaXml")]
    public string SchemaXml
    {
      get { return m_fieldCollection.SchemaXml; }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "addDependentLookup")]
    public string AddDependentLookup(string displayName, object guid)
    {
      if (displayName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "A display name must be specified as the first argument.");

      var guidInstance = GuidInstance.ConvertFromJsObjectToGuid(guid);

      return m_fieldCollection.AddDependentLookup(displayName, guidInstance);
    }

    [JSFunction(Name = "addField")]
    public string AddField(SPFieldInstance field)
    {
      if (field == null)
        throw new JavaScriptException(this.Engine, "Error", "A field must be specified as the first argument.");

      return m_fieldCollection.Add(field.SPField);
    }

    [JSFunction(Name = "addNewField")]
    public string AddNewField(string displayName, string fieldType, bool required)
    {
      SPFieldType ft;
      if (fieldType.TryParseEnum(true, out ft) == false)
      {
        SPFieldInstance.ThrowUnknownFieldTypeException(this.Engine);
      }

      return m_fieldCollection.Add(displayName, ft, required);
    }

    [JSFunction(Name = "addNewFieldEx")]
    public string AddNewField(string displayName, string fieldType, bool required, bool compactName, ArrayInstance choices)
    {
      if (choices == null)
        throw new JavaScriptException(this.Engine, "Error", "The choices argument must be specified.");

      SPFieldType ft;
      if (fieldType.TryParseEnum(true, out ft) == false)
      {
        SPFieldInstance.ThrowUnknownFieldTypeException(this.Engine);
      }

      var strChoices = new StringCollection();
      foreach (var c in choices.ElementValues)
      {
        strChoices.Add(TypeConverter.ToString(c));
      }

      return m_fieldCollection.Add(displayName, ft, required, compactName, strChoices);
    }

    [JSFunction(Name = "addFieldAsXml")]
    public string AddFieldAsXml(string xml)
    {
      return m_fieldCollection.AddFieldAsXml(xml);
    }

    [JSFunction(Name = "addLookup")]
    public string AddLookup(string displayName, object lookupListId, object lookupWebId, object isRequired)
    {
      var lookupListIdInstance = GuidInstance.ConvertFromJsObjectToGuid(lookupListId);

      if (lookupWebId is bool)
        return m_fieldCollection.AddLookup(displayName, lookupListIdInstance, TypeConverter.ToBoolean(isRequired));

      var lookupWebIdInstance = GuidInstance.ConvertFromJsObjectToGuid(lookupWebId);
      return m_fieldCollection.AddLookup(displayName, lookupListIdInstance, lookupWebIdInstance,
        TypeConverter.ToBoolean(isRequired));
    }

    [JSFunction(Name = "containsField")]
    public bool ContainsField(string name)
    {
      return m_fieldCollection.ContainsField(name);
    }

    [JSFunction(Name = "containsFieldWithStaticName")]
    public bool ContainsFieldWithStaticName(string staticName)
    {
      return m_fieldCollection.ContainsFieldWithStaticName(staticName);
    }

    [JSFunction(Name = "createNewField")]
    public SPFieldInstance CreateNewField(string typeName, string displayName)
    {
      return new SPFieldInstance(this.Engine.Object.InstancePrototype, m_fieldCollection.CreateNewField(typeName, displayName));
    }

    [JSFunction(Name = "delete")]
    public void Delete(string name)
    {
      m_fieldCollection.Delete(name);
    }

    [JSFunction(Name = "getAllFields")]
    public ArrayInstance GetAllFields()
    {
      var fields = m_fieldCollection
        .OfType<SPField>()
        .Select(f => new SPFieldInstance(this.Engine.Object.InstancePrototype, f))
        .ToArray();

// ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(fields);
// ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "getField")]
    public SPFieldInstance GetField(string name)
    {
      var field = m_fieldCollection.GetField(name);
      if (field == null)
        return null;

      return new SPFieldInstance(this.Engine.Object.InstancePrototype, field);
    }

    [JSFunction(Name = "getFieldByGuid")]
    public SPFieldInstance GetFieldByGuid(object guid)
    {
      var guidInstance = GuidInstance.ConvertFromJsObjectToGuid(guid);

      var field = m_fieldCollection[guidInstance];
      if (field == null)
        return null;

      return new SPFieldInstance(this.Engine.Object.InstancePrototype, field);
    }

    [JSFunction(Name = "getFieldByIndex")]
    public SPFieldInstance GetFieldByIndex(int index)
    {
      var field = m_fieldCollection[index];
      return field == null
        ? null
        : new SPFieldInstance(this.Engine.Object.InstancePrototype, field);
    }

    [JSFunction(Name = "getFieldByDisplayName")]
    public SPFieldInstance GetFieldByGuid(string displayName)
    {
      var field = m_fieldCollection[displayName];
      return field == null
        ? null
        : new SPFieldInstance(this.Engine.Object.InstancePrototype, field);
    }

    [JSFunction(Name = "getFieldByInternalName")]
    public SPFieldInstance GetFieldByInternalName(string name)
    {
      var field = m_fieldCollection.GetFieldByInternalName(name);
      return field == null
        ? null
        : new SPFieldInstance(this.Engine.Object.InstancePrototype, null);
    }

    [JSFunction(Name = "getList")]
    public SPListInstance GetList()
    {
      return new SPListInstance(this.Engine, m_fieldCollection.List);
    }

    [JSFunction(Name = "tryGetFieldByStaticName")]
    public object TryGetFieldByStaticName(string staticName)
    {
      var field = m_fieldCollection.TryGetFieldByStaticName(staticName);
      if (field == null)
        return Null.Value;

      return new SPFieldInstance(this.Engine.Object.InstancePrototype, field);
    }
    #endregion
  }
}
