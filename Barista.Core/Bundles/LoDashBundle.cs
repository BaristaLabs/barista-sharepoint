namespace Barista.Bundles
{
  using Barista.Jurassic;

  public class LoDashBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Lo-Dash"; }
    }

    public string BundleDescription
    {
      get { return "LoDash Bundle. A utility library delivering consistency, customization, performance, & extras. Based off of Underscore.js. (See http://lodash.com/)"; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(Barista.Properties.Resources.lodash_min);
      return Null.Value;
    }
  }
}
