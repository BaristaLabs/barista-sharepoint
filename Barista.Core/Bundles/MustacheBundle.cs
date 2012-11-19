namespace Barista.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class MustacheBundle : IBundle
  {
    public string BundleName
    {
      get { return "Mustache"; }
    }

    public string BundleDescription
    {
      get { return "Mustache Bundle. Includes the Mustache templating engine."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.mustache);
      return Null.Value;
    }
  }
}
