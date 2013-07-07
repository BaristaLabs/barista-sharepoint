namespace Barista.SharePoint.Bundles
{
  using Barista.DocumentStore.Library;
  using Barista.DocumentStore;
  using Barista.SharePoint.DocumentStore;
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
      get { return "Document Store Bundle. Enables document database capabilities on top of SharePoint"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("Repository", new SPRepositoryConstructor(engine));

      var factory = new BaristaRepositoryFactory();
      var repository = Repository.GetRepository(factory, new SPDocumentStore());
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
