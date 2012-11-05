namespace Barista.SharePoint.Bundles
{
  using Jurassic;

  public class JsonDataBundle : IBundle
  {
    public string BundleName
    {
      get { return "Json Data"; }
    }

    public string BundleDescription
    {
      get { return "Json Data Bundle. Provides a mechanism to Diff/Merge Json objects."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.jsonDataHandler);
      return Null.Value;
    }
  }
}
