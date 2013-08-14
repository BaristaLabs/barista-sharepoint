namespace Barista.SharePoint.Bundles
{
  using Barista.SharePoint.Migration.Library;
  using System;

  [Serializable]
  public class SharePointContentMigrationBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "SharePoint Content Migration"; }
    }

    public string BundleDescription
    {
      get { return "SharePoint Content Migration Bundle. Provides functionality to perform selective content migration."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      //Add the Various types required.
      engine.SetGlobalValue("SPExportSettings", new SPExportSettingsConstructor(engine));
      engine.SetGlobalValue("SPExportObject", new SPExportObjectConstructor(engine));
      engine.SetGlobalValue("SPExportObjectCollection", new SPExportObjectCollectionConstructor(engine));

      return new SPMigrationInstance(engine.Object.InstancePrototype);
    }
  }
}
