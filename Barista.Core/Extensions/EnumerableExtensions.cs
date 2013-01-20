namespace Barista.Extensions
{
  using System;
  using System.Collections.Generic;

  public static class EnumerableExtensions
  {
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
