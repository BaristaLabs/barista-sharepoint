namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Globalization;
  using System.Linq.Expressions;

  internal class ByteExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (byte); }
    }

    public ConstantExpression Convert(string token)
    {
      byte number;
      if (byte.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as byte.");
    }
  }
}