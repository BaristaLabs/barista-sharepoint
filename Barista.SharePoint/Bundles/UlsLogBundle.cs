namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Library;

  public class UlsLogBundle : IBundle
  {
    public string BundleName
    {
      get { return "Unified Logging Service"; }
    }

    public string BundleDescription
    {
      get { return "Unified Logging Service Bundle. Provides a mechanism to query the SharePoint ULS Logs."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new LogInstance(engine);
    }
  }
}
