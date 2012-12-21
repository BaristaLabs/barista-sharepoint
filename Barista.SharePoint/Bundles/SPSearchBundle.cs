namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Search.Library;
  using System;

  [Serializable]
  public class SPSearchBundle : IBundle
  {
    public string BundleName
    {
      get { return "SPSearch"; }
    }

    public string BundleDescription
    {
      get { return "SPSearch Bundle. Provides search functionality via Lucene within SharePoint."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new SPLuceneInstance(engine.Object.InstancePrototype);
    }
  }
}
