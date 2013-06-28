namespace Barista.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class MomentBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }

    }
    public string BundleName
    {
      get { return "Moment"; }
    }

    public string BundleDescription
    {
      get { return "Moment Bundle. Includes a library that provides extra date/time methods. (See http://momentjs.com/)"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.moment_min);
      return Null.Value;
    }
  }
}
