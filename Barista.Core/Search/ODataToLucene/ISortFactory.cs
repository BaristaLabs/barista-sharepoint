namespace Barista.Search.ODataToLucene
{
  using Lucene.Net.Search;

  public interface ISortFactory
  {
    Sort Create(string sort);
  }
}
