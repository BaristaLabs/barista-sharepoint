namespace Barista.Linq2Rest
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;

  internal static class GeneralExtensions
  {
    public static bool IsAnonymousType(this Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      return Attribute.IsDefined(type, typeof (CompilerGeneratedAttribute), false)
             && type.IsGenericType
             && type.Name.Contains("AnonymousType") && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
             && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
    }

    public static string Capitalize(this string input)
    {
      if (String.IsNullOrEmpty(input))
        throw new ArgumentOutOfRangeException("input");

      return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }

    public static Stream ToStream(this string input)
    {
      if (input == null)
        throw new ArgumentNullException("input");

      return new MemoryStream(Encoding.UTF8.GetBytes(input));
    }

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, Expression keySelector)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      var propertyType = keySelector.GetType().GetGenericArguments()[0].GetGenericArguments()[1];
      var orderbyMethod =
        typeof (Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .FirstOrDefault(x => x.Name == "OrderBy" && x.GetParameters().Length == 2);

      orderbyMethod = orderbyMethod.MakeGenericMethod(typeof (T), propertyType);

      return (IOrderedQueryable<T>) orderbyMethod.Invoke(null, new object[] {source, keySelector});
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, Expression keySelector)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      var propertyType = keySelector.GetType().GetGenericArguments()[0].GetGenericArguments()[1];
      var orderbyMethod =
        typeof (Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .FirstOrDefault(x => x.Name == "OrderByDescending" && x.GetParameters().Length == 2);

      orderbyMethod = orderbyMethod.MakeGenericMethod(typeof (T), propertyType);

      return (IOrderedQueryable<T>) orderbyMethod.Invoke(null, new object[] {source, keySelector});
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, Expression keySelector)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      var propertyType = keySelector.GetType().GetGenericArguments()[0].GetGenericArguments()[1];
      var orderbyMethod =
        typeof (Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .FirstOrDefault(x => x.Name == "ThenBy" && x.GetParameters().Length == 2);

      orderbyMethod = orderbyMethod.MakeGenericMethod(typeof (T), propertyType);

      return (IOrderedQueryable<T>) orderbyMethod.Invoke(null, new object[] {source, keySelector});
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, Expression keySelector)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (keySelector == null)
        throw new ArgumentNullException("keySelector");

      var propertyType = keySelector.GetType().GetGenericArguments()[0].GetGenericArguments()[1];
      var orderbyMethod =
        typeof (Queryable).GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .FirstOrDefault(x => x.Name == "ThenByDescending" && x.GetParameters().Length == 2);

      orderbyMethod = orderbyMethod.MakeGenericMethod(typeof (T), propertyType);

      return (IOrderedQueryable<T>) orderbyMethod.Invoke(null, new object[] {source, keySelector});
    }
  }
}