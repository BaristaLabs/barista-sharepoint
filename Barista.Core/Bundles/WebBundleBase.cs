namespace Barista.Bundles
{
  using Barista.Library;
  using Jurassic;
  using System;

  /// <summary>
  /// Installs the WebInstance implementation.
  /// </summary>
  [Serializable]
  public abstract class WebBundleBase : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }

    }
    public virtual string BundleName
    {
      get { return "Web"; }
    }

    public virtual string BundleDescription
    {
      get { return "Web Bundle. Provides a mechanism to make Ajax calls and query the request and manipulate response of the current context."; }
    }

    public WebInstanceBase WebInstance
    {
      get;
      protected set;
    }

    public object InstallBundle(ScriptEngine engine)
    {
      engine.SetGlobalValue("AjaxSettings", new AjaxSettingsConstructor(engine));
      engine.SetGlobalValue("ProxySettings", new ProxySettingsConstructor(engine));
      engine.SetGlobalValue("Cookie", new CookieConstructor(engine));

      return CreateWebInstance(engine);
    }

    protected abstract WebInstanceBase CreateWebInstance(ScriptEngine engine);
  }
}
