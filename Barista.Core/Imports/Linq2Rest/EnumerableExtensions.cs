namespace Barista.Linq2Rest
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Stuff
  /// </summary>
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Zip
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(
      this IEnumerable<TFirst> first,
      IEnumerable<TSecond> second,
      Func<TFirst, TSecond, TResult> func)
    {
      var ie1 = first.GetEnumerator();
      var ie2 = second.GetEnumerator();

      while (ie1.MoveNext() && ie2.MoveNext())
        yield return func(ie1.Current, ie2.Current);
    }
  }
}