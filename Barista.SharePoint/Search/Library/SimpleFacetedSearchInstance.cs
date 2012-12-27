namespace Barista.SharePoint.Search.Library
{
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Standard;
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

      SearchArguments searchArguments = new SearchArguments();

      if (n != null && n != Null.Value && n != Undefined.Value && n is int)
        searchArguments.Take = (int) n;

      var searchQueryType = searchQuery.GetType();

      if (searchQueryType.IsSubclassOfRawGeneric(typeof (QueryInstance<>)))
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


      var hitsPerFacetInstances = m_simpleFacetedSearch.Search(searchArguments.Query, searchArguments.Take).HitsPerFacet
                                .AsQueryable()
                                .OrderByDescending(hit => hit.HitCount)
                                .Select(hit =>
                                        new HitsPerFacetInstance(this.Engine.Object.InstancePrototype, hit)
                                  );

      return this.Engine.Array.Construct(hitsPerFacetInstances.ToArray());
    }
  }
}
