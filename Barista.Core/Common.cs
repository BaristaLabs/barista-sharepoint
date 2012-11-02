namespace Barista
{
  using Jurassic;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Reflection;
  using Jurassic.Library;

  /// <summary>
  /// Provides functionality similar to CommonJS
  /// </summary>
  public class Common
  {
    public Dictionary<string, IBundle> m_registeredBundles = new Dictionary<string, IBundle>();

    /// <summary>
    /// Installs the current bundle within the script engine, optionally returning a result.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public object Require(ScriptEngine engine, string name)
    {
      IBundle bundle;
      if (m_registeredBundles.ContainsKey(name))
      {
        bundle = m_registeredBundles[name];
      }
      else
      {
        var type = Type.GetType(name, false, true);
        if (type == null)
          throw new JavaScriptException(engine, "Error", "The specified bundle does not exist.");

        bundle = Activator.CreateInstance(type) as IBundle;
      }

      return bundle.InstallBundle(engine);
    }

    /// <summary>
    /// Lists all Bundles currently registered.
    /// </summary>
    /// <param name="engine"></param>zzz
    /// <returns></returns>
    public ObjectInstance List(ScriptEngine engine)
    {
      var result = engine.Object.Construct();
      foreach (var bundleName in m_registeredBundles.Keys.OrderBy(k => k))
      {
        result.SetPropertyValue(bundleName, m_registeredBundles[bundleName].BundleDescription, false);
      }
      return result;
    }

    /// <summary>
    /// Registeres the specified bundle (used by the bootstrapper)
    /// </summary>
    /// <param name="bundle"></param>
    public void RegisterBundle(IBundle bundle)
    {
      if (m_registeredBundles.ContainsKey(bundle.BundleName))
        throw new InvalidOperationException("The specified bundle has already been registered.");

      m_registeredBundles.Add(bundle.BundleName, bundle);
    }
  }
}
