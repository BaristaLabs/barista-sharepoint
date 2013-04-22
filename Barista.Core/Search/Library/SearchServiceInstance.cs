namespace Barista.Search.Library
{
  using System.Reflection;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System.Linq;
  using System;
  using Barista.Newtonsoft.Json;
  using Barista.Search;
  using Barista.Newtonsoft.Json.Linq;

  [Serializable]
  public class SearchServiceConstructor : ClrFunction
  {
    public SearchServiceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchService", new SearchServiceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchServiceInstance Construct()
    {
      return new SearchServiceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchServiceInstance : ObjectInstance
  {
    private readonly IBaristaSearch m_baristaSearchServiceProxy;

    public SearchServiceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchServiceInstance(ObjectInstance prototype, IBaristaSearch baristaSearch)
      : this(prototype)
    {
      if (baristaSearch == null)
        throw new ArgumentNullException("baristaSearch");

      m_baristaSearchServiceProxy = baristaSearch;
    }

    public IBaristaSearch BaristaSearch
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

    [JSFunction(Name = "createTermRangeQuery")]
    public TermRangeQueryInstance CreateTermRangeQuery(string fieldName, string lowerTerm, string upperTerm, bool includeLower, bool includeUpper)
    {
      return new TermRangeQueryInstance(this.Engine.Object.InstancePrototype, new TermRangeQuery {
        FieldName = fieldName,
        LowerTerm = lowerTerm, 
        UpperTerm = upperTerm,
        LowerInclusive = includeLower,
        UpperInclusive = includeUpper
      });
    }

    [JSFunction(Name = "createPrefixQuery")]
    public PrefixQueryInstance CreatePrefixQuery(string fieldName, string text)
    {
      return new PrefixQueryInstance(this.Engine.Object.InstancePrototype, new PrefixQuery
        {
          Term = new Term
            {
              FieldName = fieldName,
              Value = text
            }
        });
    }

    [JSFunction(Name = "createIntRangeQuery")]
    public NumericRangeQueryInstance<int> CreateIntRangeQuery(string fieldName, object min, object max, bool minInclusive, bool maxInclusive)
    {
      int? intMin;
      if (min == null || min == Null.Value || min == Undefined.Value)
        intMin = null;
      else
        intMin = JurassicHelper.GetTypedArgumentValue(this.Engine, min, 0);

      int? intMax;
      if (max == null || max == Null.Value || max == Undefined.Value)
        intMax = null;
      else
        intMax = JurassicHelper.GetTypedArgumentValue(this.Engine, max, 0);

      var query = new IntNumericRangeQuery
        {
          FieldName = fieldName,
          Min = intMin,
          Max = intMax,
          MinInclusive = minInclusive,
          MaxInclusive = maxInclusive
        };

      return new NumericRangeQueryInstance<int>(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createDoubleRangeQuery")]
    public NumericRangeQueryInstance<double> CreateDoubleRangeQuery(string fieldName,object min, object max, bool minInclusive, bool maxInclusive)
    {
      double? doubleMin;
      if (min == null || min == Null.Value || min == Undefined.Value)
        doubleMin = null;
      else
        doubleMin = JurassicHelper.GetTypedArgumentValue(this.Engine, min, 0);

      float? doubleMax;
      if (max == null || max == Null.Value || max == Undefined.Value)
        doubleMax = null;
      else
        doubleMax = JurassicHelper.GetTypedArgumentValue(this.Engine, max, 0);

      var query = new DoubleNumericRangeQuery
      {
        FieldName = fieldName,
        Min = doubleMin,
        Max = doubleMax,
        MinInclusive = minInclusive,
        MaxInclusive = maxInclusive
      };

      return new NumericRangeQueryInstance<double>(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createFloatRangeQuery")]
    public NumericRangeQueryInstance<float> CreateFloatRangeQuery(string fieldName, object min, object max, bool minInclusive, bool maxInclusive)
    {
      float? floatMin;
      if (min == null || min == Null.Value || min == Undefined.Value)
        floatMin = null;
      else
        floatMin = JurassicHelper.GetTypedArgumentValue(this.Engine, min, 0);

      float? floatMax;
      if (max == null || max == Null.Value || max == Undefined.Value)
        floatMax = null;
      else
        floatMax = JurassicHelper.GetTypedArgumentValue(this.Engine, max, 0);

      var query = new FloatNumericRangeQuery
      {
        FieldName = fieldName,
        Min = floatMin,
        Max = floatMax,
        MinInclusive = minInclusive,
        MaxInclusive = maxInclusive
      };

      return new NumericRangeQueryInstance<float>(this.Engine.Object.InstancePrototype, query);
    }

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

    [JSFunction(Name = "createWildcardQuery")]
    public WildcardQueryInstance CreateWildcardQuery(string fieldName, string text)
    {
      var query = new WildcardQuery
        {
          Term = new Term
            {
              FieldName = fieldName,
              Value = text,
            }
        };

      return new WildcardQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createFuzzyQuery")]
    public FuzzyQueryInstance CreateFuzzyQuery(string fieldName, string text)
    {
      var query = new FuzzyQuery
        {
          Term = new Term
            {
              FieldName = fieldName,
              Value = text
            }
        };

      return new FuzzyQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createQueryParserQuery")]
    public QueryParserQueryInstance CreateQueryParserQuery(string query, string defaultField, bool allowLeadingWildcard)
    {
      var qPq = new QueryParserQuery
      {
        AllowLeadingWildcard = allowLeadingWildcard,
        DefaultField = defaultField,
        Query = query
      };

      return new QueryParserQueryInstance(this.Engine.Object.InstancePrototype, qPq);
    }

    [JSFunction(Name = "createODataQuery")]
    public ODataQueryInstance CreateODataQuery(string query, string defaultField, bool allowLeadingWildcard)
    {
      var oDataQuery = new ODataQuery
      {
        AllowLeadingWildcard = allowLeadingWildcard,
        DefaultField = defaultField,
        Query = query
      };

      return new ODataQueryInstance(this.Engine.Object.InstancePrototype, oDataQuery);
    }

    //[JSFunction(Name = "createMultiFieldQuery")]
    //public GenericQueryInstance CreateMultiFieldQuery(ArrayInstance fieldNames, string text)
    //{
    //  if (fieldNames == null)
    //    throw new JavaScriptException(this.Engine, "Error", "The first parameter must be an array of field names.");

    //  var parser = new MultiFieldQueryParser(Version.LUCENE_30, fieldNames.ElementValues.OfType<string>().ToArray(), new StandardAnalyzer(Version.LUCENE_30));
    //  var query = parser.Parse(text);
    //  return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    //}

    [JSFunction(Name = "createRegexQuery")]
    public RegexQueryInstance CreateRegexQuery(string fieldName, string text)
    {
      var query = new RegexQuery
        {
          Term = new Term
            {
              FieldName = fieldName,
              Value = text
            }
        };

      return new RegexQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createMatchAllDocsQuery")]
    public GenericQueryInstance CreateMatchAllDocsQuery()
    {
      var query = new MatchAllDocsQuery();
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }
    #endregion

    #region Filter Creation
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
      if (this.IndexName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");
      
      m_baristaSearchServiceProxy.DeleteAllDocuments(this.IndexName);
    }

    [JSFunction(Name = "deleteDocuments")]
    public void DeleteDocuments(ArrayInstance documentIds)
    {
      if (this.IndexName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

      var documentIdValues = documentIds.ElementValues
                                        .Select(documentId => TypeConverter.ConvertTo<string>(this.Engine, documentId))
                                        .Where(
                                          documentIdValue =>
                                          documentIdValue.IsNullOrWhiteSpace() == false &&
                                          documentIdValue != "undefined")
                                        .ToList();

      m_baristaSearchServiceProxy.DeleteDocuments(this.IndexName, documentIdValues);
    }

    [JSFunction(Name = "index")]
    public void Index(object documentObject)
    {
      if (this.IndexName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

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
      if (this.IndexName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

      var result = m_baristaSearchServiceProxy.Retrieve(this.IndexName, documentId);
      return new JsonDocumentInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object query, object maxResults)
    {
      if (this.IndexName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

      Query queryValue;
      if (query == null || query == Null.Value || query == Undefined.Value)
        queryValue = new MatchAllDocsQuery();
      else
      {
        var searchQueryType = query.GetType();
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        if (queryProperty == null || typeof(Query).IsAssignableFrom(queryProperty.PropertyType) == false)
          throw new JavaScriptException(this.Engine, "Error", "Unsupported query object.");

        queryValue = queryProperty.GetValue(query, null) as Query;
      }

      var maxResultsValue = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, 1000);

      var args = new Barista.Search.SearchArguments
        {
          Query = queryValue,
          Take = maxResultsValue
        };

      var searchResults = m_baristaSearchServiceProxy.Search(this.IndexName, args);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine.Object.Prototype, sr)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }
  }
}
