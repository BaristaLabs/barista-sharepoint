namespace Barista.SharePoint.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class LinqBundle : IBundle
  {
    public string BundleName
    {
      get { return "Linq"; }
    }

    public string BundleDescription
    {
      get { return "Linq Bundle. Adds objects to allow javascript arrays to be queried via linq-like syntax."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.SharePoint.Properties.Resources.linq);
      return Null.Value;
    }
  }
}
