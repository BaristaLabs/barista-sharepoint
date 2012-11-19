namespace Barista.Bundles
{
  using Jurassic;
  using System;
  using System.IO;
  using System.Runtime.Serialization.Formatters.Binary;
  using System.Web;
  using System.Web.Caching;

  [Serializable]
  public class LinqBundle : IBundle
  {
    private static StringScriptSource s_linqScriptSource = new StringScriptSource(Barista.Properties.Resources.linq);

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
      engine.Execute(s_linqScriptSource);
      return Null.Value;
    }
  }
}
