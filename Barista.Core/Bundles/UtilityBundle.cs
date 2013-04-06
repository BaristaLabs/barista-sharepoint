namespace Barista.Bundles
{
  using Barista.Library;
  using System;

  [Serializable]
  public class UtilityBundle : IBundle
  {
    public string BundleName
    {
      get { return "Utility"; }
    }

    public string BundleDescription
    {
      get { return "Utility Bundle. Provides utility functions."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new UtilInstance(engine);
    }
  }
}
