namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class ShortExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (short); }
    }

    public ConstantExpression Convert(string token)
    {
      short number;
      if (short.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as Short.");
    }
  }
}