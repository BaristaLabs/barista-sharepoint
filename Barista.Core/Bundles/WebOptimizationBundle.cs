namespace Barista.Bundles
{
  using System;
  using Barista.Library;

  [Serializable]
  public class WebOptimizationBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Web Optimization"; }
    }

    public string BundleDescription
    {
      get { return "Web Optimization Bundle. Provides functionality to help optimize web content. Bundling, minification, sprites, image resizing and so forth."; }
    }

    public virtual object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var bundlerInstance = new WebOptimizationInstance(engine);
      return bundlerInstance;
    }
  }
}
