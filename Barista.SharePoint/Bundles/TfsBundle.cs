namespace Barista.SharePoint.Bundles
{
  using Barista.Library;
  using System;

  [Serializable]
  public class TfsBundle : IBundle
  {
    public string BundleName
    {
      get { return "Tfs"; }
    }

    public string BundleDescription
    {
      get { return "Team Foundation Server Bundle. Provides access to TFS Work Item Tracking and Version Control."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      return new UtilInstance(engine);
    }
  }
}
