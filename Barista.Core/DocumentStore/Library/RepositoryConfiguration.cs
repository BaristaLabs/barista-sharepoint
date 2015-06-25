namespace Barista.DocumentStore.Library
{
  using System.Collections.Generic;

  public class RepositoryConfiguration
  {
    public RepositoryConfiguration()
    {
      this.Options = new Dictionary<string, string>();
    }

    public string ContainerName
    {
      get;
      set;
    }

    public string RepositoryKind
    {
      get;
      set;
    }

    public IDictionary<string, string> Options
    {
      get;
      set;
    }
  }
}
