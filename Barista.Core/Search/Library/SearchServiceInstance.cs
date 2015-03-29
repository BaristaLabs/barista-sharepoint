namespace Barista.Search.Library
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System.Linq;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.Search;
    using Barista.Newtonsoft.Json.Linq;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.QueryParsers;
    using Query = Barista.Search.Query;
    using Term = Barista.Search.Term;

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
        private static readonly Regex FieldIndexDeclarative = new Regex(@"^@@(?<FieldName>.*?)\.(?<Index>Index)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FieldStoreDeclarative = new Regex(@"^@@(?<FieldName>.*?)\.(?<Store>Store)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FieldTermVectorDeclarative = new Regex(@"^@@(?<FieldName>.*?)\.(?<TermVector>TermVector)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            this.DefaultMaxResults = 10000;
        }

        public IBaristaSearch BaristaSearch
        {
            get { return m_baristaSearchServiceProxy; }
        }

        /// <summary>
        /// Gets or sets the name of the index the current instance is associated with.
        /// </summary>
        [JSProperty(Name = "indexName")]
        [JSDoc("Gets or sets the name of index.")]
        public string IndexName
        {
            get;
            set;
        }

        [JSProperty(Name = "defaultMaxResults")]
        [JSDoc("Gets or sets the maximum number of results to return. Initial value is 10000.")]
        public int DefaultMaxResults
        {
            get;
            set;
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
            return new TermRangeQueryInstance(this.Engine.Object.InstancePrototype, new TermRangeQuery
            {
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
        public NumericRangeQueryInstance<double> CreateDoubleRangeQuery(string fieldName, object min, object max, bool minInclusive, bool maxInclusive)
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

        [JSFunction(Name = "createMultiFieldQueryParserQuery")]
        public MultiFieldQueryParserQueryInstance CreateMultiFieldQuery(ArrayInstance fieldNames, string query)
        {
            if (fieldNames == null)
                throw new JavaScriptException(this.Engine, "Error", "The first parameter must be an array of field names.");

            var qPq = new MultiFieldQueryParserQuery
            {
                FieldNames = fieldNames.ElementValues.OfType<string>().ToList(),
                Query = query
            };

            return new MultiFieldQueryParserQueryInstance(this.Engine.Object.InstancePrototype, qPq);
        }

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

        [JSFunction(Name = "createPrefixFilter")]
        public PrefixFilterInstance CreatePrefixFilter(string fieldName, string text)
        {
            var filter = new PrefixFilter { Term = new Term { FieldName = fieldName, Value = text } };
            return new PrefixFilterInstance(this.Engine.Object.InstancePrototype, filter);
        }

        [JSFunction(Name = "createTermsFilter")]
        public TermsFilterInstance CreateTermsFilter(object fieldName, object text)
        {
            var fieldNameValue = JurassicHelper.GetTypedArgumentValue(this.Engine, fieldName, String.Empty);
            var textValue = JurassicHelper.GetTypedArgumentValue(this.Engine, text, String.Empty);

            var termsFilter = new TermsFilter();
            if (fieldNameValue.IsNullOrWhiteSpace() == false && textValue.IsNullOrWhiteSpace() == false)
                termsFilter.Terms.Add(new Term { FieldName = fieldNameValue, Value = textValue });

            return new TermsFilterInstance(this.Engine.Object.InstancePrototype, termsFilter);
        }

        [JSFunction(Name = "createQueryWrapperFilter")]
        public QueryWrapperFilterInstance CreateQueryWrapperFilter(object query)
        {
            var filter = new QueryWrapperFilter();

            if (TypeUtilities.IsString(query))
            {
                filter.Query = new QueryParserQuery
                {
                    Query = TypeConverter.ToString(query)
                };
            }
            else
            {
                var queryType = query.GetType();
                var queryProperty = queryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
                if (queryProperty == null || typeof(Query).IsAssignableFrom(queryProperty.PropertyType) == false)
                    throw new JavaScriptException(this.Engine, "Error", "Unsupported query object.");
            }

            return new QueryWrapperFilterInstance(this.Engine.Object.InstancePrototype, filter);
        }

        [JSFunction(Name = "createSort")]
        public SortInstance CreateSort()
        {
            var sort = new Sort();
            return new SortInstance(this.Engine.Object.InstancePrototype, sort);
        }

        //TODO: More filter types...
        #endregion

        [JSFunction(Name = "deleteAllDocuments")]
        public void DeleteAllDocuments()
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            m_baristaSearchServiceProxy.DeleteAllDocuments(this.IndexName);
        }

        [JSFunction(Name = "deleteDocuments")]
        public void DeleteDocuments(object documentIds)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            IEnumerable<string> documentIdValues;
            if (documentIds is ArrayInstance)
            {
                var arrDocumentIds = documentIds as ArrayInstance;
                documentIdValues = arrDocumentIds.ElementValues
                  .Select(documentId => TypeConverter.ConvertTo<string>(this.Engine, documentId))
                  .Where(
                    documentIdValue =>
                      documentIdValue.IsNullOrWhiteSpace() == false &&
                      documentIdValue != "undefined")
                  .ToList();
            }
            else
            {
                documentIdValues = new List<string> { TypeConverter.ToString(documentIds) };
            }

            m_baristaSearchServiceProxy.DeleteDocuments(this.IndexName, documentIdValues);
        }

        [JSFunction(Name = "doesIndexExist")]
        public bool DoesIndexExist()
        {
            return m_baristaSearchServiceProxy.DoesIndexExist(this.IndexName);
        }

        [JSFunction(Name = "explain")]
        public ExplanationInstance Explain(object query, object docId)
        {
            if (query == null || query == Null.Value || query == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error", "A query object must be specified as the first parameter.");

            if (docId == null || docId == Null.Value || docId == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error",
                                              "A search result or document id must be specified as the second parameter.");

            var searchQueryType = query.GetType();
            var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
            if (queryProperty == null || typeof(Query).IsAssignableFrom(queryProperty.PropertyType) == false)
                throw new JavaScriptException(this.Engine, "Error", "Unsupported query object.");

            var queryValue = queryProperty.GetValue(query, null) as Query;

            //TODO: Change doc ID to also accept searchResults.
            var docIdValue = TypeConverter.ToInteger(docId);

            var explanation = m_baristaSearchServiceProxy.Explain(this.IndexName, queryValue, docIdValue);

            return new ExplanationInstance(this.Engine.Object.InstancePrototype, explanation);
        }

        [JSFunction(Name = "getFieldNames")]
        public ArrayInstance GetFieldNames()
        {
            var fieldNames = m_baristaSearchServiceProxy.GetFieldNames(this.IndexName);
            // ReSharper disable CoVariantArrayConversion
            return this.Engine.Array.Construct(fieldNames.ToArray());
            // ReSharper enable CoVariantArrayConversion
        }

        [JSFunction(Name = "highlight")]
        public string Highlight(object query, object docId, object fieldName, object fragCharSize)
        {
            if (query == null || query == Null.Value || query == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error", "A query object must be specified as the first argument.");

            if (docId == null || docId == Null.Value || docId == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error",
                                              "A search result or document id must be specified as the second argument.");

            if (fieldName == null || fieldName == Null.Value || fieldName == Undefined.Value || TypeConverter.ToString(fieldName).IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error",
                                              "A field name must be specified as the third argument.");

            var searchQueryType = query.GetType();
            var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
            if (queryProperty == null || typeof(Query).IsAssignableFrom(queryProperty.PropertyType) == false)
                throw new JavaScriptException(this.Engine, "Error", "Unsupported query object.");

            var queryValue = queryProperty.GetValue(query, null) as Query;

            //TODO: Change doc ID to also accept searchResults.
            var docIdValue = TypeConverter.ToInteger(docId);

            var fragCharSizeValue = 100;
            if (fragCharSize != null && fragCharSize != Null.Value && fragCharSize != Undefined.Value)
                fragCharSizeValue = TypeConverter.ToInteger(fragCharSize);

            return m_baristaSearchServiceProxy.Highlight(this.IndexName, queryValue, docIdValue, TypeConverter.ToString(fieldName), fragCharSizeValue);
        }

        [JSFunction(Name = "index")]
        public SearchServiceInstance Index(object documentObject)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            if (documentObject == null || documentObject == Null.Value || documentObject == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error",
                  "A document object to be indexed must be supplied as the first parameter.");

            if (documentObject is ArrayInstance)
            {
                var documentObjects = documentObject as ArrayInstance;
                var documentsToIndex = documentObjects.ElementValues
                  .Select(ConvertObjectToJsonDocumentDto)
                  .ToList();

                m_baristaSearchServiceProxy.IndexJsonDocuments(this.IndexName, documentsToIndex);
            }
            else
            {
                var documentToIndex = ConvertObjectToJsonDocumentDto(documentObject);
                m_baristaSearchServiceProxy.IndexJsonDocument(this.IndexName, documentToIndex);
            }

            return this;
        }

        [JSFunction(Name = "retrieve")]
        public JsonDocumentInstance Retrieve(string documentId)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            var result = m_baristaSearchServiceProxy.Retrieve(this.IndexName, documentId);
            return result == null
              ? null
              : new JsonDocumentInstance(this.Engine, result);
        }

        [JSFunction(Name = "search")]
        public ArrayInstance Search(object query, object maxResults)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            var args = CoerceSearchArguments(query, maxResults, null);

            var searchResults = m_baristaSearchServiceProxy.Search(this.IndexName, args);

            // ReSharper disable CoVariantArrayConversion
            return this.Engine.Array.Construct(searchResults.Select(sr => new SearchResultInstance(this.Engine, sr)).ToArray());
            // ReSharper restore CoVariantArrayConversion
        }

        [JSFunction(Name = "searchResultCount")]
        public int SearchResultCount(object query, object maxResults)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            var args = CoerceSearchArguments(query, maxResults, null);

            var searchResultCount = m_baristaSearchServiceProxy.SearchResultCount(this.IndexName, args);

            return searchResultCount;
        }

        [JSFunction(Name = "facetedSearch")]
        public ArrayInstance FacetedSearch(object query, object maxResults, object groupByFields)
        {
            if (this.IndexName.IsNullOrWhiteSpace())
                throw new JavaScriptException(this.Engine, "Error", "indexName not set. Please set the indexName property on the Search Instance prior to performing an operation.");

            var args = CoerceSearchArguments(query, maxResults, groupByFields);

            var searchResults = m_baristaSearchServiceProxy.FacetedSearch(this.IndexName, args);

            // ReSharper disable CoVariantArrayConversion
            return this.Engine.Array.Construct(searchResults.Select(sr => new FacetedSearchResultInstance(this.Engine.Object.InstancePrototype, sr)).ToArray());
            // ReSharper restore CoVariantArrayConversion
        }

        [JSFunction(Name = "setFieldOptions")]
        public SearchServiceInstance SetFieldOptions(object fieldOptions)
        {
            if (fieldOptions == null || fieldOptions == Null.Value || fieldOptions == Undefined.Value)
                throw new JavaScriptException(this.Engine, "Error",
                  "The first argument must be an object, or an array of objects, that defines the field options for the index.");

            var args = new List<FieldOptions>();
            if (fieldOptions is ArrayInstance)
            {
                var fieldOptionsArray = fieldOptions as ArrayInstance;
                args.AddRange(fieldOptionsArray.ElementValues.OfType<ObjectInstance>().Select(CoerceFieldOptions));
            }
            else if (fieldOptions is ObjectInstance)
            {
                args.Add(CoerceFieldOptions(fieldOptions as ObjectInstance));
            }

            m_baristaSearchServiceProxy.SetFieldOptions(this.IndexName, args);

            return this;
        }

        [JSFunction(Name = "shutdown")]
        public SearchServiceInstance Shutdown()
        {
            m_baristaSearchServiceProxy.Shutdown(this.IndexName);
            return this;
        }

        private FieldOptions CoerceFieldOptions(ObjectInstance fieldOptions)
        {
            if (fieldOptions == null)
                throw new JavaScriptException(this.Engine, "Error", "Cannot convert a null field options object.");

            var result = new FieldOptions();

            if (fieldOptions.HasProperty("fieldName"))
                result.FieldName = TypeConverter.ToString(fieldOptions.GetPropertyValue("fieldName"));
            else
                throw new JavaScriptException(this.Engine, "Error", "All field options objects must contain a field name.");

            if (fieldOptions.HasProperty("index"))
            {
                var propertyValue = fieldOptions.GetPropertyValue("index");
                if (propertyValue != null && propertyValue != Null.Value && propertyValue != Undefined.Value)
                {
                    FieldIndexType fieldIndexType;
                    if (TypeConverter.ToString(propertyValue).TryParseEnum(true, out fieldIndexType) ==
                        false)
                        throw new JavaScriptException(this.Engine, "Error",
                          "If the index property is defined on the field options, it must be one of these values: Analyzed, AnalyzedNoNorms, NotAnalyzed, NotAnalyzedNoNorms, NotIndexed");

                    result.Index = fieldIndexType;
                }
            }

            if (fieldOptions.HasProperty("storage"))
            {
                var propertyValue = fieldOptions.GetPropertyValue("storage");
                if (propertyValue != null && propertyValue != Null.Value && propertyValue != Undefined.Value)
                {
                    FieldStorageType fieldStorageType;
                    if (TypeConverter.ToString(propertyValue).TryParseEnum(true, out fieldStorageType) ==
                        false)
                        throw new JavaScriptException(this.Engine, "Error",
                          "If the storage property is defined on the field options, it must be one of these values: Stored, NotStored");

                    result.Storage = fieldStorageType;
                }
            }

            if (fieldOptions.HasProperty("termVectorType"))
            {
                var propertyValue = fieldOptions.GetPropertyValue("termVectorType");
                if (propertyValue != null && propertyValue != Null.Value && propertyValue != Undefined.Value)
                {
                    FieldTermVectorType fieldTermVectorType;
                    if (TypeConverter.ToString(propertyValue).TryParseEnum(true, out fieldTermVectorType) ==
                        false)
                        throw new JavaScriptException(this.Engine, "Error",
                          "If the termVectorType property is defined on the field options, it must be one of these values: No, WithOffsets, WithPositions, WithPositionsOffsets, Yes");

                    result.TermVectorType = fieldTermVectorType;
                }
            }

            return result;
        }

        private SearchArguments CoerceSearchArguments(object query, object maxResults, object groupByFields)
        {
            var args = new Barista.Search.SearchArguments();

            if (query == null || query == Null.Value || query == Undefined.Value)
            {
                args.Query = new MatchAllDocsQuery();
                if (maxResults != Undefined.Value && maxResults != Null.Value && maxResults != null)
                    args.Take = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, this.DefaultMaxResults);
            }
            else if (TypeUtilities.IsString(query))
            {
                args.Query = new QueryParserQuery
                {
                    Query = TypeConverter.ToString(query)
                };

                if (maxResults != Undefined.Value && maxResults != Null.Value && maxResults != null)
                    args.Take = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, this.DefaultMaxResults);

                if (groupByFields != null && groupByFields != Undefined.Value && groupByFields != Null.Value && groupByFields is ArrayInstance)
                    args.GroupByFields = (groupByFields as ArrayInstance).ElementValues.Select(TypeConverter.ToString).ToList();
            }
            else if (query is SearchArgumentsInstance)
            {
                var searchArgumentsInstance = query as SearchArgumentsInstance;
                args = searchArgumentsInstance.GetSearchArguments();
            }
            else if (query.GetType().IsAssignableFrom(typeof(IQuery<>)))
            {
                args = new SearchArguments();

                var pi = typeof(IQuery<>).GetProperty("Query");
                args.Query = (Query)pi.GetValue(query, null);
            }
            else if (query is ObjectInstance)
            {
                var argumentsObj = query as ObjectInstance;

                args = new SearchArguments();

                //Duck Type for the win
                if (argumentsObj.HasProperty("query"))
                {
                    var queryObj = argumentsObj["query"];
                    var queryObjType = queryObj.GetType();

                    var queryProperty = queryObjType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
                    if (queryProperty != null && typeof(Query).IsAssignableFrom(queryProperty.PropertyType))
                        args.Query = queryProperty.GetValue(queryObj, null) as Query;
                }
                else
                {
                    var queryObjType = query.GetType();

                    var queryProperty = queryObjType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
                    if (queryProperty != null && typeof(Query).IsAssignableFrom(queryProperty.PropertyType))
                        args.Query = queryProperty.GetValue(query, null) as Query;

                    if (maxResults != Undefined.Value && maxResults != Null.Value && maxResults != null)
                        args.Take = JurassicHelper.GetTypedArgumentValue(this.Engine, maxResults, this.DefaultMaxResults);
                }

                if (argumentsObj.HasProperty("filter"))
                {
                    var filterObj = argumentsObj["filter"];
                    var filterObjType = filterObj.GetType();

                    var filterProperty = filterObjType.GetProperty("Filter", BindingFlags.Instance | BindingFlags.Public);
                    if (filterProperty != null && typeof(Filter).IsAssignableFrom(filterProperty.PropertyType))
                        args.Filter = filterProperty.GetValue(filterObj, null) as Filter;
                }

                if (argumentsObj.HasProperty("groupByFields"))
                {
                    var groupByFieldsValue = argumentsObj["groupByFields"] as ArrayInstance;
                    if (groupByFieldsValue != null)
                    {
                        args.GroupByFields = groupByFieldsValue.ElementValues.Select(TypeConverter.ToString).ToList();
                    }
                }

                if (argumentsObj.HasProperty("sort") && argumentsObj["sort"] is SortInstance)
                {
                    var sortValue = argumentsObj["sort"] as SortInstance;
                    args.Sort = sortValue.Sort;
                }

                if (argumentsObj.HasProperty("skip"))
                {
                    var skipObj = argumentsObj["skip"];
                    args.Skip = TypeConverter.ToInteger(skipObj);
                }

                if (argumentsObj.HasProperty("take"))
                {
                    var takeObj = argumentsObj["take"];
                    args.Take = TypeConverter.ToInteger(takeObj);
                }
            }
            else
            {
                throw new JavaScriptException(this.Engine, "Error", "Unable to determine the search arguments.");
            }

            return args;
        }

        protected virtual JsonDocumentDto ConvertObjectToJsonDocumentDto(object documentObject)
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

                var metadata = String.Empty;
                if (obj.HasProperty("@metadata"))
                    metadata = JSONObject.Stringify(this.Engine, obj.GetPropertyValue("@metadata"), null, null);

                //Clone the object and remove the @id and @metadata
                var json = JSONObject.Stringify(this.Engine, obj, null, null);
                var jObject = JObject.Parse(json);
                jObject.Remove("@id");
                jObject.Remove("@metadata");

                //Obtain any field options, add them to the field options collection and remove them from the cloned object.
                var fieldOptions = new Dictionary<string, FieldOptions>();
                foreach (var property in jObject.Properties().ToList())
                {
                    var fieldOption = new FieldOptions();

                    var foundMatch = false;

                    if (FieldIndexDeclarative.IsMatch(property.Name))
                    {
                        fieldOption.FieldName = FieldIndexDeclarative.Match(property.Name).Groups["FieldName"].Value;
                        FieldIndexType indexType;
                        if (property.Value.ToString().TryParseEnum(true, FieldIndexType.Analyzed, out indexType))
                        {
                            fieldOption.Index = indexType;
                            foundMatch = true;
                        }
                    }
                    else if (FieldStoreDeclarative.IsMatch(property.Name))
                    {
                        fieldOption.FieldName = FieldStoreDeclarative.Match(property.Name).Groups["FieldName"].Value;
                        FieldStorageType storageType;
                        if (property.Value.ToString().TryParseEnum(true, FieldStorageType.Stored, out storageType))
                        {
                            fieldOption.Storage = storageType;
                            foundMatch = true;
                        }
                    }
                    else if (FieldTermVectorDeclarative.IsMatch(property.Name))
                    {
                        fieldOption.FieldName = FieldTermVectorDeclarative.Match(property.Name).Groups["FieldName"].Value;
                        FieldTermVectorType termVectorType;
                        if (property.Value.ToString().TryParseEnum(true, FieldTermVectorType.Yes, out termVectorType))
                        {
                            fieldOption.TermVectorType = termVectorType;
                            foundMatch = true;
                        }
                    }

                    if (foundMatch)
                    {
                        if (fieldOptions.ContainsKey(fieldOption.FieldName))
                        {
                            var efo = fieldOptions[fieldOption.FieldName];
                            efo.Index = fieldOption.Index ?? efo.Index;
                            efo.Storage = fieldOption.Storage ?? efo.Storage;
                            efo.TermVectorType = fieldOption.TermVectorType ?? efo.TermVectorType;
                        }
                        else
                        {
                            fieldOptions.Add(fieldOption.FieldName, fieldOption);
                        }

                        jObject.Remove(property.Name);
                    }
                }

                documentToIndex = new JsonDocumentDto
                {
                    DocumentId = obj.GetPropertyValue("@id").ToString(),
                    FieldOptions = fieldOptions.Values,
                    MetadataAsJson = metadata,
                    DataAsJson = jObject.ToString(Formatting.None)
                };
            }

            return documentToIndex;
        }

        //TODO: Think about re-implementing the following.
        /*
        [JSFunction(Name = "searchAfter")]
        public ArrayInstance SearchAfter(ScoreDocInstance scoreDoc, object searchQuery, object n, object lookAhead)
        {
          if (scoreDoc == null)
            throw new JavaScriptException(this.Engine, "Error", "A score doc must be specified to indicate the start of the search.");

          if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
            throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

          SearchArguments searchArguments = new SearchArguments();

          if (n != null && n != Null.Value && n != Undefined.Value && n is int)
            searchArguments.Take = (int)n;

          var intLookAhead = 1000;
          if (lookAhead != null && lookAhead != Null.Value && lookAhead != Undefined.Value && lookAhead is int)
            intLookAhead = (int)lookAhead;

          var searchQueryType = searchQuery.GetType();

          if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
          {
            var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
            searchArguments.Query = queryProperty.GetValue(searchQuery, null) as Query;
          }
          else if (searchQuery is string || searchQuery is StringInstance || searchQuery is ConcatenatedString)
          {
            var parser = new QueryParser(Version.LUCENE_30, "_contents", new StandardAnalyzer(Version.LUCENE_30));
            searchArguments.Query = parser.Parse(searchQuery.ToString());
          }
          else if (searchQuery is SearchArgumentsInstance)
          {
            var searchArgumentsInstance = searchQuery as SearchArgumentsInstance;
            searchArguments = SearchArguments.GetSearchArgumentsFromSearchArgumentsInstance(searchArgumentsInstance);
          }
          else if (searchQuery is ObjectInstance)
          {
            var searchArgumentsDuck = JurassicHelper.Coerce<SearchArgumentsInstance>(this.Engine, searchQuery);
            searchArguments = SearchArguments.GetSearchArgumentsFromSearchArgumentsInstance(searchArgumentsDuck);
          }
          else
            throw new JavaScriptException(this.Engine, "Error", "Could not determine query from arguments. The query argument must either be a query instance, a string, a search arguments instance or an object that can be converted to a search arguments instance.");


          //Since the current Lucene.Net implementation does not include searchAfter, perform similar capabilities.
          var hitInstances = m_indexSearcher.Search(searchArguments.Query, searchArguments.Filter, searchArguments.Take + intLookAhead, Sort.INDEXORDER).ScoreDocs
                                            .AsQueryable()
                                            .Where(s => s.Doc > scoreDoc.DocumentId)
                                            .Take(searchArguments.Take)
                                            .Select(s =>
                                                new ScoreDocInstance(this.Engine.Object.InstancePrototype, s, m_indexSearcher)
                                              )
                                            .OrderByDescending(d => d.Score)
                                            .ToArray();

          return this.Engine.Array.Construct(hitInstances);
        }*/
    }
}
