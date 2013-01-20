namespace Barista.Linq2Rest.Provider
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using Barista.Extensions;
  using Linq2Rest.Provider.Writers;

  internal class ExpressionWriter : IExpressionWriter
  {
    private static readonly ExpressionType[] CompositeExpressionTypes = new[]
      {ExpressionType.Or, ExpressionType.OrElse, ExpressionType.And, ExpressionType.AndAlso};

    public string Write(Expression expression)
    {
      return expression == null ? null : Write(expression, expression.Type, GetRootParameterName(expression));
    }

    private static Type GetUnconvertedType(Expression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      switch (expression.NodeType)
      {
        case ExpressionType.Convert:
          var unaryExpression = expression as UnaryExpression;

          return unaryExpression.Operand.Type;
        default:
          return expression.Type;
      }
    }

    private static string GetMemberCall(MemberExpression memberExpression)
    {
      if (memberExpression == null)
        throw new ArgumentNullException("memberExpression");

      var declaringType = memberExpression.Member.DeclaringType;
      var name = memberExpression.Member.Name;

      if (declaringType == typeof (string))
      {
        if (name == "Length")
        {
          return name.ToLowerInvariant();
        }
      }
      else if (declaringType == typeof (DateTime))
      {
        switch (name)
        {
          case "Hour":
          case "Minute":
          case "Second":
          case "Day":
          case "Month":
          case "Year":
            return name.ToLowerInvariant();
        }
      }

      return string.Empty;
    }

    private static Expression CollapseCapturedOuterVariables(MemberExpression input)
    {
      if (input == null || input.NodeType != ExpressionType.MemberAccess)
      {
        return input;
      }

      switch (input.Expression.NodeType)
      {
        case ExpressionType.New:
        case ExpressionType.MemberAccess:
          var value = GetValue(input);
          return Expression.Constant(value);
        case ExpressionType.Constant:
          var obj = ((ConstantExpression) input.Expression).Value;
          if (obj == null)
          {
            return input;
          }

          var fieldInfo = input.Member as FieldInfo;
          if (fieldInfo != null)
          {
            var result = fieldInfo.GetValue(obj);
            return result is Expression ? (Expression) result : Expression.Constant(result);
          }

          var propertyInfo = input.Member as PropertyInfo;
          if (propertyInfo != null)
          {
            var result = propertyInfo.GetValue(obj, null);
            return result is Expression ? (Expression) result : Expression.Constant(result);
          }

          break;
      }

      return input;
    }

    private static object GetValue(Expression input)
    {
      if (input == null)
        throw new ArgumentNullException("input");

      var objectMember = Expression.Convert(input, typeof (object));
      var getterLambda = Expression.Lambda<Func<object>>(objectMember).Compile();

      return getterLambda();
    }

    private static bool IsMemberOfParameter(MemberExpression input)
    {
      if (input == null || input.Expression == null)
      {
        return false;
      }

      var nodeType = input.Expression.NodeType;
      var tempExpression = input.Expression as MemberExpression;
      while (nodeType == ExpressionType.MemberAccess)
      {
        if (tempExpression == null || tempExpression.Expression == null)
        {
          return false;
        }

        nodeType = tempExpression.Expression.NodeType;
        tempExpression = tempExpression.Expression as MemberExpression;
      }

      return nodeType == ExpressionType.Parameter;
    }

    private static string GetOperation(Expression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      switch (expression.NodeType)
      {
        case ExpressionType.Add:
          return "add";
        case ExpressionType.AddChecked:
          break;
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return "and";
        case ExpressionType.Divide:
          return "div";
        case ExpressionType.Equal:
          return "eq";
        case ExpressionType.GreaterThan:
          return "gt";
        case ExpressionType.GreaterThanOrEqual:
          return "ge";
        case ExpressionType.LessThan:
          return "lt";
        case ExpressionType.LessThanOrEqual:
          return "le";
        case ExpressionType.Modulo:
          return "mod";
        case ExpressionType.Multiply:
          return "mul";
        case ExpressionType.Not:
          return "not";
        case ExpressionType.NotEqual:
          return "ne";
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return "or";
        case ExpressionType.Subtract:
          return "sub";
      }

      return string.Empty;
    }

    private static string GetRootParameterName(Expression expression)
    {
      if (expression is UnaryExpression)
      {
        expression = ((UnaryExpression) expression).Operand;
      }

      if (expression is LambdaExpression && ((LambdaExpression) expression).Parameters.Any())
      {
        return ((LambdaExpression) expression).Parameters.First().Name;
      }

      return null;
    }

    private static string Write(Expression expression, string rootParameterName)
    {
      return expression == null ? null : Write(expression, expression.Type, rootParameterName);
    }

    private static string GetMethodCall(MethodCallExpression expression, string rootParameterName)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var methodName = expression.Method.Name;
      var declaringType = expression.Method.DeclaringType;

      if (methodName == "Equals")
      {
        return string.Format(
          "{0} eq {1}",
          Write(expression.Object, rootParameterName),
          Write(expression.Arguments[0], rootParameterName));
      }

      if (declaringType == typeof (string))
      {
        var obj = expression.Object;

        switch (methodName)
        {
          case "Replace":
            {
              var firstArgument = expression.Arguments[0];
              var secondArgument = expression.Arguments[1];

              return string.Format(
                "replace({0}, {1}, {2})",
                Write(obj, rootParameterName),
                Write(firstArgument, rootParameterName),
                Write(secondArgument, rootParameterName));
            }

          case "Trim":
            return string.Format("trim({0})", Write(obj, rootParameterName));
          case "ToLower":
          case "ToLowerInvariant":
            return string.Format("tolower({0})", Write(obj, rootParameterName));
          case "ToUpper":
          case "ToUpperInvariant":
            return string.Format("toupper({0})", Write(obj, rootParameterName));
          case "Substring":
            {
              if (expression.Arguments.Count == 1)
              {
                var argumentExpression = expression.Arguments[0];

                return string.Format(
                  "substring({0}, {1})", Write(obj, rootParameterName), Write(argumentExpression, rootParameterName));
              }

              var firstArgument = expression.Arguments[0];
              var secondArgument = expression.Arguments[1];

              return string.Format(
                "substring({0}, {1}, {2})",
                Write(obj, rootParameterName),
                Write(firstArgument, rootParameterName),
                Write(secondArgument, rootParameterName));
            }

          case "Contains":
            {
              var argumentExpression = expression.Arguments[0];

              return string.Format(
                "substringof({0}, {1})",
                Write(argumentExpression, rootParameterName),
                Write(obj, rootParameterName));
            }

          case "IndexOf":
            {
              var argumentExpression = expression.Arguments[0];
              return string.Format("indexof({0}, {1})", Write(obj, rootParameterName),
                                   Write(argumentExpression, rootParameterName));
            }

          case "EndsWith":
            {
              var argumentExpression = expression.Arguments[0];
              return string.Format("endswith({0}, {1})", Write(obj, rootParameterName),
                                   Write(argumentExpression, rootParameterName));
            }

          case "StartsWith":
            {
              var argumentExpression = expression.Arguments[0];
              return string.Format("startswith({0}, {1})", Write(obj, rootParameterName),
                                   Write(argumentExpression, rootParameterName));
            }
        }
      }
      else if (declaringType == typeof (Math))
      {
        var mathArgument = expression.Arguments[0];

        switch (methodName)
        {
          case "Round":
            return string.Format("round({0})", Write(mathArgument, rootParameterName));
          case "Floor":
            return string.Format("floor({0})", Write(mathArgument, rootParameterName));
          case "Ceiling":
            return string.Format("ceiling({0})", Write(mathArgument, rootParameterName));
        }
      }

      if (expression.Method.Name == "Any" || expression.Method.Name == "All")
      {
        var firstArg = Write(expression.Arguments[0], rootParameterName);
        var method = expression.Method.Name.ToLowerInvariant();
        var parameter = expression.Arguments[1] is LambdaExpression
                          ? (expression.Arguments[1] as LambdaExpression).Parameters.First().Name
                          : null;
        var predicate = Write(expression.Arguments[1], rootParameterName);

        return string.Format("{0}/{1}({2}: {3})", firstArg, method, parameter, predicate);
      }

      return ParameterValueWriter.Write(GetValue(expression));
    }

    private static string Write(Expression expression, Type type, string rootParameterName)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      if (type == null)
        throw new ArgumentNullException("type");

      switch (expression.NodeType)
      {
        case ExpressionType.Parameter:
          var parameterExpression = expression as ParameterExpression;
          return parameterExpression.Name;
        case ExpressionType.Constant:
          {
            var value = GetValue(Expression.Convert(expression, type));
            return ParameterValueWriter.Write(value);
          }

        case ExpressionType.Add:
        case ExpressionType.And:
        case ExpressionType.AndAlso:
        case ExpressionType.Divide:
        case ExpressionType.Equal:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
        case ExpressionType.LessThan:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.Modulo:
        case ExpressionType.Multiply:
        case ExpressionType.NotEqual:
        case ExpressionType.Or:
        case ExpressionType.OrElse:
        case ExpressionType.Subtract:
          {
            var binaryExpression = expression as BinaryExpression;
            var operation = GetOperation(binaryExpression);

            if (binaryExpression.Left.NodeType == ExpressionType.Call)
            {
              var compareResult = ResolveCompareToOperation(rootParameterName,
                                                            (MethodCallExpression) binaryExpression.Left, operation,
                                                            binaryExpression.Right as ConstantExpression);
              if (compareResult != null)
              {
                return compareResult;
              }
            }

            if (binaryExpression.Right.NodeType == ExpressionType.Call)
            {
              var compareResult = ResolveCompareToOperation(rootParameterName,
                                                            (MethodCallExpression) binaryExpression.Right, operation,
                                                            binaryExpression.Left as ConstantExpression);
              if (compareResult != null)
              {
                return compareResult;
              }
            }

            var isLeftComposite = CompositeExpressionTypes.Any(x => x == binaryExpression.Left.NodeType);
            var isRightComposite = CompositeExpressionTypes.Any(x => x == binaryExpression.Right.NodeType);

            var leftType = GetUnconvertedType(binaryExpression.Left);
            var leftString = Write(binaryExpression.Left, rootParameterName);
            var rightString = Write(binaryExpression.Right, leftType, rootParameterName);

            return string.Format(
              "{0} {1} {2}",
              string.Format(isLeftComposite ? "({0})" : "{0}", leftString),
              operation,
              string.Format(isRightComposite ? "({0})" : "{0}", rightString));
          }

        case ExpressionType.Negate:
          {
            var unaryExpression = expression as UnaryExpression;

            var operand = unaryExpression.Operand;

            return string.Format("-{0}", Write(operand, rootParameterName));
          }

        case ExpressionType.Not:
          {
            var unaryExpression = expression as UnaryExpression;

            var operand = unaryExpression.Operand;

            return string.Format("not({0})", Write(operand, rootParameterName));
          }
        case ExpressionType.Convert:
        case ExpressionType.Quote:
          {
            var unaryExpression = expression as UnaryExpression;

            var operand = unaryExpression.Operand;
            return Write(operand, rootParameterName);
          }

        case ExpressionType.MemberAccess:
          {
            var memberExpression = expression as MemberExpression;

            if (memberExpression.Expression == null)
            {
              var memberValue = GetValue(memberExpression);
              return ParameterValueWriter.Write(memberValue);
            }

            var pathPrefixes = new List<string>();

            var currentMemberExpression = memberExpression;
            while (currentMemberExpression != null)
            {
              pathPrefixes.Add(currentMemberExpression.Member.Name);
              if (currentMemberExpression.Expression is ParameterExpression &&
                  ((ParameterExpression) currentMemberExpression.Expression).Name != rootParameterName)
              {
                pathPrefixes.Add(((ParameterExpression) currentMemberExpression.Expression).Name);
              }

              currentMemberExpression = currentMemberExpression.Expression as MemberExpression;
            }

            pathPrefixes.Reverse();
            var prefix = pathPrefixes.Join("/");

            if (!IsMemberOfParameter(memberExpression))
            {
              var collapsedExpression = CollapseCapturedOuterVariables(memberExpression);
              if (!(collapsedExpression is MemberExpression))
              {
                return Write(collapsedExpression, rootParameterName);
              }

              memberExpression = (MemberExpression) collapsedExpression;
            }

            var memberCall = GetMemberCall(memberExpression);

            var innerExpression = memberExpression.Expression;

            return memberCall.IsNullOrWhiteSpace()
                     ? prefix
                     : string.Format("{0}({1})", memberCall, Write(innerExpression, rootParameterName));
          }

        case ExpressionType.Call:
          var methodCallExpression = expression as MethodCallExpression;
          return GetMethodCall(methodCallExpression, rootParameterName);
        case ExpressionType.New:
          var newValue = GetValue(expression);
          return ParameterValueWriter.Write(newValue);
        case ExpressionType.Lambda:
          var lambdaExpression = expression as LambdaExpression;
          var body = lambdaExpression.Body;
          return Write(body, rootParameterName);
        default:
          throw new InvalidOperationException("Expression is not recognized or supported");
      }
    }

    private static string ResolveCompareToOperation(
      string rootParameterName,
      MethodCallExpression methodCallExpression,
      string operation,
      ConstantExpression comparisonExpression)
    {
      if (methodCallExpression != null
          && methodCallExpression.Method.Name == "CompareTo"
          && methodCallExpression.Method.ReturnType == typeof (int)
          && comparisonExpression != null
          && Equals(comparisonExpression.Value, 0))
      {
        return string.Format(
          "{0} {1} {2}",
          Write(methodCallExpression.Object, rootParameterName),
          operation,
          Write(methodCallExpression.Arguments[0], rootParameterName));
      }

      return null;
    }
  }
}