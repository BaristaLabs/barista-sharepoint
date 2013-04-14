namespace Barista.SharePoint.Search.Bundles
{
  using System;
  using Barista.SharePoint.Search.Library;

  [Serializable]
  public class BaristaSearchIndexBundle : IBundle
  {
    public string BundleName
    {
      get { return "Barista Search Index"; }
    }

    public string BundleDescription
    {
      get { return "Barista Search Index Bundle. Provides Information Retrieval functionality via Lucene within SharePoint."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("JsonDocument", new JsonDocumentConstructor(engine));

      return new SPBaristaSearchServiceInstance(engine.Object.InstancePrototype, new SPBaristaSearchServiceProxy());
    }
  }
}
