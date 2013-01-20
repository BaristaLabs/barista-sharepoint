namespace Barista.Linq2Rest
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;

  internal class UntypedQueryable<T> : IQueryable<object>
  {
    private readonly IQueryable m_source;

    public UntypedQueryable(IQueryable<T> source, Expression<Func<T, object>> projection)
    {
      m_source = projection == null
                   ? (IQueryable) source
                   : source.Select(projection);
    }

    public Expression Expression
    {
      get { return m_source.Expression; }
    }

    public Type ElementType
    {
      get { return typeof (T); }
    }

    public IQueryProvider Provider
    {
      get { return m_source.Provider; }
    }

    public IEnumerator<object> GetEnumerator()
    {
      return Enumerable.Cast<object>(m_source).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}