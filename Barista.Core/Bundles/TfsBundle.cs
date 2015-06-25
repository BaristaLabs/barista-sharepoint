namespace Barista.Bundles
{
  using System;
  using Barista.TeamFoundationServer.Library;

  [Serializable]
  public class TfsBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Team Foundation Server"; }
    }

    public string BundleDescription
    {
      get { return "Team Foundation Server Bundle. Provides access to TFS Work Item Tracking and Version Control."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new TfsInstance(engine.Object.InstancePrototype);
    }
  }
}
