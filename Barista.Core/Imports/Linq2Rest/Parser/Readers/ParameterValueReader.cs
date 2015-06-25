namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  internal static class ParameterValueReader
  {
    private static readonly IList<IValueExpressionFactory> ExpressionFactories;

    static ParameterValueReader()
    {
      ExpressionFactories = new List<IValueExpressionFactory>
        {
          new BooleanExpressionFactory(),
          new ByteExpressionFactory(),
          new GuidExpressionFactory(),
          new DateTimeExpressionFactory(),
          new TimeSpanExpressionFactory(),
          new DateTimeOffsetExpressionFactory(),
          new DecimalExpressionFactory(),
          new DoubleExpressionFactory(),
          new SingleExpressionFactory(),
          new ByteArrayExpressionFactory(),
          new StreamExpressionFactory(),
          new LongExpressionFactory(),
          new IntExpressionFactory(),
          new ShortExpressionFactory(),
          new UnsignedIntExpressionFactory(),
          new UnsignedLongExpressionFactory(),
          new UnsignedShortExpressionFactory()
        };
    }

    public static Expression Read(Type type, string token, IFormatProvider formatProvider)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (type == null)
        throw new ArgumentNullException("type");

      var factory = ExpressionFactories.FirstOrDefault(x => x.Handles == type);

      return factory == null
               ? GetKnownConstant(type, token, formatProvider)
               : factory.Convert(token);
    }

    private static Expression GetKnownConstant(Type type, string token, IFormatProvider formatProvider)
    {
      if (token == null)
        throw new ArgumentNullException("token");

      if (type == null)
        throw new ArgumentNullException("type");

      if (type.IsEnum)
      {
        var enumValue = Enum.Parse(type, token, true);
        return Expression.Constant(enumValue);
      }

      if (typeof (IConvertible).IsAssignableFrom(type))
      {
        return Expression.Constant(Convert.ChangeType(token, type, formatProvider), type);
      }

      if (type.IsGenericType && typeof (Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
      {
        var genericTypeArgument = type.GetGenericArguments()[0];
        var value = Read(genericTypeArgument, token, formatProvider);
        if (value != null)
        {
          return Expression.Convert(value, type);
        }
      }

      return GetParseExpression(token, formatProvider, type);
    }

    private static Expression GetParseExpression(string filter, IFormatProvider formatProvider, Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      var parseMethods =
        type.GetMethods(BindingFlags.Static | BindingFlags.Public).Where(x => x.Name == "Parse").ToArray();
      if (parseMethods.Length > 0)
      {
        var withFormatProvider =
          parseMethods.FirstOrDefault(
            x =>
              {
                var parameters = x.GetParameters();
                return parameters.Length == 2
                       && typeof (string).IsAssignableFrom(parameters[0].ParameterType)
                       && typeof (IFormatProvider).IsAssignableFrom(parameters[1].ParameterType);
              });
        if (withFormatProvider != null)
        {
          return Expression.Call(withFormatProvider, Expression.Constant(filter), Expression.Constant(formatProvider));
        }

        var withoutFormatProvider = parseMethods.FirstOrDefault(
          x =>
            {
              var parameters = x.GetParameters();
              return parameters.Length == 1
                     && typeof (string).IsAssignableFrom(parameters[0].ParameterType);
            });

        if (withoutFormatProvider != null)
        {
          return Expression.Call(withoutFormatProvider, Expression.Constant(filter));
        }
      }

      return null;
    }
  }
}