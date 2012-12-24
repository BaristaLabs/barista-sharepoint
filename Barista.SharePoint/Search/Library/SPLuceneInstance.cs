using System.Collections.Generic;

namespace Barista.SharePoint.Search.Library
{
  using Barista.SharePoint.Library;
  using Barista.SharePoint.Search;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis;
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
    public IndexSearcherInstance GetIndexSearcher(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var indexSearcher = LuceneHelper.GetIndexSearcher(targetFolder);

      return new IndexSearcherInstance(this.Engine.Object.InstancePrototype, indexSearcher);
    }

    [JSFunction(Name = "getIndexWriter")]
    public IndexWriterInstance GetIndexWriter(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var bCreateIndex = false;
      if (createIndex != null && createIndex != Null.Value && createIndex != Undefined.Value && createIndex is Boolean)
        bCreateIndex = (bool)createIndex;

      var indexWriter = LuceneHelper.GetIndexWriterSingleton(targetFolder, bCreateIndex);
      return new IndexWriterInstance(this.Engine.Object.InstancePrototype, indexWriter, targetFolder);
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
      var parser = new QueryParser(Version.LUCENE_30, fieldName, new SimpleAnalyzer());
      var query = parser.Parse(text);
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }

    [JSFunction(Name = "createMultiFieldQuery")]
    public GenericQueryInstance CreateMultiFieldQuery(ArrayInstance fieldNames, string text)
    {
      if (fieldNames == null)
        throw new JavaScriptException(this.Engine, "Error", "The first parameter must be an array of field names.");

      var parser = new MultiFieldQueryParser(Version.LUCENE_30, fieldNames.ElementValues.OfType<string>().ToArray(), new SimpleAnalyzer());
      var query = parser.Parse(text);
      return new GenericQueryInstance(this.Engine.Object.InstancePrototype, query);
    }
    #endregion

    [JSFunction(Name = "closeIndexWriter")]
    public void CloseIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      LuceneHelper.CloseIndexWriterSingleton(targetFolder);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object folder, string query, object n)
    {

      var targetFolder = GetFolderFromObject(folder);

      var intN = 100;
      if (n != null && n != Null.Value && n != Undefined.Value && n is int)
        intN = (int) n;

      var hitInstances = LuceneHelper.Search(targetFolder, query, intN)
                                     .Select(hit => new SearchHitInstance(this.Engine.Object.InstancePrototype, hit))
                                     .ToArray();

      return this.Engine.Array.Construct(hitInstances);
    }

    private SPFolder GetFolderFromObject(object folder)
    {
      if (folder == Null.Value || folder == Undefined.Value || folder == null)
        throw new JavaScriptException(this.Engine, "Error", "A folder must be specified.");

      SPFolder targetFolder;
      if (folder is SPFolderInstance)
      {
        var folderInstance = folder as SPFolderInstance;
        targetFolder = folderInstance.Folder;
      }
      else if (folder is string)
      {
        SPSite site;
        SPWeb web;

        var url = folder as string;
        if (SPHelper.TryGetSPFolder(url, out site, out web, out targetFolder) == false)
          throw new JavaScriptException(this.Engine, "Error", "A folder is not available at the specified url.");
      }
      else
        throw new JavaScriptException(this.Engine, "Error",
                                      "Cannot create a folder with the specified object: " +
                                      TypeConverter.ToString(folder));

      return targetFolder;
    }
  }
}
