namespace Barista.Bundles
{
  using System;
  using Barista.Library;

  /// <summary>
  /// Installs the SharePoint-specific implementation of the Active Directory instance.
  /// </summary>
  [Serializable]
  public class ActiveDirectoryBundle : IBundle
  {
    public string BundleName
    {
      get { return "Active Directory"; }
    }

    public string BundleDescription
    {
      get { return "Active Directory Bundle. Provides a mechanism to query Active Directory."; } 
    }

    public virtual object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var adInstance = new ActiveDirectoryInstance(engine);

      return adInstance;
    }
  }
}
