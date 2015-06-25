namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal class UnsignedShortExpressionFactory : IValueExpressionFactory
  {
    public Type Handles
    {
      get { return typeof (ushort); }
    }

    public ConstantExpression Convert(string token)
    {
      ushort number;
      if (ushort.TryParse(token, out number))
      {
        return Expression.Constant(number);
      }

      throw new FormatException("Could not read " + token + " as Unsigned Short.");
    }
  }
}