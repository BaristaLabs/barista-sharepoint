namespace Barista.Web.Bundles
{
  using System;
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

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("SearchArguments", new SearchArgumentsConstructor(engine));
      engine.SetGlobalValue("JsonDocument", new JsonDocumentConstructor(engine));

      var hostUrl = BaristaContext.Current.Request.Url;
      var searchUrl = hostUrl.Scheme + "://" + hostUrl.Host + ":" + hostUrl.Port + "/Barista/v1/Search.svc";

      return new SearchServiceInstance(engine.Object.InstancePrototype, new BaristaSearchWebClient(searchUrl));
    }
  }
}
