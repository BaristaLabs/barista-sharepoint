namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Globalization;
  using System.Linq.Expressions;

  internal class DoubleExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (double); }
    }

    public ConstantExpression Convert(string token)
    {
      double number;
      if (double.TryParse(token.Trim('D', 'd'), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as double.");
    }
  }
}