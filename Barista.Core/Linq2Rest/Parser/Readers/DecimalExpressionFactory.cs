namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Globalization;
  using System.Linq.Expressions;

  internal class DecimalExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (decimal); }
    }

    public ConstantExpression Convert(string token)
    {
      decimal number;
      if (decimal.TryParse(token.Trim('M', 'm'), NumberStyles.Any, CultureInfo.InvariantCulture, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as decimal.");
    }
  }
}