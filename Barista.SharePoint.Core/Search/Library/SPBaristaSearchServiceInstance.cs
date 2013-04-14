namespace Barista.SharePoint.Search.Library
{
  using System.Collections.Generic;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System.Linq;
  using System;
  using Barista.Newtonsoft.Json;
  using Barista.SharePoint.SPBaristaSearchService;
  using Barista.Newtonsoft.Json.Linq;

  [Serializable]
  public class SPBaristaSearchServiceConstructor : ClrFunction
  {
    public SPBaristaSearchServiceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPBaristaSearchService", new SPBaristaSearchServiceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPBaristaSearchServiceInstance Construct()
    {
      return new SPBaristaSearchServiceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPBaristaSearchServiceInstance : ObjectInstance
  {
    private readonly SPBaristaSearchServiceProxy m_baristaSearchServiceProxy;

    public SPBaristaSearchServiceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPBaristaSearchServiceInstance(ObjectInstance prototype, SPBaristaSearchServiceProxy baristaSearchServiceProxy)
      : this(prototype)
    {
      if (baristaSearchServiceProxy == null)
        throw new ArgumentNullException("baristaSearchServiceProxy");

      m_baristaSearchServiceProxy = baristaSearchServiceProxy;
    }

    public SPBaristaSearchServiceProxy SPBaristaSearchServiceProxy
    {
      get { return m_baristaSearchServiceProxy; }
    }

    [JSProperty(Name = "indexName")]
    public string IndexName
    {
      get;
      set;
    }

    [JSFunction(Name = "deleteAllDocuments")]
    public void DeleteAllDocuments()
    {
      m_baristaSearchServiceProxy.DeleteAllDocuments(this.IndexName);
    }

    [JSFunction(Name = "deleteDocuments")]
    public void DeleteDocuments(ArrayInstance documentIds)
    {
      var documentIdValues = new List<string>();
      foreach (var documentId in documentIds.ElementValues)
      {
        var documentIdValue = TypeConverter.ConvertTo<string>(this.Engine, documentId);
        if (documentIdValue.IsNullOrWhiteSpace() == false && documentIdValue != "undefined")
          documentIdValues.Add(documentIdValue);
      }

      m_baristaSearchServiceProxy.DeleteDocuments(this.IndexName, documentIdValues);
    }

    [JSFunction(Name = "index")]
    public void Index(object documentObject)
    {
      //TODO: Recognize DocumentInstance, recognize StringInstance, recognize SPListItemInstance.
      //And convert/create a JsonDocumentInstance appropriately.

      JsonDocument documentToIndex = null;
      if (documentObject is JsonDocumentInstance)
      {
        documentToIndex = (documentObject as JsonDocumentInstance).JsonDocument;
      }
      else if (documentObject is ObjectInstance)
      {
        var obj = documentObject as ObjectInstance;
        if (obj.HasProperty("@id") == false)
          throw new JavaScriptException(this.Engine, "Error",
                                        "When adding a POJO to the index, a property named @id must be specified on the object that indicates the document id.");

        string metadata = String.Empty;
        if (obj.HasProperty("@metadata"))
          metadata = JSONObject.Stringify(this.Engine, obj.GetPropertyValue("@metadata"), null, null);

        //Clone the object and remove the @id and @metadata
        var json = JSONObject.Stringify(this.Engine, obj, null, null);
        var jObject = JObject.Parse(json);
        jObject.Remove("@id");
        jObject.Remove("@metadata");

        documentToIndex = new JsonDocument
          {
            DocumentId = obj.GetPropertyValue("@id").ToString(),
            MetadataAsJson = metadata,
            DataAsJson = jObject.ToString(Formatting.None)
          };
      }

      m_baristaSearchServiceProxy.IndexJsonDocument(this.IndexName, documentToIndex);
    }

    [JSFunction(Name = "retrieve")]
    public JsonDocumentInstance Retrieve(string documentId)
    {
      var result = m_baristaSearchServiceProxy.Retrieve(this.IndexName, documentId);
      return new JsonDocumentInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(string defaultField, string query, object maxResults)
    {
      var maxResultsValue = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, 1000);

      var searchResults = m_baristaSearchServiceProxy.Search(this.IndexName, defaultField, query, maxResultsValue);

// ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
// ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "searchOData")]
    public ArrayInstance SearchOData(string defaultField, string queryString)
    {
      var searchResults = m_baristaSearchServiceProxy.SearchOData(this.IndexName, defaultField, queryString);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }
  }
}
