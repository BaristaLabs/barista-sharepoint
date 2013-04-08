namespace Barista.Search.ODataToLucene
{
  using System;
  using System.Collections.Specialized;
  using System.Web;
  using Barista.Extensions;

  public class ODataQueryParser
  {
    private readonly IQueryFactory m_queryFactory;
    private readonly ISelectFilterFactory m_selectFilterFactory;
    private readonly IQueryFactory m_filterFactory;
    private readonly ISortFactory m_sortFactory;

    public ODataQueryParser()
    {
      m_queryFactory = new QueryFactory();
      //m_selectFilterFactory = new SelectFilterFactory();
      m_filterFactory = new QueryFactory();
      //m_sortFactory = new SortFactory();
    }

    public IQueryFactory QueryFactory
    {
      get { return m_queryFactory; }
    }

    public IQueryFactory FilterFactory
    {
      get { return m_filterFactory; }
    }

    public ISortFactory SortExpressionFactory
    {
      get { return m_sortFactory; }
    }

    public LuceneModelFilter ParseQuery(string defaultField, string queryString)
    {
      var queryParameters = HttpUtility.ParseQueryString(queryString);
      return ParseQuery(defaultField, queryParameters);
    }

    public LuceneModelFilter ParseQuery(string defaultField, NameValueCollection queryParameters)
    {
      var orderbyField = queryParameters[StringConstants.OrderByParameter];
      var query = queryParameters[StringConstants.QueryParameter];
      var selects = queryParameters[StringConstants.SelectParameter];
      var filter = queryParameters[StringConstants.FilterParameter];
      var skip = queryParameters[StringConstants.SkipParameter];
      var top = queryParameters[StringConstants.TopParameter];

      var luceneQuery = m_queryFactory.Create(query);
      //var luceneSelectFilter = m_selectFilterFactory.Create(selects);
      var luceneFilter = m_filterFactory.Create(filter);
      //var luceneSort = m_sortFactory.Create(orderbyField);
      
      var modelFilter = new LuceneModelFilter
        {
          Query = luceneQuery,
          //SelectFilter =  luceneSelectFilter,
          Filter = luceneFilter,
          //Sort = luceneSort,
          Skip = skip.IsNullOrWhiteSpace() ? -1 : Convert.ToInt32(skip),
          Take = top.IsNullOrWhiteSpace() ? -1 : Convert.ToInt32(top)
        };
      return modelFilter;
    }
  }
}
