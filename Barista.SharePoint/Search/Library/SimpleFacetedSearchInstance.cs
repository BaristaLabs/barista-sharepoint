namespace Barista.SharePoint.Search.Library
{
  using System.Linq;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using System;
  using System.Reflection;
  using Version = Lucene.Net.Util.Version;

  [Serializable]
  public class SimpleFacetedSearchConstructor : ClrFunction
  {
    public SimpleFacetedSearchConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SimpleFacetedSearch", new SimpleFacetedSearchInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SimpleFacetedSearchInstance Construct()
    {
      return new SimpleFacetedSearchInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SimpleFacetedSearchInstance : ObjectInstance
  {
    private readonly SimpleFacetedSearch m_simpleFacetedSearch;

    public SimpleFacetedSearchInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SimpleFacetedSearchInstance(ObjectInstance prototype, SimpleFacetedSearch simpleFacetedSearch)
      : this(prototype)
    {
      if (simpleFacetedSearch == null)
        throw new ArgumentNullException("simpleFacetedSearch");

      m_simpleFacetedSearch = simpleFacetedSearch;
    }

    public SimpleFacetedSearch SimpleFacetedSearch
    {
      get { return m_simpleFacetedSearch; }
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
      if (searchQueryType.IsSubclassOfRawGeneric(typeof (QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        query = queryProperty.GetValue(searchQuery, null) as Query;
      }
      else
      {
        var parser = new QueryParser(Version.LUCENE_30, "contents", new SimpleAnalyzer());
        query = parser.Parse(searchQuery.ToString());
      }

      var hitInstances = m_simpleFacetedSearch.Search(query, intN).HitsPerFacet
                                       .AsQueryable()
                                       .OrderByDescending(hit => hit.HitCount)
                                       .Select(hit =>
                                           new HitsPerFacetInstance(this.Engine.Object.InstancePrototype, hit)
                                         )
                                       .ToArray();

      return this.Engine.Array.Construct(hitInstances);
    }
  }
}
