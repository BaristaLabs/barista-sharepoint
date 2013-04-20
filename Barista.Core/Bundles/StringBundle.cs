namespace Barista.Bundles
{
  using System;

  [Serializable]
  public class StringBundle : IBundle
  {
    public string BundleName
    {
      get { return "String"; }
    }

    public string BundleDescription
    {
      get { return "String Bundle. Includes a library that provides extra string methods. (See http://stringjs.com/)"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return engine.Evaluate(Barista.Properties.Resources._string);
    }
  }
}
