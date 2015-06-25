namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;

  internal interface IValueExpressionFactory
  {
    Type Handles
    {
      get;
    }

    ConstantExpression Convert(string token);
  }

  internal abstract class ValueExpressionFactoryContracts : IValueExpressionFactory
  {
    public Type Handles
    {
      get { throw new NotImplementedException(); }
    }

    public ConstantExpression Convert(string token)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      throw new NotImplementedException();
    }
  }
}