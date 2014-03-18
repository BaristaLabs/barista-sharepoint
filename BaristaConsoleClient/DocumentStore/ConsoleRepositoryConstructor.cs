namespace BaristaConsoleClient.DocumentStore
{
  using System;
  using Barista.DocumentStore;
  //using Barista.DocumentStore.FileSystem;
  using Barista.DocumentStore.Library;
  using Barista.Jurassic;
  using RepositoryConfiguration = Barista.DocumentStore.Library.RepositoryConfiguration;

  public class ConsoleRepositoryConstructor : RepositoryConstructor
  {
    public ConsoleRepositoryConstructor(ScriptEngine engine)
      : base(engine)
    {
    }

    protected override RepositoryInstance CreateRepository(RepositoryConfiguration configuration)
    {
      switch (configuration.RepositoryKind)
      {
        case "Azure":
          throw new NotImplementedException();
        case "Raven Embedded":
          throw new NotImplementedException();
        case "Raven Client":
          throw new NotImplementedException();
        case "SharePoint":
          throw new NotImplementedException();
        case "SharePoint Client":
          throw new NotImplementedException();
        default:
          if (configuration.Options.ContainsKey("path") == false)
            throw new InvalidOperationException("FSDocument Store requires a 'path' variable passed as a config option.");


          throw new NotImplementedException();
          //var fs = new FSDocumentStore(configuration.Options["path"]);
          //var repository = new Repository(fs);
          //return new RepositoryInstance(this.Engine, repository);
      }
    }
  }
}
