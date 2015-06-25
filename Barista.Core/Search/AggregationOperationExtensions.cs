namespace Barista.Search
{
  public static class AggregationOperationExtensions
  {
    public static AggregationOperation RemoveOptionals(this AggregationOperation operation)
    {
      return (operation & ~AggregationOperation.Distinct) & ~AggregationOperation.Dynamic;
    }
  }
}
