namespace Barista.Bundles
{
  using Barista.Library;
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
      return new MustacheInstance(engine.Object.InstancePrototype);
    }
  }
}
