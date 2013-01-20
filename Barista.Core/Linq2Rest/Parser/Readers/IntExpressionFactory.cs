namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class IntExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (int); }
    }

    public ConstantExpression Convert(string token)
    {
      int number;
      if (int.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as integer.");
    }
  }
}