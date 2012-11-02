namespace Barista.DirectoryServices
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;

  /// <summary>
  /// LINQ query provider for Directory Services.
  /// </summary>
  public class DirectoryQueryProvider : IQueryProvider
  {
    #region CreateQuery implementation

    public IQueryable CreateQuery(Expression expression)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Constructs an IQueryable object that can evaluate the query represented by the specified expression tree. 
    /// </summary>
    /// <param name="expression">Expression representing the LDAP query.</param>
    /// <typeparam name="TElement">The type of the elements of the IQueryable that is returned.</typeparam>
    /// <returns>IQueryable object that can evaluate the query represented by the specified expression tree.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new DirectoryQuery<TElement>(expression);
    }

    #endregion

    #region Direct execution (not implemented)

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public TResult Execute<TResult>(Expression expression)
    {
      throw new NotImplementedException();
    }

    public object Execute(Expression expression)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
