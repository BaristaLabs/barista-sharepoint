namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Web.UI.WebControls;
  using Barista.Extensions;

  /// <summary>
  /// Defines the SortExpressionFactory´.
  /// </summary>
  public class SortExpressionFactory : ISortExpressionFactory
  {
    /// <summary>
    /// Creates an enumeration of sort descriptions from its string representation.
    /// </summary>
    /// <param name="filter">The string representation of the sort descriptions.</param>
    /// <typeparam name="T">The <see cref="Type"/> of item to sort.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> if the passed sort descriptions are valid, otherwise null.</returns>
    public IEnumerable<SortDescription> Create<T>(string filter)
    {
      if (filter.IsNullOrWhiteSpace())
      {
        return new SortDescription[0];
      }

      var parameterExpression = Expression.Parameter(typeof (T), "x");

      var sortTokens = filter.Split(',');
      return from sortToken in sortTokens
             select sortToken.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
             into sort
             let property = GetPropertyExpression<T>(sort.First(), parameterExpression)
             where property != null
             let direction = sort.ElementAtOrDefault(1) == "desc" ? SortDirection.Descending : SortDirection.Ascending
             select new SortDescription(property, direction);
    }

    private static Expression GetPropertyExpression<T>(string propertyToken, ParameterExpression parameter)
    {
      if (propertyToken.IsNullOrWhiteSpace())
      {
        return null;
      }

      var parentType = typeof (T);
      Expression propertyExpression = null;
      var propertyChain = propertyToken.Split('/');
      foreach (var propertyName in propertyChain)
      {
        var property = parentType.GetProperty(propertyName);
        if (property != null)
        {
          parentType = property.PropertyType;
          propertyExpression = propertyExpression == null
                                 ? Expression.Property(parameter, property)
                                 : Expression.Property(propertyExpression, property);
        }
      }

      if (propertyExpression == null)
      {
        throw new FormatException(propertyToken + " is not recognized as a valid property");
      }

      var funcType = typeof (Func<,>).MakeGenericType(typeof (T), parentType);

      return Expression.Lambda(funcType, propertyExpression, parameter);
    }
  }
}