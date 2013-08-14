namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPFieldLinkCollectionConstructor : ClrFunction
  {
    public SPFieldLinkCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFieldLinkCollection", new SPFieldLinkCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPFieldLinkCollectionInstance Construct()
    {
      return new SPFieldLinkCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPFieldLinkCollectionInstance : ObjectInstance
  {
    private readonly SPFieldLinkCollection m_fieldLinkCollection;

    public SPFieldLinkCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFieldLinkCollectionInstance(ObjectInstance prototype, SPFieldLinkCollection fieldLinkCollection)
      : this(prototype)
    {
      if (fieldLinkCollection == null)
        throw new ArgumentNullException("fieldLinkCollection");

      m_fieldLinkCollection = fieldLinkCollection;
    }

    public SPFieldLinkCollection SPFieldLinkCollection
    {
      get { return m_fieldLinkCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get { return m_fieldLinkCollection.Count; }
    }

    [JSFunction(Name = "add")]
    public void Add(SPFieldLinkInstance fieldLink)
    {
      if (fieldLink == null)
        throw new JavaScriptException(this.Engine, "Error", "When adding a field link to a field link collection, the field link must be provided as the first argument.");

      m_fieldLinkCollection.Add(fieldLink.SPFieldLink);
    }

    [JSFunction(Name = "delete")]
    public void Delete(object fieldLink)
    {
      if (fieldLink == null || fieldLink == Undefined.Value || fieldLink == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "When deleting a field link from a field link collection, the field link id or name must be provided as the first argument.");

      if (fieldLink is GuidInstance)
      {
        var id = (fieldLink as GuidInstance).Value;
        m_fieldLinkCollection.Delete(id);
      }
      else
      {
        var name = TypeConverter.ToString(fieldLink);
        m_fieldLinkCollection.Delete(name);
      }
      
    }

    [JSFunction(Name = "reorder")]
    public void Reorder(ArrayInstance internalNames)
    {
      if (internalNames == null)
        throw new JavaScriptException(this.Engine, "Error", "When reordering the field links in a field link collection, an array of field link names must be provided as the first argument.");

      var internalNamesList = internalNames.ElementValues
        .Select(TypeConverter.ToString)
        .ToArray();

      m_fieldLinkCollection.Reorder(internalNamesList);
    }

    [JSFunction(Name = "getFieldLinkById")]
    public SPFieldLinkInstance GetFieldLinkById(object id)
    {
      if (id == null || id == Null.Value || id == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "An id must be specified.");

      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
      return new SPFieldLinkInstance(this.Engine.Object.InstancePrototype, m_fieldLinkCollection[guid]);
    }

    [JSFunction(Name = "getFieldLinkByName")]
    public SPFieldLinkInstance GetFieldLinkByName(string name)
    {
      if (name.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "The name of the field link to retrieve must be specified.");

      return new SPFieldLinkInstance(this.Engine.Object.InstancePrototype, m_fieldLinkCollection[name]);
    }

    [JSFunction(Name = "getFieldLinkByIndex")]
    public SPFieldLinkInstance GetFieldLinkByIndex(int index)
    {
      return new SPFieldLinkInstance(this.Engine.Object.InstancePrototype, m_fieldLinkCollection[index]);
    }

    [JSFunction(Name = "getAllFieldLinks")]
    public ArrayInstance GetAllFieldLinks()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentType in m_fieldLinkCollection.OfType<SPFieldLink>())
      {
        ArrayInstance.Push(result, new SPFieldLinkInstance(this.Engine.Object.InstancePrototype, contentType));
      }
      return result;
    }
  }
}
