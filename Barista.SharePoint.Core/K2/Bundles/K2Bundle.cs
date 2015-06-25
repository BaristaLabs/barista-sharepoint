namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.K2.Library;
  using System;

  [Serializable]
  public class K2Bundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "K2"; }
    }

    public string BundleDescription
    {
      get { return "K2 Bundle. Provides a mechanism to query K2."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new K2Instance(engine);
    }
  }
}
