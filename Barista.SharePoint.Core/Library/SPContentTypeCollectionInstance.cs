namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPContentTypeCollectionConstructor : ClrFunction
  {
    public SPContentTypeCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPContentTypeCollection", new SPContentTypeCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPContentTypeCollectionInstance Construct()
    {
      return new SPContentTypeCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPContentTypeCollectionInstance : ObjectInstance
  {
    private readonly SPContentTypeCollection m_contentTypeCollection;

    public SPContentTypeCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPContentTypeCollectionInstance(ObjectInstance prototype, SPContentTypeCollection contentTypeCollection)
      : this(prototype)
    {
      if (contentTypeCollection == null)
        throw new ArgumentNullException("contentTypeCollection");

      m_contentTypeCollection = contentTypeCollection;
    }

    public SPContentTypeCollection SPContentTypeCollection
    {
      get { return m_contentTypeCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_contentTypeCollection.Count;
      }
    }

    [JSFunction(Name = "add")]
    public SPContentTypeInstance Add(SPContentTypeInstance contentType)
    {
      if (contentType == null)
        throw new JavaScriptException(this.Engine, "Error", "When adding a content type to a content type collection, the content type must be provided as the first argument.");

      var addedContentType = m_contentTypeCollection.Add(contentType.ContentType);
      return addedContentType == null
        ? null
        : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, addedContentType);
    }


    [JSFunction(Name = "bestMatch")]
    public SPContentTypeIdInstance BestMatch(object contentTypeId)
    {
      if (contentTypeId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Content Type Id must be provided as the first argument.");

      SPContentTypeId spContentTypeId;
      if (contentTypeId is SPContentTypeIdInstance)
        spContentTypeId = (contentTypeId as SPContentTypeIdInstance).ContentTypeId;
      else
        spContentTypeId = new SPContentTypeId(TypeConverter.ToString(contentTypeId));

      var result = m_contentTypeCollection.BestMatch(spContentTypeId);
      return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "delete")]
    public void Delete(object contentTypeId)
    {
      if (contentTypeId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Content Type Id must be provided as the first argument.");

      SPContentTypeId spContentTypeId;
      if (contentTypeId is SPContentTypeIdInstance)
        spContentTypeId = (contentTypeId as SPContentTypeIdInstance).ContentTypeId;
      else
        spContentTypeId = new SPContentTypeId(TypeConverter.ToString(contentTypeId));

      m_contentTypeCollection.Delete(spContentTypeId);
    }

    [JSFunction(Name = "getContentTypeByName")]
    public SPContentTypeInstance GetContentTypeByName(string name)
    {
      var result = m_contentTypeCollection[name];
      return result == null
        ? null
        : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getContentTypeById")]
    public SPContentTypeInstance GetContentTypeById(object contentTypeId)
    {
      if (contentTypeId == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A Content Type Id must be provided as the first argument.");

      SPContentTypeId spContentTypeId;
      if (contentTypeId is SPContentTypeIdInstance)
        spContentTypeId = (contentTypeId as SPContentTypeIdInstance).ContentTypeId;
      else if (contentTypeId is GuidInstance)
        spContentTypeId = new SPContentTypeId((contentTypeId as GuidInstance).Value.ToString());
      else
        spContentTypeId = new SPContentTypeId(TypeConverter.ToString(contentTypeId));

      var result = m_contentTypeCollection[spContentTypeId];
      return result == null
        ? null
        : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "getContentTypeByIndex")]
    public SPContentTypeInstance GetContentTypeByIndex(int index)
    {
      var result = m_contentTypeCollection[index];
      return result == null
        ? null
        : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var contentType in m_contentTypeCollection.OfType<SPContentType>())
      {
        ArrayInstance.Push(result, new SPContentTypeInstance(this.Engine.Object.InstancePrototype, contentType));
      }
      return result;
    }
  }
}
