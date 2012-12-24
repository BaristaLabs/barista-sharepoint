namespace Barista.SharePoint.Search.Library
{
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
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

    [JSFunction(Name = "explain")]
    public ExplanationInstance Explain(object searchQuery, object hit)
    {
      if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

      if (hit == null || hit == Null.Value || hit == Undefined.Value)
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

      Explanation explanation;
      if (hit is SearchHitInstance)
      {
        var searchHit = (hit as SearchHitInstance).Hit;
        explanation = m_indexSearcher.Explain(query, searchHit.DocumentId);
      }
      else
      {
        explanation = m_indexSearcher.Explain(query, Convert.ToInt32(hit));
      }

      return new ExplanationInstance(this.Engine.Object.InstancePrototype, explanation);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object searchQuery, object n)
    {
      if (searchQuery == null || searchQuery == Null.Value || searchQuery == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "A search query must be specified as the first parameter.");

      var intN = 100;
      if (n != null && n != Null.Value && n != Undefined.Value && n is int)
        intN = (int) n;

      var searchQueryType = searchQuery.GetType();

      Query query;
      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        query = queryProperty.GetValue(searchQuery, null) as Query;
      }
      else
      {
        var parser = new QueryParser(Version.LUCENE_30, "contents", new SimpleAnalyzer());
        query = parser.Parse(searchQuery.ToString());
      }

      var hitInstances = m_indexSearcher.Search(query, intN).ScoreDocs
                                        .AsQueryable()
                                        .OrderByDescending(hit => hit.Score)
                                        .Select(hit =>
                                            new SearchHitInstance(this.Engine.Object.InstancePrototype, new Hit
                                              {
                                                Score = hit.Score,
                                                DocumentId = hit.Doc,
                                                Document = m_indexSearcher.Doc(hit.Doc)
                                              })
                                          )
                                        .ToArray();

      return this.Engine.Array.Construct(hitInstances);
    }
  }
}
