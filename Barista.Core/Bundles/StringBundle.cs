namespace Barista.Bundles
{
  using Jurassic;
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
      get { return "String Bundle. Includes a library that provides extra string methods."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.string_min);
      return Null.Value;
    }
  }
}
