namespace Barista.Imports.Linq2Rest.Parser
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using Barista.Extensions;
  using Linq2Rest.Parser.Readers;
  using System.Diagnostics;

  /// <summary>
  /// Defines the FilterExpressionFactory.
  /// </summary>
  public class FilterExpressionFactory : IFilterExpressionFactory
  {
    private static readonly Regex StringRx = new Regex(@"^[""'](.*?)[""']$", RegexOptions.Compiled);
    private static readonly Regex NegateRx = new Regex(@"^-[^\d]*", RegexOptions.Compiled);

    /// <summary>
    /// Creates a filter expression from its string representation.
    /// </summary>
    /// <param name="filter">The string representation of the filter.</param>
    /// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
    /// <returns>An <see cref="Expression{TDelegate}"/> if the passed filter is valid, otherwise null.</returns>
    public Expression<Func<T, bool>> Create<T>(string filter)
    {
      return Create<T>(filter, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Creates a filter expression from its string representation.
    /// </summary>
    /// <param name="filter">The string representation of the filter.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use when reading the filter.</param>
    /// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
    /// <returns>An <see cref="Expression{TDelegate}"/> if the passed filter is valid, otherwise null.</returns>
    public Expression<Func<T, bool>> Create<T>(string filter, IFormatProvider formatProvider)
    {
      if (filter.IsNullOrWhiteSpace())
      {
        return x => true;
      }

      var parameter = Expression.Parameter(typeof (T), "x");

      var expression = CreateExpression<T>(filter, parameter, new List<ParameterExpression>(), null, formatProvider);

      if (expression != null)
      {
        return Expression.Lambda<Func<T, bool>>(expression, parameter);
      }

      throw new InvalidOperationException("Could not create valid expression from: " + filter);
    }

    private static Type GetFunctionParameterType(string operation)
    {
      if (operation == null)
        throw new ArgumentNullException("operation");

      switch (operation.ToLowerInvariant())
      {
        case "substring":
          return typeof (int);
        default:
          return null;
      }
    }

    private static Expression GetPropertyExpression<T>(string propertyToken, ParameterExpression parameter,
                                                       ICollection<ParameterExpression> lambdaParameters)
    {
      if (propertyToken == null)
        throw new ArgumentNullException("propertyToken");

      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      if (!propertyToken.IsImpliedBoolean())
      {
        var token = propertyToken.GetTokens().FirstOrDefault();
        if (token != null)
        {
          return GetPropertyExpression<T>(token.Left, parameter, lambdaParameters) ??
                 GetPropertyExpression<T>(token.Right, parameter, lambdaParameters);
        }
      }

      var parentType = parameter.Type;
      Expression propertyExpression = null;

      var propertyChain = propertyToken.Split('/');

      Debug.Assert(propertyChain != null);

      if (propertyChain.Any() && lambdaParameters.Any(p => p.Name == propertyChain.First()))
      {
        ParameterExpression lambdaParameter = lambdaParameters.First(p => p.Name == propertyChain.First());

        parentType = lambdaParameter.Type;
        propertyExpression = lambdaParameter;
      }

      foreach (var propertyName in propertyChain)
      {
        var property = parentType.GetProperty(propertyName);
        if (property != null)
        {
          parentType = property.PropertyType;
          propertyExpression = propertyExpression == null
                                 ? Expression.Property(parameter, property)
                                 : Expression.Property(propertyExpression, property);
        }
      }

      return propertyExpression;
    }

    private static Type GetExpressionType<T>(TokenSet set, ParameterExpression parameter,
                                             ICollection<ParameterExpression> lambdaParameters)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      if (set == null)
      {
        return null;
      }

      if (Regex.IsMatch(set.Left, @"^\(.*\)$") && set.Operation.IsCombinationOperation())
      {
        return null;
      }

      var property = GetPropertyExpression<T>(set.Left, parameter, lambdaParameters) ??
                     GetPropertyExpression<T>(set.Right, parameter, lambdaParameters);
      if (property != null)
      {
        return property.Type;
      }

      var type = GetExpressionType<T>(set.Left.GetArithmeticToken(), parameter, lambdaParameters);

      return type ?? GetExpressionType<T>(set.Right.GetArithmeticToken(), parameter, lambdaParameters);
    }

    private static Expression GetOperation(string token, Expression left, Expression right)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (right == null)
        throw new ArgumentNullException("right");

      return left == null ? GetRightOperation(token, right) : GetLeftRightOperation(token, left, right);
    }

    private static Expression GetLeftRightOperation(string token, Expression left, Expression right)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (right == null)
        throw new ArgumentNullException("right");

      if (left == null)
        throw new ArgumentNullException("left");

      switch (token.ToLowerInvariant())
      {
        case "eq":
          if (left.Type.IsEnum && left.Type.GetCustomAttributes(typeof (FlagsAttribute), true).Any())
          {
            var underlyingType = Enum.GetUnderlyingType(left.Type);
            var leftValue = Expression.Convert(left, underlyingType);
            var rightValue = Expression.Convert(right, underlyingType);
            var andExpression = Expression.And(leftValue, rightValue);
            return Expression.Equal(andExpression, rightValue);
          }

          return Expression.Equal(left, right);
        case "ne":
          return Expression.NotEqual(left, right);
        case "gt":
          return Expression.GreaterThan(left, right);
        case "ge":
          return Expression.GreaterThanOrEqual(left, right);
        case "lt":
          return Expression.LessThan(left, right);
        case "le":
          return Expression.LessThanOrEqual(left, right);
        case "and":
          return Expression.AndAlso(left, right);
        case "or":
          return Expression.OrElse(left, right);
        case "add":
          return Expression.Add(left, right);
        case "sub":
          return Expression.Subtract(left, right);
        case "mul":
          return Expression.Multiply(left, right);
        case "div":
          return Expression.Divide(left, right);
        case "mod":
          return Expression.Modulo(left, right);
      }

      throw new InvalidOperationException("Could not understand operation: " + token);
    }

    private static Expression GetRightOperation(string token, Expression right)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (right == null)
        throw new ArgumentNullException("right");

      Expression result = null;
      switch (token.ToLowerInvariant())
      {
        case "not":
          result = right.Type == typeof (bool) ? Expression.Not(right) : null;
          break;
      }

      if (result == null)
      {
        throw new InvalidOperationException(string.Format("Could not create valid expression from: {0} {1}", token,
                                                          right));
      }

      return result;
    }

    private static Expression GetFunction(string function, Expression left, Expression right,
                                          ParameterExpression sourceParameter,
                                          IEnumerable<ParameterExpression> lambdaParameters)
    {
      if (function == null)
        throw new ArgumentNullException("function");

      if (left == null)
        throw new ArgumentNullException("left");

      switch (function.ToLowerInvariant())
      {
        case "substringof":
          return Expression.Call(right, MethodProvider.ContainsMethod, new[] {left});
        case "endswith":
          return Expression.Call(left, MethodProvider.EndsWithMethod, new[] {right, MethodProvider.IgnoreCaseExpression});
        case "startswith":
          return Expression.Call(left, MethodProvider.StartsWithMethod,
                                 new[] {right, MethodProvider.IgnoreCaseExpression});
        case "length":
          return Expression.Property(left, MethodProvider.LengthProperty);
        case "indexof":
          return Expression.Call(left, MethodProvider.IndexOfMethod, new[] {right, MethodProvider.IgnoreCaseExpression});
        case "substring":
          return Expression.Call(left, MethodProvider.SubstringMethod, new[] {right});
        case "tolower":
          return Expression.Call(left, MethodProvider.ToLowerMethod);
        case "toupper":
          return Expression.Call(left, MethodProvider.ToUpperMethod);
        case "trim":
          return Expression.Call(left, MethodProvider.TrimMethod);
        case "hour":
          return Expression.Property(left, MethodProvider.HourProperty);
        case "minute":
          return Expression.Property(left, MethodProvider.MinuteProperty);
        case "second":
          return Expression.Property(left, MethodProvider.SecondProperty);
        case "day":
          return Expression.Property(left, MethodProvider.DayProperty);
        case "month":
          return Expression.Property(left, MethodProvider.MonthProperty);
        case "year":
          return Expression.Property(left, MethodProvider.YearProperty);
        case "round":
          return
            Expression.Call(
              left.Type == typeof (double) ? MethodProvider.DoubleRoundMethod : MethodProvider.DecimalRoundMethod, left);
        case "floor":
          return
            Expression.Call(
              left.Type == typeof (double) ? MethodProvider.DoubleFloorMethod : MethodProvider.DecimalFloorMethod, left);
        case "ceiling":
          return
            Expression.Call(
              left.Type == typeof (double) ? MethodProvider.DoubleCeilingMethod : MethodProvider.DecimalCeilingMethod,
              left);
        case "any":
        case "all":
          {
            return CreateAnyAllExpression(
              left,
              right,
              sourceParameter,
              lambdaParameters,
              MethodProvider.GetAnyAllMethod(function.Capitalize(), left.Type));
          }

        default:
          return null;
      }
    }

    private static Expression CreateAnyAllExpression(
      Expression left,
      Expression right,
      ParameterExpression sourceParameter,
      IEnumerable<ParameterExpression> lambdaParameters,
      MethodInfo anyAllMethod)
    {
      if (left == null)
        throw new ArgumentNullException("left");

      if (right == null)
        throw new ArgumentNullException("right");

      var genericFunc = typeof (Func<,>)
        .MakeGenericType(
          MethodProvider.GetIEnumerableImpl(left.Type).GetGenericArguments()[0],
          typeof (bool));

      var filteredParameters = new ParameterVisitor()
        .GetParameters(right)
        .Where(p => p.Name != sourceParameter.Name)
        .ToArray();

      if (filteredParameters.Length > 0)
      {
        return Expression.Call(
          anyAllMethod,
          left,
          Expression.Lambda(genericFunc, right, filteredParameters));
      }

      return Expression.Call(
        MethodProvider.GetAnyAllMethod("All", left.Type),
        left,
        Expression.Lambda(genericFunc, right, lambdaParameters));
    }

    private static Type GetNonNullableType(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>)
               ? type.GetGenericArguments()[0]
               : type;
    }

    private static bool SupportsNegate(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      type = GetNonNullableType(type);
      if (!type.IsEnum)
      {
        switch (Type.GetTypeCode(type))
        {
          case TypeCode.Int16:
          case TypeCode.Int32:
          case TypeCode.Int64:
          case TypeCode.Double:
          case TypeCode.Single:
            return true;
        }
      }

      return false;
    }

    private Expression CreateExpression<T>(string filter, ParameterExpression sourceParameter,
                                           ICollection<ParameterExpression> lambdaParameters, Type type,
                                           IFormatProvider formatProvider)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (sourceParameter == null)
        throw new ArgumentNullException("sourceParameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      if (filter.IsNullOrWhiteSpace())
      {
        return null;
      }

      var tokens = filter.GetTokens();

      if (tokens.Any())
      {
        return GetTokenExpression<T>(sourceParameter, lambdaParameters, type, formatProvider, tokens);
      }

      if (string.Equals(filter, "null", StringComparison.OrdinalIgnoreCase))
      {
        return Expression.Constant(null);
      }

      var stringMatch = StringRx.Match(filter);

      if (stringMatch.Success)
      {
        return Expression.Constant(stringMatch.Groups[1].Value, typeof (string));
      }

      if (NegateRx.IsMatch(filter))
      {
        var negateExpression = CreateExpression<T>(
          filter.Substring(1),
          sourceParameter,
          lambdaParameters,
          type,
          formatProvider);

        if (SupportsNegate(negateExpression.Type))
        {
          return Expression.Negate(negateExpression);
        }

        throw new InvalidOperationException("Cannot negate " + negateExpression);
      }

      var expression = GetAnyAllFunctionExpression<T>(filter, sourceParameter, lambdaParameters, formatProvider)
                       ?? GetPropertyExpression<T>(filter, sourceParameter, lambdaParameters)
                       ?? GetArithmeticExpression<T>(filter, sourceParameter, lambdaParameters, type, formatProvider)
                       ?? GetFunctionExpression<T>(filter, sourceParameter, lambdaParameters, type, formatProvider);

      if (expression == null)
      {
        if (type != null)
        {
          expression = ParameterValueReader.Read(type, filter, formatProvider);
        }
        else
        {
          var booleanExpression = ParameterValueReader.Read(typeof (bool), filter, formatProvider) as ConstantExpression;
          if (booleanExpression != null && booleanExpression.Value != null)
          {
            expression = booleanExpression;
          }
        }
      }

      if (expression == null)
      {
        throw new InvalidOperationException("Could not create expression from: " + filter);
      }

      return expression;
    }

    private Expression GetTokenExpression<T>(ParameterExpression parameter,
                                             ICollection<ParameterExpression> lambdaParameters, Type type,
                                             IFormatProvider formatProvider, IEnumerable<TokenSet> tokens)
    {
      if (tokens == null)
        throw new ArgumentNullException("tokens");

      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");


      string combiner = null;
      Expression existing = null;
      foreach (var tokenSet in tokens)
      {
        if (tokenSet.Left.IsNullOrWhiteSpace())
        {
          if (string.Equals(tokenSet.Operation, "not", StringComparison.OrdinalIgnoreCase))
          {
            var right = CreateExpression<T>(
              tokenSet.Right,
              parameter,
              lambdaParameters,
              type ?? GetExpressionType<T>(tokenSet, parameter, lambdaParameters),
              formatProvider);

            return right == null
                     ? null
                     : GetOperation(tokenSet.Operation, null, right);
          }

          combiner = tokenSet.Operation;
        }
        else
        {
          var left = CreateExpression<T>(
            tokenSet.Left,
            parameter,
            lambdaParameters,
            type ?? GetExpressionType<T>(tokenSet, parameter, lambdaParameters),
            formatProvider);
          if (left == null)
          {
            return null;
          }

          var rightExpressionType = tokenSet.Operation == "and" ? null : left.Type;
          var right = CreateExpression<T>(tokenSet.Right, parameter, lambdaParameters, rightExpressionType,
                                          formatProvider);

          if (existing != null && !combiner.IsNullOrWhiteSpace())
          {
            var current = right == null ? null : GetOperation(tokenSet.Operation, left, right);
            existing = GetOperation(combiner, existing, current ?? left);
          }
          else if (right != null)
          {
            existing = GetOperation(tokenSet.Operation, left, right);
          }
        }
      }

      return existing;
    }

    private Expression GetArithmeticExpression<T>(string filter, ParameterExpression parameter,
                                                  ICollection<ParameterExpression> lambdaParameters, Type type,
                                                  IFormatProvider formatProvider)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      var arithmeticToken = filter.GetArithmeticToken();
      if (arithmeticToken == null)
      {
        return null;
      }

      var type1 = type ?? GetExpressionType<T>(arithmeticToken, parameter, lambdaParameters);
      var leftExpression = CreateExpression<T>(arithmeticToken.Left, parameter, lambdaParameters, type1, formatProvider);
      var rightExpression = CreateExpression<T>(arithmeticToken.Right, parameter, lambdaParameters, type1,
                                                formatProvider);

      return leftExpression == null || rightExpression == null
               ? null
               : GetLeftRightOperation(arithmeticToken.Operation, leftExpression, rightExpression);
    }

    private Expression GetAnyAllFunctionExpression<T>(string filter, ParameterExpression sourceParameter,
                                                      ICollection<ParameterExpression> lambdaParameters,
                                                      IFormatProvider formatProvider)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (sourceParameter == null)
        throw new ArgumentNullException("sourceParameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      var functionTokens = filter.GetAnyAllFunctionTokens();
      if (functionTokens == null)
      {
        return null;
      }

      var propertyExpression = GetPropertyExpression<T>(functionTokens.Left, sourceParameter, lambdaParameters);
      var leftType = propertyExpression.Type;
      var left = CreateExpression<T>(
        functionTokens.Left,
        sourceParameter,
        lambdaParameters,
        leftType,
        formatProvider);

      // Create a new ParameterExpression from the lambda parameter and add to a collection to pass around
      var parameterName =
        functionTokens.Right.Substring(0, functionTokens.Right.IndexOf(":", System.StringComparison.Ordinal)).Trim();
      var lambdaParameter =
        Expression.Parameter(MethodProvider.GetIEnumerableImpl(leftType).GetGenericArguments()[0], parameterName);
      lambdaParameters.Add(lambdaParameter);
      var lambdaFilter =
        functionTokens.Right.Substring(functionTokens.Right.IndexOf(":", System.StringComparison.Ordinal) + 1).Trim();
      var lambdaType = GetFunctionParameterType(functionTokens.Operation) ?? left.Type;

      var isLambdaAnyAllFunction = lambdaFilter.GetAnyAllFunctionTokens() != null;
      var right = isLambdaAnyAllFunction
                    ? GetAnyAllFunctionExpression<T>(lambdaFilter, lambdaParameter, lambdaParameters, formatProvider)
                    : CreateExpression<T>(lambdaFilter, sourceParameter, lambdaParameters, lambdaType, formatProvider);

      return left == null
               ? null
               : GetFunction(functionTokens.Operation, left, right, sourceParameter, lambdaParameters);
    }

    private Expression GetFunctionExpression<T>(string filter, ParameterExpression sourceParameter,
                                                ICollection<ParameterExpression> lambdaParameters, Type type,
                                                IFormatProvider formatProvider)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (sourceParameter == null)
        throw new ArgumentNullException("sourceParameter");

      if (lambdaParameters == null)
        throw new ArgumentNullException("lambdaParameters");

      var functionTokens = filter.GetFunctionTokens();
      if (functionTokens == null)
      {
        return null;
      }

      var left = CreateExpression<T>(
        functionTokens.Left,
        sourceParameter,
        lambdaParameters,
        type ?? GetExpressionType<T>(functionTokens, sourceParameter, lambdaParameters),
        formatProvider);

      var right = CreateExpression<T>(functionTokens.Right, sourceParameter, lambdaParameters,
                                      GetFunctionParameterType(functionTokens.Operation) ?? left.Type, formatProvider);

      return left == null
               ? null
               : GetFunction(functionTokens.Operation, left, right, sourceParameter, lambdaParameters);
    }

    /// <summary>
    /// Used to get the ParameterExpressions used in an Expression so that Expression.Call will have the correct number of parameters supplied.
    /// </summary>
    private class ParameterVisitor : ExpressionVisitor
    {
      private static readonly string[] AnyAllMethodNames = new[] {"Any", "All"};
      private List<ParameterExpression> m_parameters;

      public IEnumerable<ParameterExpression> GetParameters(Expression expr)
      {
        if (expr == null)
          throw new ArgumentNullException("expr");

        m_parameters = new List<ParameterExpression>();
        Visit(expr);
        return m_parameters;
      }

      public override Expression Visit(Expression node)
      {
        if (null == node)
          return null;

        if (node.NodeType == ExpressionType.Call &&
            AnyAllMethodNames.Contains(((MethodCallExpression) node).Method.Name))
        {
          // Skip the second parameter of the Any/All as this has already been covered
          return base.Visit(((MethodCallExpression) node).Arguments.First());
        }

        return base.Visit(node);
      }

      protected override Expression VisitBinary(BinaryExpression node)
      {
        if (node.NodeType == ExpressionType.AndAlso)
        {
          Visit(node.Left);
          Visit(node.Right);
          return node;
        }

        return base.VisitBinary(node);
      }

      protected override Expression VisitParameter(ParameterExpression p)
      {
        if (!m_parameters.Contains(p))
        {
          m_parameters.Add(p);
        }

        return base.VisitParameter(p);
      }
    }
  }
}