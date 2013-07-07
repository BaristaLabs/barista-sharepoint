namespace Barista.SharePoint.DocumentStore
{
  using System;
  using Barista.DocumentStore;
  using Barista.DocumentStore.Library;
  using Barista.Jurassic;
  using RepositoryConfiguration = Barista.DocumentStore.Library.RepositoryConfiguration;

  public class SPRepositoryConstructor : RepositoryConstructor
  {
    public SPRepositoryConstructor(ScriptEngine engine)
      : base(engine)
    {
    }

    protected override RepositoryInstance CreateRepository(RepositoryConfiguration configuration)
    {
      switch (configuration.RepositoryKind)
      {
        default:
          if (configuration.Options.ContainsKey("path") == false)
            throw new InvalidOperationException("FSDocument Store requires a 'path' variable passed as a config option.");

          var factory = new SPDocumentStore();
          var repository = new Repository(factory);
          return new RepositoryInstance(this.Engine, repository);
      }
    }
  }
}
