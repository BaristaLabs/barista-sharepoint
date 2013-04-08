namespace Barista.Search.ODataToLucene
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using System.Text.RegularExpressions;
  using Barista.Extensions;
  using Lucene.Net.Search;

  public class QueryFactory : IQueryFactory
  {
    private static readonly Regex StringRx = new Regex(@"^[""'](.*?)[""']$", RegexOptions.Compiled);
    private static readonly Regex NegateRx = new Regex(@"^-[^\d]*", RegexOptions.Compiled);

    /// <summary>
    /// Creates a lucene filter from a odata representation.
    /// </summary>
    /// <param name="query">The odata query representation of the query.</param>
    public string Create(string query)
    {
      if (query.IsNullOrWhiteSpace())
        return null;

      var tokens = query.GetTokens();

      if (tokens.Any())
        return GetTokenFilter(tokens);

      if (string.Equals(query, "null", StringComparison.OrdinalIgnoreCase))
        return null;

      var stringMatch = StringRx.Match(query);

      if (stringMatch.Success)
        return String.Format("{0}", EscapeLuceneSpecialCharacters(stringMatch.Groups[1].Value));

      if (NegateRx.IsMatch(query))
        return String.Format("NOT ({0})", EscapeLuceneSpecialCharacters(query.Substring(1)));

      //var lQuery = GetAnyAllFunctionExpression<T>(filter, sourceParameter, lambdaParameters, formatProvider)
                   //    ?? GetPropertyFilter(query)
                   //    ?? GetArithmeticFilter(query)
      var lQuery = GetFunctionFilter(query);

      if (lQuery.IsNullOrWhiteSpace())
      {
        throw new InvalidOperationException("Could not create query from: " + query);
      }

      return lQuery;
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

    private static string GetPropertyFilter(string propertyToken)
    {
      if (propertyToken == null)
        throw new ArgumentNullException("propertyToken");

      if (!propertyToken.IsImpliedBoolean())
      {
        var token = propertyToken.GetTokens().FirstOrDefault();
        if (token != null)
        {
          return GetPropertyFilter(token.Left) ??
                 GetPropertyFilter(token.Right);
        }
      }

      var propertyChain = propertyToken.Split('/');

      Debug.Assert(propertyChain != null);

      return String.Join(".", propertyChain);
    }

    private static string GetOperation(string token, string left, string right)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (right == null)
        throw new ArgumentNullException("right");

      return left.IsNullOrWhiteSpace() ? GetRightOperation(token, right) : GetLeftRightOperation(token, left, right);
    }

    private static string GetLeftRightOperation(string token, string left, string right)
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
          return String.Format("\"{0}\":\"{1}\"", left, right);
        case "ne":
          return String.Format("NOT \"{0}\":\"{1}\"", left, right);
        case "gt":
          return String.Format("[{0} TO {1}]", left, right);
          //  case "ge":
          //    return Expression.GreaterThanOrEqual(left, right);
        case "lt":
          return String.Format("[{1} TO {0}]", left, right);
          //  case "le":
          //    return Expression.LessThanOrEqual(left, right);
        case "and":
          return String.Format("({0} AND {1})", left, right);
        case "or":
          return String.Format("({0} OR {1})", left, right);
          //  case "add":
          //    return Expression.Add(left, right);
          //  case "sub":
          //    return Expression.Subtract(left, right);
          //  case "mul":
          //    return Expression.Multiply(left, right);
          //  case "div":
          //    return Expression.Divide(left, right);
          //  case "mod":
          //    return Expression.Modulo(left, right);
      }

      throw new InvalidOperationException("Could not understand operation: " + token);
    }

    private static string GetRightOperation(string token, string right)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (right == null)
        throw new ArgumentNullException("right");

      string result = null;
      switch (token.ToLowerInvariant())
      {
        case "not":
          result = right;
          break;
      }

      if (result == null)
      {
        throw new InvalidOperationException(string.Format("Could not create valid expression from: {0} {1}", token,
                                                          right));
      }

      return result;
    }

    private static string GetFunction(string function, string left, string right)
    {
      if (function == null)
        throw new ArgumentNullException("function");

      if (left == null)
        throw new ArgumentNullException("left");

      switch (function.ToLowerInvariant())
      {
          //case "substringof":
          //  return Expression.Call(right, MethodProvider.ContainsMethod, new[] { left });
        case "endswith":
          return String.Format("{0}:*{1}", EscapeLuceneSpecialCharacters(left), EscapeLuceneSpecialCharacters(right));
        case "startswith":
          return String.Format("{0}:{1}*", EscapeLuceneSpecialCharacters(left), EscapeLuceneSpecialCharacters(right));
          //case "length":
          //  return Expression.Property(left, MethodProvider.LengthProperty);
          //case "indexof":
          //  return Expression.Call(left, MethodProvider.IndexOfMethod, new[] { right, MethodProvider.IgnoreCaseExpression });
          //case "substring":
          //  return Expression.Call(left, MethodProvider.SubstringMethod, new[] { right });
          //case "tolower":
          //  return Expression.Call(left, MethodProvider.ToLowerMethod);
          //case "toupper":
          //  return Expression.Call(left, MethodProvider.ToUpperMethod);
          //case "trim":
          //  return Expression.Call(left, MethodProvider.TrimMethod);
          //case "hour":
          //  return Expression.Property(left, MethodProvider.HourProperty);
          //case "minute":
          //  return Expression.Property(left, MethodProvider.MinuteProperty);
          //case "second":
          //  return Expression.Property(left, MethodProvider.SecondProperty);
          //case "day":
          //  return Expression.Property(left, MethodProvider.DayProperty);
          //case "month":
          //  return Expression.Property(left, MethodProvider.MonthProperty);
          //case "year":
          //  return Expression.Property(left, MethodProvider.YearProperty);
          //case "round":
          //  return
          //    Expression.Call(
          //      left.Type == typeof(double) ? MethodProvider.DoubleRoundMethod : MethodProvider.DecimalRoundMethod, left);
          //case "floor":
          //  return
          //    Expression.Call(
          //      left.Type == typeof(double) ? MethodProvider.DoubleFloorMethod : MethodProvider.DecimalFloorMethod, left);
          //case "ceiling":
          //  return
          //    Expression.Call(
          //      left.Type == typeof(double) ? MethodProvider.DoubleCeilingMethod : MethodProvider.DecimalCeilingMethod,
          //      left);
          //case "any":
          //case "all":
          //  {
          //    return CreateAnyAllExpression(
          //      left,
          //      right,
          //      sourceParameter,
          //      lambdaParameters,
          //      MethodProvider.GetAnyAllMethod(function.Capitalize(), left.Type));
          //  }

        default:
          return null;
      }
    }

    private static string CreateAnyAllFilter(
      Filter left,
      Filter right)
    {
      if (left == null)
        throw new ArgumentNullException("left");

      if (right == null)
        throw new ArgumentNullException("right");


      throw new NotImplementedException();
      //var filteredParameters = new ParameterVisitor()
      //  .GetParameters(right)
      //  .Where(p => p.Name != sourceParameter.Name)
      //  .ToArray();

      //if (filteredParameters.Length > 0)
      //{
      //  return Expression.Call(
      //    anyAllMethod,
      //    left,
      //    Expression.Lambda(genericFunc, right, filteredParameters));
      //}

      //return Expression.Call(
      //  MethodProvider.GetAnyAllMethod("All", left.Type),
      //  left,
      //  Expression.Lambda(genericFunc, right, lambdaParameters));
    }

    private string GetTokenFilter(IEnumerable<TokenSet> tokens)
    {
      if (tokens == null)
        throw new ArgumentNullException("tokens");

      string combiner = null;
      string existing = String.Empty;
      foreach (var tokenSet in tokens)
      {
        if (tokenSet.Left.IsNullOrWhiteSpace())
        {
          if (string.Equals(tokenSet.Operation, "not", StringComparison.OrdinalIgnoreCase))
          {
            var right = Create(tokenSet.Right);

            return right == null
                     ? null
                     : GetOperation(tokenSet.Operation, null, right);
          }

          combiner = tokenSet.Operation;
        }
        else
        {
          var left = Create(tokenSet.Left);

          if (left == null)
          {
            return null;
          }

          //var rightExpressionType = tokenSet.Operation == "and" ? null : left.Type;
          var right = Create(tokenSet.Right);

          if (!existing.IsNullOrWhiteSpace() && !combiner.IsNullOrWhiteSpace())
          {
            var current = right.IsNullOrWhiteSpace() ? null : GetOperation(tokenSet.Operation, left, right);
            existing = GetOperation(combiner, existing, current ?? left);
          }
          else if (right.IsNullOrWhiteSpace() == false)
          {
            existing = GetOperation(tokenSet.Operation, left, right);
          }
        }
      }

      return existing;
    }

    private string GetArithmeticFilter(string filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      var arithmeticToken = filter.GetArithmeticToken();

      if (arithmeticToken == null)
        return null;

      var leftExpression = Create(arithmeticToken.Left);
      var rightExpression = Create(arithmeticToken.Right);

      return leftExpression == null || rightExpression == null
               ? null
               : GetLeftRightOperation(arithmeticToken.Operation, leftExpression, rightExpression);
    }

    private string GetAnyAllFunctionFilter(string filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      var functionTokens = filter.GetAnyAllFunctionTokens();
      if (functionTokens == null)
        return null;

      var left = Create(functionTokens.Left);

      throw new NotImplementedException();
      //// Create a new ParameterExpression from the lambda parameter and add to a collection to pass around
      //var parameterName =
      //  functionTokens.Right.Substring(0, functionTokens.Right.IndexOf(":", System.StringComparison.Ordinal)).Trim();
      //var lambdaParameter =
      //  Expression.Parameter(MethodProvider.GetIEnumerableImpl(leftType).GetGenericArguments()[0], parameterName);
      //lambdaParameters.Add(lambdaParameter);
      //var lambdaFilter =
      //  functionTokens.Right.Substring(functionTokens.Right.IndexOf(":", System.StringComparison.Ordinal) + 1).Trim();
      //var lambdaType = GetFunctionParameterType(functionTokens.Operation) ?? left.Type;

      //var isLambdaAnyAllFunction = lambdaFilter.GetAnyAllFunctionTokens() != null;
      //var right = isLambdaAnyAllFunction
      //              ? GetAnyAllFunctionExpression<T>(lambdaFilter, lambdaParameter, lambdaParameters, formatProvider)
      //              : CreateExpression<T>(lambdaFilter, sourceParameter, lambdaParameters, lambdaType, formatProvider);

      //return left == null
      //         ? null
      //         : GetFunction(functionTokens.Operation, left, right, sourceParameter, lambdaParameters);
    }

    private string GetFunctionFilter(string filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      var functionTokens = filter.GetFunctionTokens();
      if (functionTokens == null)
      {
        return null;
      }

      var left = functionTokens.Left; //Not sure if nested expressions are possible here.

      var right = Create(functionTokens.Right);

      return left == null
               ? null
               : GetFunction(functionTokens.Operation, left, right);
    }

    private static string EscapeLuceneSpecialCharacters(string unescapedString)
    {
      var luceneSpecialCharacters = new[]
        {
          "+",
          "-",
          "&&",
          "||",
          "!",
          "(",
          ")",
          "{",
          "}",
          "[",
          "]",
          "^",
          "\"",
          "~",
          "*",
          "?",
          ":",
          "\\\\"
        };

      //TODO: Make me better.
      var sb = new StringBuilder(unescapedString);
      foreach (var lsc in luceneSpecialCharacters)
      {
        sb.Replace(lsc, "\\" + lsc);
      }
      return sb.ToString();
    }
  }
}
