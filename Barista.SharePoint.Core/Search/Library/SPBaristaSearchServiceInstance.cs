namespace Barista.SharePoint.Search.Library
{
  using System.Collections.Generic;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System.Linq;
  using System;
  using Barista.Newtonsoft.Json;
  using Barista.Search;
  using Barista.Search.Library;
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


    #region Query Creation
    [JSFunction(Name = "createTermQuery")]
    public TermQueryInstance CreateTermQuery(string fieldName, string text)
    {
      return new TermQueryInstance(this.Engine.Object.InstancePrototype, new TermQuery
        {
          Term = new Term
            {
              FieldName = fieldName,
              Value = text
            }
        });
    }

    //[JSFunction(Name = "createTermRangeQuery")]
    //public TermRangeQueryInstance CreateTermRangeQuery(string fieldName, string lowerTerm, string upperTerm, bool includeLower, bool includeUpper)
    //{
    //  return new TermRangeQueryInstance(this.Engine.Object.InstancePrototype, new TermRangeQuery(fieldName, lowerTerm, upperTerm, includeLower, includeUpper));
    //}

    //[JSFunction(Name = "createPrefixQuery")]
    //public PrefixQueryInstance CreatePrefixQuery(string fieldName, string text)
    //{
    //  return new PrefixQueryInstance(this.Engine.Object.InstancePrototype, new PrefixQuery(new Term(fieldName, text)));
    //}

    //[JSFunction(Name = "createIntRangeQuery")]
    //public NumericRangeQueryInstance<int> CreateIntRangeQuery(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    //{
    //  int? intMin = null;
    //  if (min != null && min != Null.Value && min != Undefined.Value && min is int)
    //    intMin = Convert.ToInt32(min);

    //  int? intMax = null;
    //  if (max != null && max != Null.Value && max != Undefined.Value && max is int)
    //    intMax = Convert.ToInt32(max);

    //  var query = NumericRangeQuery.NewIntRange(fieldName, precisionStep, intMin, intMax, minInclusive, maxInclusive);

    //  return new NumericRangeQueryInstance<int>(this.Engine.Object.InstancePrototype, query);
    //}

    //[JSFunction(Name = "createDoubleRangeQuery")]
    //public NumericRangeQueryInstance<double> CreateDoubleRangeQuery(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    //{
    //  double? doubleMin = null;
    //  if (min != null && min != Null.Value && min != Undefined.Value && min is int)
    //    doubleMin = Convert.ToDouble(min);

    //  double? doubleMax = null;
    //  if (max != null && max != Null.Value && max != Undefined.Value && max is int)
    //    doubleMax = Convert.ToDouble(max);

    //  var query = NumericRangeQuery.NewDoubleRange(fieldName, precisionStep, doubleMin, doubleMax, minInclusive, maxInclusive);

    //  return new NumericRangeQueryInstance<double>(this.Engine.Object.InstancePrototype, query);
    //}

    [JSFunction(Name = "createBooleanQuery")]
    public BooleanQueryInstance CreateBooleanQuery()
    {
      var query = new BooleanQuery();
      return new BooleanQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createPhraseQuery")]
    public PhraseQueryInstance CreatePhraseQuery()
    {
      var query = new PhraseQuery();
      return new PhraseQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    //[JSFunction(Name = "createWildcardQuery")]
    //public WildcardQueryInstance CreateWildcardQuery(string fieldName, string text)
    //{
    //  var query = new WildcardQuery(new Term(fieldName, text));
    //  return new WildcardQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    //[JSFunction(Name = "createFuzzyQuery")]
    //public FuzzyQueryInstance CreateFuzzyQuery(string fieldName, string text)
    //{
    //  var query = new FuzzyQuery(new Term(fieldName, text));
    //  return new FuzzyQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    //[JSFunction(Name = "createQuery")]
    //public GenericQueryInstance CreateQuery(string fieldName, string text)
    //{
    //  var parser = new QueryParser(Version.LUCENE_30, fieldName, new StandardAnalyzer(Version.LUCENE_30));
    //  var query = parser.Parse(text);
    //  return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    //[JSFunction(Name = "createMultiFieldQuery")]
    //public GenericQueryInstance CreateMultiFieldQuery(ArrayInstance fieldNames, string text)
    //{
    //  if (fieldNames == null)
    //    throw new JavaScriptException(this.Engine, "Error", "The first parameter must be an array of field names.");

    //  var parser = new MultiFieldQueryParser(Version.LUCENE_30, fieldNames.ElementValues.OfType<string>().ToArray(), new StandardAnalyzer(Version.LUCENE_30));
    //  var query = parser.Parse(text);
    //  return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    //[JSFunction(Name = "createRegexQuery")]
    //public RegexQueryInstance CreateRegexQuery(string fieldName, string text)
    //{
    //  var query = new RegexQuery(new Term(fieldName, text));
    //  return new RegexQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    [JSFunction(Name = "createMatchAllDocsQuery")]
    public GenericQueryInstance CreateMatchAllDocsQuery()
    {
      var query = new MatchAllDocsQuery();
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }
    #endregion


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

      JsonDocumentDto documentToIndex = null;
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

        documentToIndex = new JsonDocumentDto
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

    [JSFunction(Name = "searchWithQuery")]
    public ArrayInstance SearchWithQuery(string defaultField, object query, object maxResults)
    {
      var queryValue = JurassicHelper.GetTypedArgumentValue(this.Engine, query, new MatchAllDocsQuery());

      var maxResultsValue = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, 1000);

      var searchResults = m_baristaSearchServiceProxy.SearchWithQuery(this.IndexName, queryValue, maxResultsValue);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "searchWithQueryParser")]
    public ArrayInstance SearchWithQueryParser(string defaultField, string query, object maxResults)
    {
      var maxResultsValue = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, 1000);

      var searchResults = m_baristaSearchServiceProxy.SearchWithQueryParser(this.IndexName, defaultField, query, maxResultsValue);

// ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
// ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "searchWithOData")]
    public ArrayInstance SearchWithOData(string defaultField, string queryString)
    {
      var searchResults = m_baristaSearchServiceProxy.SearchWithOData(this.IndexName, defaultField, queryString);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }
  }
}
