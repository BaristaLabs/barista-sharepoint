namespace Barista.SharePoint.Search
{
  using Barista.Extensions;
  using Barista.SharePoint.Search.Library;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using System;
  using System.Reflection;
  using Version = Lucene.Net.Util.Version;

  public sealed class SearchArguments
  {
    public SearchArguments()
    {
      Filter = null;
      Sort = null;
      Take = 100;
    }

    public Query Query
    {
      get;
      set;
    }

    public Filter Filter
    {
      get;
      set;
    }

    public Sort Sort
    {
      get;
      set;
    }

    public int Take
    {
      get;
      set;
    }

    public static SearchArguments GetSearchArgumentsFromSearchArgumentsInstance(SearchArgumentsInstance instance)
    {
      if (instance == null)
        throw new ArgumentNullException("instance");

      var result = new SearchArguments();

      //Determine the Query value.
      if (instance.Query == null || instance.Query == Null.Value || instance.Query == Undefined.Value)
        throw new JavaScriptException(instance.Engine, "Error", "The Query property of the argument instance cannot be null or undefined.");

      var searchQueryType = instance.Query.GetType();

      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        result.Query = queryProperty.GetValue(instance.Query, null) as Query;
      }
      else
      {
        var parser = new QueryParser(Version.LUCENE_30, "contents", new StandardAnalyzer(Version.LUCENE_30));
        result.Query = parser.Parse(instance.Query.ToString());
      }
      
      //Determine the Filter value.

      var searchFilterType = instance.Filter.GetType();

      if (searchFilterType.IsSubclassOfRawGeneric(typeof (FilterInstance<>)))
      {
        var filterProperty = searchFilterType.GetProperty("Filter", BindingFlags.Instance | BindingFlags.Public);
        result.Filter = filterProperty.GetValue(instance.Filter, null) as Filter;
      }
      else
      {
        var parser = new QueryParser(Version.LUCENE_30, "contents", new StandardAnalyzer(Version.LUCENE_30));
        var query = parser.Parse(instance.Query.ToString());
        result.Filter = new QueryWrapperFilter(query);
      }

      //Determine the Sort value.
      if (instance.Sort is SortInstance)
      {
        result.Sort = (instance.Sort as SortInstance).Sort;
      }
      else if (instance.Sort is string || instance.Sort is StringInstance || instance.Sort is ConcatenatedString)
      {
        result.Sort = new Sort(new SortField(instance.Sort as string, SortField.STRING));
      }

      return result;
    }
  }
}
