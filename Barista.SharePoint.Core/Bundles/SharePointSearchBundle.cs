namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Library;
  using System;

  [Serializable]
  public class SharePointSearchBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "SharePoint Search"; }
    }

    public string BundleDescription
    {
      get { return "SharePoint Search Bundle. Provides top-level objects to interact with SharePoint Search"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new SPSearchInstance(engine.Object.InstancePrototype);
    }
  }
}
