namespace Barista.Search.OData2Lucene
{
  using Lucene.Net.Search;

  public interface ISelectFilterFactory
  {
    Filter Create(string select);
  }
}
