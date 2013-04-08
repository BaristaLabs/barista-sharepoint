namespace Barista.Search.ODataToLucene
{
  using Lucene.Net.Search;

  public interface ISelectFilterFactory
  {
    Filter Create(string select);
  }
}
