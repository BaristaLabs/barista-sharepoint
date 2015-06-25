namespace Barista.Search
{
  using System;

  [Flags]
  public enum AggregationOperation
  {
    None = 0,
    Count = 1,

    Distinct = 1 << 26,
    Dynamic = 1 << 27
  }
}
