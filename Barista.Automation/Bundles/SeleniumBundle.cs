namespace Barista.Automation.Bundles
{
  using System;
  using Barista.Automation.Selenium;
  using Barista.Jurassic;

  [Serializable]
  public class SeleniumBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Selenium"; }
    }

    public string BundleDescription
    {
      get { return "Selenium Bundle. Allows the Selenium framework to be executed from Barista."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("By", new ByConstructor(engine));
      engine.SetGlobalValue("Cookie", new CookieConstructor(engine));


      engine.SetGlobalValue("PhantomJSDriver", new PhantomJSDriverConstructor(engine));

      return Null.Value;
    }
  }
}
