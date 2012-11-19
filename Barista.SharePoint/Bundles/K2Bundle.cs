namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Library;
  using System;

  [Serializable]
  public class K2Bundle : IBundle
  {
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
