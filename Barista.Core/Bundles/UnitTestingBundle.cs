namespace Barista.Bundles
{
  using Barista.Jurassic;

  public class UnitTestingBundle :IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Unit Testing"; }
    }

    public string BundleDescription
    {
      get { return "Unit Testing Bundle. Adds components that facilitate unit testing of scripts."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Properties.Resources.chance);
      engine.Execute(Properties.Resources.assert);

      return Null.Value;
    }
  }
}
