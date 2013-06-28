namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.DocumentStore.Library;
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
      var factory = new BaristaRepositoryFactory();
      var repository = Repository.GetRepository(factory, new SPDocumentStore());
      return new RepositoryInstance(engine, repository);
    }

    [Serializable]
    private class BaristaRepositoryFactory : IRepositoryFactory
    {
      public object CreateRepository()
      {
        Repository repository = new Repository();

        //TODO: Allow Repository configuration (JSON Object passed to require as a param)

        return repository;
      }

      public object CreateRepository(IDocumentStore documentStore)
      {
        Repository repository = new Repository(documentStore);

        //TODO: Allow Repository Configuration

        return repository;
      }
    }
  }
}
