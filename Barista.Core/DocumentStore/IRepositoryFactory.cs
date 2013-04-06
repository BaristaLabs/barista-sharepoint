namespace Barista.DocumentStore
{
  public interface IRepositoryFactory
  {
    object CreateRepository();

    object CreateRepository(IDocumentStore documentStore);
  }
}
