namespace Barista.Linq2Rest.Provider
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using Barista.Extensions;

  internal class ExpressionProcessor : IExpressionProcessor
  {
    private readonly IExpressionWriter m_writer;

    public ExpressionProcessor(IExpressionWriter writer)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");

      m_writer = writer;
    }

    public object ProcessMethodCall<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                       Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                       Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
      if (methodCall == null)
      {
        return null;
      }

      var method = methodCall.Method.Name;

      switch (method)
      {
        case "First":
        case "FirstOrDefault":
          builder.TakeParameter = "1";
          return methodCall.Arguments.Count >= 2
                   ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                   : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);

        case "Single":
        case "SingleOrDefault":
        case "Last":
        case "LastOrDefault":
        case "Count":
        case "LongCount":
          return methodCall.Arguments.Count >= 2
                   ? GetMethodResult(methodCall, builder, resultLoader, intermediateResultLoader)
                   : GetResult(methodCall, builder, resultLoader, intermediateResultLoader);
        case "Where":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            var newFilter = m_writer.Write(methodCall.Arguments[1]);

            builder.FilterParameter = builder.FilterParameter.IsNullOrWhiteSpace()
                                        ? newFilter
                                        : string.Format("({0}) and ({1})", builder.FilterParameter, newFilter);
          }

          break;
        case "Select":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            var unaryExpression = methodCall.Arguments[1] as UnaryExpression;
            if (unaryExpression != null)
            {
              var lambdaExpression = unaryExpression.Operand as LambdaExpression;
              if (lambdaExpression != null)
              {
                var selectFunction = lambdaExpression.Body as NewExpression;

                if (selectFunction != null)
                {
                  var members = selectFunction.Members.Select(x => x.Name).ToArray();
                  var args = selectFunction.Arguments.OfType<MemberExpression>().Select(x => x.Member.Name).ToArray();
                  if (members.Intersect(args).Count() != members.Length)
                  {
                    throw new InvalidOperationException("Projection into new member names is not supported.");
                  }

                  builder.SelectParameter = string.Join(",", args);
                }

                var propertyExpression = lambdaExpression.Body as MemberExpression;
                if (propertyExpression != null)
                {
                  builder.SelectParameter = builder.SelectParameter.IsNullOrWhiteSpace()
                                              ? propertyExpression.Member.Name
                                              : builder.SelectParameter + "," + propertyExpression.Member.Name;
                }
              }
            }
          }

          break;
        case "OrderBy":
        case "ThenBy":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            var item = m_writer.Write(methodCall.Arguments[1]);
            builder.OrderByParameter.Add(item);
          }

          break;
        case "OrderByDescending":
        case "ThenByDescending":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            var visit = m_writer.Write(methodCall.Arguments[1]);
            builder.OrderByParameter.Add(visit + " desc");
          }

          break;
        case "Take":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            builder.TakeParameter = m_writer.Write(methodCall.Arguments[1]);
          }

          break;
        case "Skip":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            builder.SkipParameter = m_writer.Write(methodCall.Arguments[1]);
          }

          break;
        case "Expand":
          {
            var result = ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader,
                                           intermediateResultLoader);
            if (result != null)
            {
              return InvokeEager(methodCall, result);
            }

            var expression = methodCall.Arguments[1];

            var objectMember = Expression.Convert(expression, typeof (object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember).Compile();

            builder.ExpandParameter = getterLambda().ToString();
          }

          break;
        default:
          return ExecuteMethod(methodCall, builder, resultLoader, intermediateResultLoader);
      }

      return null;
    }

    private static object InvokeEager(MethodCallExpression methodCall, object source)
    {
      if (methodCall == null)
        throw new ArgumentNullException("methodCall");

      if (source == null)
        throw new ArgumentNullException("source");

      var results = source as IEnumerable;

      var parameters = ResolveInvocationParameters(results, methodCall);
      return methodCall.Method.Invoke(null, parameters);
    }

    private static object[] ResolveInvocationParameters(IEnumerable results, MethodCallExpression methodCall)
    {
      if (results == null)
        throw new ArgumentNullException("results");

      if (methodCall == null)
        throw new ArgumentNullException("methodCall");

      var parameters = new object[] {results.AsQueryable()}
        .Concat(methodCall.Arguments.Where((x, i) => i > 0).Select(GetExpressionValue))
        .Where(x => x != null)
        .ToArray();
      return parameters;
    }

    private static object GetExpressionValue(Expression expression)
    {
      if (expression is UnaryExpression)
      {
        return (expression as UnaryExpression).Operand;
      }

      if (expression is ConstantExpression)
      {
        return (expression as ConstantExpression).Value;
      }

      return null;
    }

    private object GetMethodResult<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                      Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                      Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
      if (methodCall == null)
        throw new ArgumentNullException("methodCall");

      if (builder == null)
        throw new ArgumentNullException("builder");

      if (resultLoader == null)
        throw new ArgumentNullException("resultLoader");

      if (intermediateResultLoader == null)
        throw new ArgumentNullException("intermediateResultLoader");

      ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);

      var processResult = m_writer.Write(methodCall.Arguments[1]);
      var currentParameter = builder.FilterParameter.IsNullOrWhiteSpace()
                               ? processResult
                               : string.Format("({0}) and ({1})", builder.FilterParameter, processResult);
      builder.FilterParameter = currentParameter;

      var genericArguments = methodCall.Method.GetGenericArguments();
      var queryableMethods = typeof (Queryable).GetMethods();

      var nonGenericMethod = queryableMethods
        .Single(x => x.Name == methodCall.Method.Name && x.GetParameters().Length == 1);

      var method = nonGenericMethod
        .MakeGenericMethod(genericArguments);

      var list = resultLoader(builder);

      var queryable = list.AsQueryable();
      var parameters = new object[] {queryable};
      return method.Invoke(null, parameters);
    }

    private object GetResult<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
      if (methodCall == null)
        throw new ArgumentNullException("methodCall");

      if (builder == null)
        throw new ArgumentNullException("builder");

      if (resultLoader == null)
        throw new ArgumentNullException("resultLoader");

      if (intermediateResultLoader == null)
        throw new ArgumentNullException("intermediateResultLoader");

      ProcessMethodCall(methodCall.Arguments[0] as MethodCallExpression, builder, resultLoader, intermediateResultLoader);
      var results = resultLoader(builder);

      var parameters = ResolveInvocationParameters(results, methodCall);
      var final = methodCall.Method.Invoke(null, parameters);
      return final;
    }

    private object ExecuteMethod<T>(MethodCallExpression methodCall, ParameterBuilder builder,
                                    Func<ParameterBuilder, IEnumerable<T>> resultLoader,
                                    Func<Type, ParameterBuilder, IEnumerable> intermediateResultLoader)
    {
      if (methodCall == null)
        throw new ArgumentNullException("methodCall");

      if (builder == null)
        throw new ArgumentNullException("builder");

      if (resultLoader == null)
        throw new ArgumentNullException("resultLoader");

      if (intermediateResultLoader == null)
        throw new ArgumentNullException("intermediateResultLoader");

      var innerMethod = methodCall.Arguments[0] as MethodCallExpression;

      if (innerMethod == null)
      {
        return null;
      }

      var result = ProcessMethodCall(innerMethod, builder, resultLoader, intermediateResultLoader);
      if (result != null)
      {
        return InvokeEager(innerMethod, result);
      }

      var genericArgument = innerMethod.Method.ReturnType.GetGenericArguments()[0];
      var type = typeof (T);
      var list = type != genericArgument
                   ? intermediateResultLoader(genericArgument, builder)
                   : resultLoader(builder);

      var arguments = ResolveInvocationParameters(list, methodCall);

      return methodCall.Method.Invoke(null, arguments);
    }
  }
}