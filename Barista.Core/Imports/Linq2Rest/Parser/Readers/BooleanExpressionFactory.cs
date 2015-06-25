namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;
  using System.Text.RegularExpressions;

  internal class BooleanExpressionFactory : IValueExpressionFactory
  {
    private static readonly Regex TrueRegex = new Regex("1|true", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FalseRegex = new Regex("0|false", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Type Handles
    {
      get { return typeof (bool); }
    }

    public ConstantExpression Convert(string token)
    {
      if (TrueRegex.IsMatch(token))
      {
        return Expression.Constant(true);
      }

      if (FalseRegex.IsMatch(token))
      {
        return Expression.Constant(false);
      }

      return Expression.Constant(null);
    }
  }
}