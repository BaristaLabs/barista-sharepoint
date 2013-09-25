namespace BaristaConsoleClient.Bundles
{
  using System;
  using System.Configuration;
  using Barista;
  using Barista.Jurassic;
  using Barista.Search;
  using Barista.Search.Library;

  [Serializable]
  public class BaristaSearchIndexBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Barista Search Index"; }
    }

    public string BundleDescription
    {
      get { return "Barista Search Index Bundle. Provides Information Retrieval functionality via Lucene."; }
    }

    public object InstallBundle(ScriptEngine engine)
    {
      engine.SetGlobalValue("SearchArguments", new SearchArgumentsConstructor(engine));
      engine.SetGlobalValue("JsonDocument", new JsonDocumentConstructor(engine));

      var searchUrl = ConfigurationManager.AppSettings["Barista_SearchIndexUrl"];
      if (String.IsNullOrWhiteSpace(searchUrl))
        throw new InvalidOperationException("The url of the Search Index must be defined in the app.config under the 'Barista_SearchIndexUrl' app settings key");
      
      return new SearchServiceInstance(engine.Object.InstancePrototype, new BaristaSearchWebClient(searchUrl));
    }
  }
}
