namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Library;
  using System;

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

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var adInstance = new ActiveDirectoryInstance(engine);
      if (BaristaContext.Current.Web != null)
        adInstance.CurrentUserLoginName = BaristaContext.Current.Web.CurrentUser.LoginName;

      return adInstance;
    }
  }
}
