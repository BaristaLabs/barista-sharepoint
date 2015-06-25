namespace Barista.SharePoint.Bundles
{
  using Barista.Jurassic;
  using Barista.SharePoint.Publishing.Library;
  using System;

  [Serializable]
  public class SharePointPublishingBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "SharePoint Publishing"; }
    }

    public string BundleDescription
    {
      get { return "SharePoint Publishing Bundle. Provides top-level objects to interact with the SharePoint Publishing Infrastructure"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("PublishingCache", new PublishingCacheConstructor(engine));
      engine.SetGlobalValue("PublishingWeb", new PublishingWebConstructor(engine));

      return Null.Value;
    }
  }
}
