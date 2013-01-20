namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class UnsignedIntExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (uint); }
    }

    public ConstantExpression Convert(string token)
    {
      uint number;
      if (uint.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as Unsigned Integer.");
    }
  }
}