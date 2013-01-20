namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class UnsignedLongExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (ulong); }
    }

    public ConstantExpression Convert(string token)
    {
      ulong number;
      if (ulong.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as Unsigned Long.");
    }
  }
}