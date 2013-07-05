namespace Barista.Web.Bundles
{
  using System.IO;
  using Barista.DocumentStore.FileSystem;
  using Barista.DocumentStore.Library;
  using Barista.DocumentStore;
  using System;

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

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var factory = new BaristaRepositoryFactory();
      var rootPath = Path.Combine(BaristaContext.Current.Request.FilePath, "DocumentStore");

      engine.SetGlobalValue("Repository", new RepositoryConstructor(engine));
      var repository = Repository.GetRepository(factory, new FSDocumentStore(rootPath));
      return new RepositoryInstance(engine, repository);
    }

    [Serializable]
    private class BaristaRepositoryFactory : IRepositoryFactory
    {
      public object CreateRepository()
      {
        var repository = new Repository();

        //TODO: Allow Repository configuration (JSON Object passed to require as a param)

        return repository;
      }

      public object CreateRepository(IDocumentStore documentStore)
      {
        var repository = new Repository(documentStore);

        //TODO: Allow Repository Configuration

        return repository;
      }
    }
  }
}
