namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class LongExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (long); }
    }

    public ConstantExpression Convert(string token)
    {
      long number;
      if (long.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as Long.");
    }
  }
}