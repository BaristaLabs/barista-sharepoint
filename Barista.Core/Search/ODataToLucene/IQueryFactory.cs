namespace Barista.Search.ODataToLucene
{
  public interface IQueryFactory
  {
    string Create(string query);
  }
}
