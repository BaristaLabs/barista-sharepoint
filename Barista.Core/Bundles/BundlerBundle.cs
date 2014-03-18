namespace Barista.Bundles
{
  using System;
  using Barista.Library;

  [Serializable]
  public class BundlerBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Bundler"; }
    }

    public string BundleDescription
    {
      get { return "Bundler Bundle. Provides behavior to bundle/minify a collection of files."; }
    }

    public virtual object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var bundlerInstance = new BundlerInstance(engine);

      return bundlerInstance;
    }
  }
}
