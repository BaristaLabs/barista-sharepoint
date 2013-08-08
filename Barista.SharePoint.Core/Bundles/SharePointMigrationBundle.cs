
namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SharePointMigrationBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "SharePoint"; }
    }

    public string BundleDescription
    {
      get { return "SharePoint Migration Bundle. Provides functionality to migrate data between Farms."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      
      //return new SPInstance(engine, SPBaristaContext.Current, SPFarm.Local, SPServer.Local);
    }
  }
}
