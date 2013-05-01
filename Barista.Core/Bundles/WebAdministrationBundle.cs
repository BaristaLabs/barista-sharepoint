namespace Barista.Bundles
{
  using Barista.Library;
  using Microsoft.Web.Administration;

  public class WebAdministrationBundle : IBundle
  {
    public string BundleName
    {
      get { return "Web Administration"; }
    }

    public string BundleDescription
    {
      get { return "Web Administration Bundle. Allows administration of IIS Configuration."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var serverManager = new ServerManager();
      return new ServerManagerInstance(engine.Object.InstancePrototype, serverManager);
    }
  }
}
