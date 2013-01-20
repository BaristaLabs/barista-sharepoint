namespace Barista.Linq2Rest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Web.UI.WebControls;
  using Linq2Rest.Parser;

  internal class ModelFilter<T> : IModelFilter<T>
  {
    private readonly Expression<Func<T, bool>> m_filterExpression;
    private readonly Expression<Func<T, object>> m_selectExpression;
    private readonly int m_skip;
    private readonly IEnumerable<SortDescription> m_sortDescriptions;
    private readonly int m_top;

    public ModelFilter(Expression<Func<T, bool>> filterExpression, Expression<Func<T, object>> selectExpression,
                       IEnumerable<SortDescription> sortDescriptions, int skip, int top)
    {
      m_skip = skip;
      m_top = top;
      m_filterExpression = filterExpression;
      m_selectExpression = selectExpression;
      m_sortDescriptions = sortDescriptions;
    }

    public IQueryable<object> Filter(IEnumerable<T> model)
    {
      var result = m_filterExpression != null
                     ? model.AsQueryable().Where(m_filterExpression)
                     : model.AsQueryable();

      if (m_sortDescriptions != null && m_sortDescriptions.Any())
      {
        var isFirst = true;
        foreach (var sortDescription in m_sortDescriptions.Where(x => x != null))
        {
          if (isFirst)
          {
            isFirst = false;
            result = sortDescription.Direction == SortDirection.Ascending
                       ? result.OrderBy(sortDescription.KeySelector)
                       : result.OrderByDescending(sortDescription.KeySelector);
          }
          else
          {
            var orderedEnumerable = result as IOrderedQueryable<T>;

            result = sortDescription.Direction == SortDirection.Ascending
                       ? orderedEnumerable.ThenBy(sortDescription.KeySelector)
                       : orderedEnumerable.ThenByDescending(sortDescription.KeySelector);
          }
        }
      }

      if (m_skip > 0)
      {
        result = result.Skip(m_skip);
      }

      if (m_top > -1)
      {
        result = result.Take(m_top);
      }

      return new UntypedQueryable<T>(result, m_selectExpression);
    }
  }
}