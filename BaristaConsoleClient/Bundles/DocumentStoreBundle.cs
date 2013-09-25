namespace BaristaConsoleClient.Bundles
{
  using System;
  using System.IO;
  using Barista;
  using Barista.DocumentStore;
  using Barista.DocumentStore.FileSystem;
  using Barista.DocumentStore.Library;
  using Barista.Jurassic;
  using BaristaConsoleClient.DocumentStore;

  [Serializable]
  public class DocumentStoreBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Document Store"; }
    }

    public string BundleDescription
    {
      get { return "Document Store Bundle. Enables document database capabilities on top of FileSystem/RavenDB/Azure etc..."; }
    }

    public object InstallBundle(ScriptEngine engine)
    {
      var factory = new BaristaRepositoryFactory();
      var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "DocumentStore");

      engine.SetGlobalValue("Repository", new ConsoleRepositoryConstructor(engine));
      var repository = Repository.GetRepository(factory, new FSDocumentStore(rootPath));
      return new RepositoryInstance(engine, repository);
    }

    [Serializable]
    private class BaristaRepositoryFactory : IRepositoryFactory
    {
      public object CreateRepository()
      {
        var repository = new Repository();
        return repository;
      }

      public object CreateRepository(IDocumentStore documentStore)
      {
        var repository = new Repository(documentStore);
        return repository;
      }
    }
  }
}
