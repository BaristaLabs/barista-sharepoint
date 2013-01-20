namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Collections.Specialized;
  using Barista.Extensions;

  /// <summary>
  /// Defines the default implementation of a parameter parser.
  /// </summary>
  /// <typeparam name="T">The <see cref="Type"/> of item to create parser for.</typeparam>
  public class ParameterParser<T> : IParameterParser<T>
  {
    private readonly IFilterExpressionFactory m_filterExpressionFactory;
    private readonly ISelectExpressionFactory<T> m_selectExpressionFactory;
    private readonly ISortExpressionFactory m_sortExpressionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterParser{T}"/> class.
    /// </summary>
    public ParameterParser()
    {
      m_filterExpressionFactory = new FilterExpressionFactory();
      m_sortExpressionFactory = new SortExpressionFactory();
      var nameResolver = new MemberNameResolver();
      m_selectExpressionFactory = new SelectExpressionFactory<T>(nameResolver, new RuntimeTypeProvider(nameResolver));
    }

    public IFilterExpressionFactory FilterExpressionFactory
    {
      get { return m_filterExpressionFactory; }
    }

    public ISelectExpressionFactory<T> SelectExpressionFactory
    {
      get { return m_selectExpressionFactory; }
    }

    public ISortExpressionFactory SortExpressionFactory
    {
      get { return m_sortExpressionFactory; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterParser{T}"/> class.
    /// </summary>
    /// <param name="filterExpressionFactory">The <see cref="IFilterExpressionFactory"/> to use.</param>
    /// <param name="sortExpressionFactory">The <see cref="ISortExpressionFactory"/> to use.</param>
    /// <param name="selectExpressionFactory">The <see cref="ISelectExpressionFactory{T}"/> to use.</param>
    public ParameterParser(
      IFilterExpressionFactory filterExpressionFactory,
      ISortExpressionFactory sortExpressionFactory,
      ISelectExpressionFactory<T> selectExpressionFactory)
    {
      if (filterExpressionFactory == null)
        throw new ArgumentNullException("filterExpressionFactory");

      if (sortExpressionFactory == null)
        throw new ArgumentNullException("sortExpressionFactory");

      if (selectExpressionFactory == null)
        throw new ArgumentNullException("selectExpressionFactory");

      m_filterExpressionFactory = filterExpressionFactory;
      m_sortExpressionFactory = sortExpressionFactory;
      m_selectExpressionFactory = selectExpressionFactory;
    }

    /// <summary>
    /// Parses the passes query parameters to a <see cref="ModelFilter{T}"/>.
    /// </summary>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public IModelFilter<T> Parse(NameValueCollection queryParameters)
    {
      var orderbyField = queryParameters[StringConstants.OrderByParameter];
      var selects = queryParameters[StringConstants.SelectParameter];
      var filter = queryParameters[StringConstants.FilterParameter];
      var skip = queryParameters[StringConstants.SkipParameter];
      var top = queryParameters[StringConstants.TopParameter];

      var filterExpression = m_filterExpressionFactory.Create<T>(filter);
      var sortDescriptions = m_sortExpressionFactory.Create<T>(orderbyField);
      var selectFunction = m_selectExpressionFactory.Create(selects);

      var modelFilter = new ModelFilter<T>(
        filterExpression,
        selectFunction,
        sortDescriptions,
        skip.IsNullOrWhiteSpace() ? -1 : Convert.ToInt32(skip),
        top.IsNullOrWhiteSpace() ? -1 : Convert.ToInt32(top));
      return modelFilter;
    }
  }
}