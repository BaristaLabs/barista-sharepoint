namespace Barista.Search.OData2Lucene
{
  using Lucene.Net.Search;

  public interface ISortFactory
  {
    Sort Create(string sort);
  }
}
