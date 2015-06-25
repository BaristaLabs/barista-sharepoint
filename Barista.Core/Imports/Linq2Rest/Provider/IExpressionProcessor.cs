namespace Barista.Imports.Linq2Rest.Provider
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq.Expressions;

  internal interface IExpressionProcessor
  {
    object ProcessMethodCall<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader);
  }

  internal abstract class ExpressionProcessorContracts : IExpressionProcessor
  {
    public object ProcessMethodCall<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                       Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                       Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
      if (builder == null)
        throw new ArgumentNullException("builder");

      if (resultLoader == null)
        throw new ArgumentNullException("resultLoader");

      if (intermediateResultLoader == null)
        throw new ArgumentNullException("intermediateResultLoader");

      throw new NotImplementedException();
    }
  }
}