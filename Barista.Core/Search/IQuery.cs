namespace Barista.Search
{
  public interface IQuery<T>
    where T : Query
  {
    T Query
    {
      get;
    }
  }
}
