namespace Barista.Search.Library
{
  using System.Collections.Generic;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Lucene.Net.Search.Vectorhighlight;
  using System;
  using System.Linq;
  using System.Reflection;
  using Version = Lucene.Net.Util.Version;

  [Serializable]
  public class IndexSearcherConstructor : ClrFunction
  {
    public IndexSearcherConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexSearcher", new IndexSearcherInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexSearcherInstance Construct()
    {
      return new IndexSearcherInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexSearcherInstance : ObjectInstance
  {
    private readonly IndexSearcher m_indexSearcher;

    public IndexSearcherInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexSearcherInstance(ObjectInstance prototype, IndexSearcher indexSearcher)
      : this(prototype)
    {
      if (indexSearcher == null)
        throw new ArgumentNullException("indexSearcher");

      m_indexSearcher = indexSearcher;
    }

    public IndexSearcher IndexSearcher
    {
      get { return m_indexSearcher; }
    }

    [JSFunction(Name = "highlight")]
    public string Highlight(object searchQuery, object scoreDoc, string fieldName)
    {
      if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

      if (scoreDoc == null || scoreDoc == Null.Value || scoreDoc == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search result or document id must be specified as the second parameter.");

      var searchQueryType = searchQuery.GetType();

      Query query;
      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        query = queryProperty.GetValue(searchQuery, null) as Query;
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "The first parameter must be a query object.");

      var highlighter = GetFastVectorHighlighter();
      FieldQuery fieldQuery = highlighter.GetFieldQuery(query);
      string highlightedResult;
      if (scoreDoc is ScoreDocInstance)
      {
        var scoreDocInstance = scoreDoc as ScoreDocInstance;
        highlightedResult = highlighter.GetBestFragment(fieldQuery, m_indexSearcher.IndexReader, scoreDocInstance.DocumentId,
                                                        fieldName, 100);
      }
      else if (scoreDoc is HitsPerFacetInstance)
      {
          throw new JavaScriptException(this.Engine, "Error", "Cannot highlight a HitsPerFacet result instance.");
      }
      else
      {
        highlightedResult = highlighter.GetBestFragment(fieldQuery, m_indexSearcher.IndexReader,
                                                        Convert.ToInt32(scoreDoc),
                                                        fieldName, 100);
      }

      return highlightedResult;
    }

    [JSFunction(Name = "explain")]
    public ExplanationInstance Explain(object searchQuery, object scoreDoc)
    {
      if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

      if (scoreDoc == null || scoreDoc == Null.Value || scoreDoc == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search result or document id must be specified as the second parameter.");

      var searchQueryType = searchQuery.GetType();

      Query query;
      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        query = queryProperty.GetValue(searchQuery, null) as Query;
      }
      else if (scoreDoc is HitsPerFacetInstance)
      {
        throw new JavaScriptException(this.Engine, "Error", "Cannot explain a HitsPerFacet result instance.");
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "The first parameter must be a query object.");

      Explanation explanation;
      if (scoreDoc is ScoreDocInstance)
      {
        var searchHit = scoreDoc as ScoreDocInstance;
        explanation = m_indexSearcher.Explain(query, searchHit.DocumentId);
      }
      else
      {
        explanation = m_indexSearcher.Explain(query, Convert.ToInt32(scoreDoc));
      }

      return new ExplanationInstance(this.Engine.Object.InstancePrototype, explanation);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object searchQuery, object n)
    {
      if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

      SearchArguments searchArguments = new SearchArguments();

      if (n != null && n != Null.Value && n != Undefined.Value && n is int)
        searchArguments.Take = (int)n;

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

      IEnumerable<ScoreDocInstance> scoreDocInstances;
      if (searchArguments.Sort == null)
      {
        scoreDocInstances = m_indexSearcher.Search(searchArguments.Query, searchArguments.Filter, searchArguments.Take).ScoreDocs
                                          .AsQueryable()
                                          .Select(scoreDoc =>
                                                  new ScoreDocInstance(this.Engine.Object.InstancePrototype, scoreDoc,
                                                                       m_indexSearcher)
                                                 );
      }
      else
      {
        scoreDocInstances = m_indexSearcher.Search(searchArguments.Query, searchArguments.Filter, searchArguments.Take,
                                                  searchArguments.Sort).ScoreDocs
                                          .AsQueryable()
                                          .Select(scoreDoc =>
                                                  new ScoreDocInstance(this.Engine.Object.InstancePrototype, scoreDoc,
                                                                       m_indexSearcher)
                                                 );
      }
      

      return this.Engine.Array.Construct(scoreDocInstances.ToArray());
    }

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
    }

    /// <summary>
    /// Utility method to get a default FastVectorHighlighter.
    /// </summary>
    /// <returns></returns>
    public static FastVectorHighlighter GetFastVectorHighlighter()
    {
      FragListBuilder fragListBuilder = new SimpleFragListBuilder();
      FragmentsBuilder fragmentBuilder = new ScoreOrderFragmentsBuilder(BaseFragmentsBuilder.COLORED_PRE_TAGS, BaseFragmentsBuilder.COLORED_POST_TAGS);
      return new FastVectorHighlighter(true, true, fragListBuilder, fragmentBuilder);
    }

  }
}
