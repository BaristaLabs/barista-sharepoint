namespace Barista.Library
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
  [Serializable]
  public class Common : ObjectInstance
  {
    private Dictionary<string, IBundle> m_registeredBundles = new Dictionary<string, IBundle>();
    private Dictionary<IBundle, object> m_installedBundles = new Dictionary<IBundle, object>();

    public Common(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    /// <summary>
    /// Installs the current bundle within the script engine, optionally returning a result.
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [JSFunction(Name = "require")]
    public object Require(string name)
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
          throw new JavaScriptException(this.Engine, "Error", "The specified bundle does not exist: " + name);

        bundle = Activator.CreateInstance(type) as IBundle;
      }

      //Insure that bundles are only installed once.
      if (m_installedBundles.ContainsKey(bundle))
      {
        return m_installedBundles[bundle];
      }
      else
      {
        var result = bundle.InstallBundle(this.Engine);
        m_installedBundles.Add(bundle, result);
        return result;
      }
    }

    /// <summary>
    /// Lists all Bundles currently registered.
    /// </summary>
    /// <param name="engine"></param>zzz
    /// <returns></returns>
    [JSFunction(Name = "listBundles")]
    public ObjectInstance ListBundles()
    {
      var result = this.Engine.Object.Construct();
      foreach (var bundleName in m_registeredBundles.Keys.OrderBy(k => k))
      {
        result.SetPropertyValue(bundleName, m_registeredBundles[bundleName].BundleDescription, false);
      }
      return result;
    }

    [JSFunction(Name = "listInstalledBundles")]
    public ObjectInstance ListInstalledBundles()
    {
      var result = this.Engine.Object.Construct();
      foreach (var bundle in m_installedBundles.Keys.OrderBy(k => k.BundleName))
      {
        result.SetPropertyValue(bundle.BundleName, bundle.BundleDescription, false);
      }
      return result;
    }

    /// <summary>
    /// Registers the specified bundle (used by the bootstrapper)
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
