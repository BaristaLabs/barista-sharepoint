namespace Barista.SharePoint.Search.Library
{
  using System.Collections.Generic;
  using Barista.Search.Library;
  using Barista.SharePoint.Search;
  using Contrib.Regex;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Index;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Microsoft.SharePoint;
  using System;
  using System.Linq;
  using Version = Lucene.Net.Util.Version;

  [Serializable]
  public class SPLuceneInstance : ObjectInstance
  {
    public SPLuceneInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "hasIndexWriter")]
    public bool HasIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      return LuceneHelper.HasIndexWriterSingleton(targetFolder);
    }

    [JSFunction(Name = "getIndexSearcher")]
    public IndexSearcherInstance GetIndexSearcher(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      var indexSearcher = LuceneHelper.GetIndexSearcher(targetFolder);

      return new IndexSearcherInstance(this.Engine.Object.InstancePrototype, indexSearcher);
    }

    [JSFunction(Name = "getIndexWriter")]
    public SPIndexWriterInstance GetIndexWriter(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var bCreateIndex = false;
      if (createIndex != null && createIndex != Null.Value && createIndex != Undefined.Value && createIndex is Boolean)
        bCreateIndex = (bool)createIndex;

      var indexWriter = LuceneHelper.GetIndexWriterSingleton(targetFolder, bCreateIndex);
      return new SPIndexWriterInstance(this.Engine.Object.InstancePrototype, indexWriter, targetFolder);
    }

    [JSFunction(Name = "getSimpleFacetedSearch")]
    public SimpleFacetedSearchInstance GetSimpleFacetedSearch(object folder, object groupByFields)
    {
      if (groupByFields == null || groupByFields == Null.Value || groupByFields == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "Must specify the field(s) to group by as the second parameter.");

      var groupByFieldsList = new List<string>();
      if (groupByFields is ArrayInstance)
      {
        groupByFieldsList.AddRange((groupByFields as ArrayInstance).ElementValues.OfType<string>());
      }
      else
      {
        groupByFieldsList.Add(groupByFields.ToString());
      }

      var targetFolder = GetFolderFromObject(folder);
      var simpleFacetedSearch = LuceneHelper.GetSimpleFacetedSearch(targetFolder, groupByFieldsList.ToArray());
      return new SimpleFacetedSearchInstance(this.Engine.Object.InstancePrototype, simpleFacetedSearch);
    }

    #region Query Creation
    [JSFunction(Name = "createTermQuery")]
    public TermQueryInstance CreateTermQuery(string fieldName, string text)
    {
      return new TermQueryInstance(this.Engine.Object.InstancePrototype, new TermQuery(new Term(fieldName, text)));
    }

    [JSFunction(Name = "createTermRangeQuery")]
    public TermRangeQueryInstance CreateTermRangeQuery(string fieldName, string lowerTerm, string upperTerm, bool includeLower, bool includeUpper)
    {
      return new TermRangeQueryInstance(this.Engine.Object.InstancePrototype, new TermRangeQuery(fieldName, lowerTerm, upperTerm, includeLower, includeUpper));
    }

    [JSFunction(Name = "createPrefixQuery")]
    public PrefixQueryInstance CreatePrefixQuery(string fieldName, string text)
    {
      return new PrefixQueryInstance(this.Engine.Object.InstancePrototype, new PrefixQuery(new Term(fieldName, text)));
    }

    [JSFunction(Name = "createIntRangeQuery")]
    public NumericRangeQueryInstance<int> CreateIntRangeQuery(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    {
      int? intMin = null;
      if (min != null && min != Null.Value && min != Undefined.Value && min is int)
        intMin = Convert.ToInt32(min);

      int? intMax = null;
      if (max != null && max != Null.Value && max != Undefined.Value && max is int)
        intMax = Convert.ToInt32(max);

      var query = NumericRangeQuery.NewIntRange(fieldName, precisionStep, intMin, intMax, minInclusive, maxInclusive);

      return new NumericRangeQueryInstance<int>(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createDoubleRangeQuery")]
    public NumericRangeQueryInstance<double> CreateDoubleRangeQuery(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    {
      double? doubleMin = null;
      if (min != null && min != Null.Value && min != Undefined.Value && min is int)
        doubleMin = Convert.ToDouble(min);

      double? doubleMax = null;
      if (max != null && max != Null.Value && max != Undefined.Value && max is int)
        doubleMax = Convert.ToDouble(max);

      var query = NumericRangeQuery.NewDoubleRange(fieldName, precisionStep, doubleMin, doubleMax, minInclusive, maxInclusive);

      return new NumericRangeQueryInstance<double>(this.Engine.Object.InstancePrototype, query);
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
      var query = new WildcardQuery(new Term(fieldName, text));
      return new WildcardQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createFuzzyQuery")]
    public FuzzyQueryInstance CreateFuzzyQuery(string fieldName, string text)
    {
      var query = new FuzzyQuery(new Term(fieldName, text));
      return new FuzzyQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createQuery")]
    public GenericQueryInstance CreateQuery(string fieldName, string text)
    {
      var parser = new QueryParser(Version.LUCENE_30, fieldName, new StandardAnalyzer(Version.LUCENE_30));
      var query = parser.Parse(text);
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createMultiFieldQuery")]
    public GenericQueryInstance CreateMultiFieldQuery(ArrayInstance fieldNames, string text)
    {
      if (fieldNames == null)
        throw new JavaScriptException(this.Engine, "Error", "The first parameter must be an array of field names.");

      var parser = new MultiFieldQueryParser(Version.LUCENE_30, fieldNames.ElementValues.OfType<string>().ToArray(), new StandardAnalyzer(Version.LUCENE_30));
      var query = parser.Parse(text);
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createRegexQuery")]
    public RegexQueryInstance CreateRegexQuery(string fieldName, string text)
    {
      var query = new RegexQuery(new Term(fieldName, text));
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

    [JSFunction(Name = "createTermRangeFilter")]
    public TermRangeFilterInstance CreateTermRangeFilter(string fieldName, string lowerTerm, string upperTerm, bool includeLower, bool includeUpper)
    {
      return new TermRangeFilterInstance(this.Engine.Object.InstancePrototype, new TermRangeFilter(fieldName, lowerTerm, upperTerm, includeLower, includeUpper));
    }

    [JSFunction(Name = "createIntRangeFilter")]
    public NumericRangeFilterInstance<int> CreateIntRangeFilter(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    {
      int? intMin = null;
      if (min != null && min != Null.Value && min != Undefined.Value && min is int)
        intMin = Convert.ToInt32(min);

      int? intMax = null;
      if (max != null && max != Null.Value && max != Undefined.Value && max is int)
        intMax = Convert.ToInt32(max);

      var filter = NumericRangeFilter.NewIntRange(fieldName, precisionStep, intMin, intMax, minInclusive, maxInclusive);
      return new NumericRangeFilterInstance<int>(this.Engine.Object.InstancePrototype, filter);
    }

    [JSFunction(Name = "createDoubleRangeFilter")]
    public NumericRangeFilterInstance<Double> CreateDoubleRangeFilter(string fieldName, int precisionStep, object min, object max, bool minInclusive, bool maxInclusive)
    {
      double? doubleMin = null;
      if (min != null && min != Null.Value && min != Undefined.Value && min is int)
        doubleMin = Convert.ToDouble(min);

      double? doubleMax = null;
      if (max != null && max != Null.Value && max != Undefined.Value && max is int)
        doubleMax = Convert.ToDouble(max);

      var filter = NumericRangeFilter.NewDoubleRange(fieldName, precisionStep, doubleMin, doubleMax, minInclusive, maxInclusive);

      return new NumericRangeFilterInstance<double>(this.Engine.Object.InstancePrototype, filter);
    }

    [JSFunction(Name = "createQueryWrapperFilter")]
    public QueryWrapperFilterInstance CreateQueryWrapperFilter<T>(QueryInstance<T> query)
      where T : Query
    {
      return new QueryWrapperFilterInstance(this.Engine.Object.InstancePrototype, new QueryWrapperFilter(query.Query));
    }

    [JSFunction(Name = "createPrefixFilter")]
    public PrefixFilterInstance CreatePrefixFilter(string fieldName, string text)
    {
      return new PrefixFilterInstance(this.Engine.Object.InstancePrototype, new PrefixFilter(new Term(fieldName, text)));
    }
    #endregion

    #region Sort Creation
    [JSFunction(Name = "createIndexOrderSort")]
    public SortInstance CreateIndexOrderSort()
    {
      return new SortInstance(this.Engine.Object.InstancePrototype, Sort.INDEXORDER);
    }

    [JSFunction(Name = "createRelevanceSort")]
    public SortInstance CreateRelevanceSort()
    {
      return new SortInstance(this.Engine.Object.InstancePrototype, Sort.RELEVANCE);
    }

    [JSFunction(Name = "createFieldSort")]
    public SortInstance CreateFieldSort(object fields)
    {
      List<SortField> sortFields = new List<SortField>();
      if (fields is ArrayInstance)
      {
        //Get single strings 
        sortFields.AddRange((fields as ArrayInstance).ElementValues
                                                     .OfType<string>()
                                                     .Select(fn => new SortField(fn, SortField.STRING))
                                                     );

        //Get objects.
        var sortObjects = (fields as ArrayInstance).ElementValues
                                                   .OfType<ObjectInstance>()
                                                   .Where(
                                                     so => so.HasProperty("fieldName") && so.HasProperty("fieldType"));

        foreach (var sortObject in sortObjects)
        {
          string fieldName = sortObject.GetPropertyValue("fieldName").ToString();
          string fieldTypeString = sortObject.GetPropertyValue("fieldType").ToString();
          int sortField;

          switch (fieldTypeString)
          {
            case "Byte":
              sortField = SortField.BYTE;
              break;
            case "Custom":
              sortField = SortField.CUSTOM;
              break;
            case "Doc":
              sortField = SortField.DOC;
              break;
            case "Double":
              sortField = SortField.DOUBLE;
              break;
            case "Float":
              sortField = SortField.FLOAT;
              break;
            case "Int":
              sortField = SortField.INT;
              break;
            case "Long":
              sortField = SortField.LONG;
              break;
            case "Score":
              sortField = SortField.SCORE;
              break;
            case "Short":
              sortField = SortField.SHORT;
              break;
            case "String":
              sortField = SortField.STRING;
              break;
            case "StringVal":
              sortField = SortField.STRING_VAL;
              break;
            default:
              sortField = SortField.STRING;
              break;
          }

          sortFields.Add(new SortField(fieldName, sortField));
        }
      }
      else
      {
        sortFields.Add(new SortField(fields.ToString(), SortField.STRING));
      }

      return new SortInstance(this.Engine.Object.InstancePrototype, new Sort(sortFields.ToArray()));
    }
    #endregion

    [JSFunction(Name = "closeIndexWriter")]
    public void CloseIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      LuceneHelper.CloseIndexWriterSingleton(targetFolder);
    }

    private SPFolder GetFolderFromObject(object folder)
    {
      throw new NotImplementedException();

      //if (folder == Null.Value || folder == Undefined.Value || folder == null)
      //  throw new JavaScriptException(this.Engine, "Error", "A folder must be specified.");

      //SPFolder targetFolder;
      //if (folder is SPFolderInstance)
      //{
      //  var folderInstance = folder as SPFolderInstance;
      //  targetFolder = folderInstance.Folder;
      //}
      //else if (folder is string)
      //{
      //  SPSite site;
      //  SPWeb web;

      //  var url = folder as string;
      //  if (SPHelper.TryGetSPFolder(url, out site, out web, out targetFolder) == false)
      //    throw new JavaScriptException(this.Engine, "Error", "A folder is not available at the specified url.");
      //}
      //else
      //  throw new JavaScriptException(this.Engine, "Error",
      //                                "Cannot create a folder with the specified object: " +
      //                                TypeConverter.ToString(folder));

      //return targetFolder;
    }
  }
}
