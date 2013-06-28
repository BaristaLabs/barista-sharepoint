namespace Barista.Bundles
{
  using Barista.Jurassic;

  public class SugarBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Sugar"; }
    }

    public string BundleDescription
    {
      get { return "SugarJS Bundle. Includes a library that extends native objects with helpful methods. (See http://sugarjs.com/)"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.sugar_1_3_9_custom_min);
      return Null.Value;
    }
  }
}
