﻿namespace Barista.SharePoint.Bundles
{
  using Barista.DocumentStore;
  using Barista.SharePoint.DocumentStore;
  using Barista.SharePoint.Library;
  using Jurassic;

  public class DocumentStoreBundle : IBundle
  {
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
      var web = BaristaContext.Current.Web;
      var factory = new BaristaRepositoryFactory();
      var repository = Repository.GetRepository(factory, new SPDocumentStore(web));
      return new RepositoryInstance(engine, repository);
    }

    class BaristaRepositoryFactory : IRepositoryFactory
    {
      public Repository CreateRepository()
      {
        Repository repository = new Repository();

        //TODO: Allow Repository configuration (JSON Object passed to require as a param)

        return repository;
      }

      public Repository CreateRepository(IDocumentStore documentStore)
      {
        Repository repository = new Repository(documentStore);

        //TODO: Allow Repository Configuration

        return repository;
      }
    }
  }
}