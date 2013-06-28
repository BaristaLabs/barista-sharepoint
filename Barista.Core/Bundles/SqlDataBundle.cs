namespace Barista.Bundles
{
  using Barista.Library;
  using Jurassic;
  using System;
  using System.Configuration;

  [Serializable]
  public class SqlDataBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return false; }
    }

    public string BundleName
    {
      get { return "Sql Data"; }
    }

    public string BundleDescription
    {
      get { return "Provides access to SQL Server Databases."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("DynamicModel", new DynamicModelConstructor(engine));

      if (ConfigurationManager.ConnectionStrings.Count > 1)
        return new DynamicModelInstance(engine.Object.InstancePrototype, ConfigurationManager.ConnectionStrings[1].Name);
      return Null.Value;
    }
  }
}
