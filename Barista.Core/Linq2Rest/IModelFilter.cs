namespace Barista.Linq2Rest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  /// Defines the public interface for a model filter.
  /// </summary>
  /// <typeparam name="T">The <see cref="Type"/> of item to filter.</typeparam>
  public interface IModelFilter<T>
  {
    /// <summary>
    /// Filters the passed collection with the defined filter.
    /// </summary>
    /// <param name="source">The source items to filter.</param>
    /// <returns>A filtered enumeration and projected of the source items.</returns>
    IQueryable<object> Filter(IEnumerable<T> source);
  }

  internal abstract class ModelFilterContracts<T> : IModelFilter<T>
  {
    public IQueryable<object> Filter(IEnumerable<T> source)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      throw new NotImplementedException();
    }
  }
}