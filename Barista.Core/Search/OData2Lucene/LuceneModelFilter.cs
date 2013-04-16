namespace Barista.Search.OData2Lucene
{
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Lucene.Net.Util;

  public class LuceneModelFilter
  {
    public string Query
    {
      get;
      set;
    }

    public string SelectFilter
    {
      get;
      set;
    }

    public string Filter
    {
      get;
      set;
    }

    public Sort Sort
    {
      get;
      set;
    }

    public int Skip
    {
      get;
      set;
    }

    public int Take
    {
      get;
      set;
    }

    /// <summary>
    /// Returns a query based on the Lucene Query Parser format.
    /// </summary>
    /// <param name="defaultField"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    public static Query ParseQuery(string defaultField, string query)
    {
      var parser = new QueryParser(Version.LUCENE_30, defaultField, new StandardAnalyzer(Version.LUCENE_30));
      return parser.Parse(query);
    }

    /// <summary>
    /// Returns a filter based on the Lucene Query Parser format.
    /// </summary>
    /// <param name="defaultField"></param>
    /// <param name="filterQuery"></param>
    /// <returns></returns>
    public static Filter ParseFilter(string defaultField, string filterQuery)
    {
      var parser = new QueryParser(Version.LUCENE_30, defaultField, new StandardAnalyzer(Version.LUCENE_30));
      var lQuery = parser.Parse(filterQuery);
      return new QueryWrapperFilter(lQuery);
    }
  }
}
