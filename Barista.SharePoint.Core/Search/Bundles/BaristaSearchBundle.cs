namespace Barista.Search.SharePoint.Bundles
{
  using System;
  using Barista.SharePoint.Library;

  [Serializable]
  public class SPSearchBundle : IBundle
  {
    public string BundleName
    {
      get { return "Barista Search"; }
    }

    public string BundleDescription
    {
      get { return "Barista Search Bundle. Provides Information Retrieval functionality via Lucene within SharePoint."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new SPLuceneInstance(engine.Object.InstancePrototype);
    }
  }
}
