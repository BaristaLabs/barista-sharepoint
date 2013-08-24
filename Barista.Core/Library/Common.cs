namespace Barista.Library
{
  using System.Globalization;
  using Barista.Bundles;
  using Jurassic;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Jurassic.Library;
  using Barista.Extensions;

  /// <summary>
  /// Provides functionality similar to CommonJS
  /// </summary>
  [Serializable]
  public class Common : ObjectInstance
  {
    private readonly Dictionary<string, IBundle> m_registeredBundles = new Dictionary<string, IBundle>();
    private readonly Dictionary<IBundle, object> m_installedBundles = new Dictionary<IBundle, object>();

    public Common(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    /// <summary>
    /// Installs the current bundle within the script engine, optionally returning a result.
    /// </summary>
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
        RegisterBundle(bundle);
      }

      if (bundle == null)
        throw new JavaScriptException(this.Engine, "Error",
                                      "A bundle was specified, but the instance of the bundle was null.");

      //Insure that bundles are only installed once.
      if (m_installedBundles.ContainsKey(bundle))
      {
        return m_installedBundles[bundle];
      }

      var result = bundle.InstallBundle(this.Engine);
      m_installedBundles.Add(bundle, result);
      return result;
    }

    [JSFunction(Name = "define")]
    public void Define(object bundleName, object scriptReferences, object bundleDependencies, object bundleDescription, object run)
    {
      var strBundleName = TypeConverter.ToString(bundleName);

      if (bundleName == Undefined.Value || bundleName == Null.Value || strBundleName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "A unique bundle name or type name must be specified as the first argument.");


      if ((scriptReferences == Undefined.Value || scriptReferences == null) &&
          (bundleDependencies == Undefined.Value || bundleDependencies == null) &&
          (bundleDescription == Undefined.Value || bundleDescription == null))
      {
        var strTypeName = TypeConverter.ToString(bundleName);

        var type = Type.GetType(strTypeName, false, true);
        if (type == null)
          throw new JavaScriptException(this.Engine, "Error", String.Format("The specified type could not be located. {0}", strTypeName));

        var bundle = Activator.CreateInstance(type) as IBundle;
        if (bundle == null)
          throw new JavaScriptException(this.Engine, "Error", String.Format("The specified type did not implement the IBundle interface. {0}", strTypeName));

        RegisterBundle(bundle);
        return;
      }

      if (m_registeredBundles.ContainsKey(strBundleName))
        throw new JavaScriptException(this.Engine, "Error", String.Format("Unable to define bundle: A bundle with the specified name has already been registered. {0}", strBundleName));

      var scriptBundle = new ScriptBundle
      {
        BundleName = strBundleName,
      };

      if (bundleDescription != Null.Value && bundleDescription != Undefined.Value)
        scriptBundle.BundleDescription = TypeConverter.ToString(bundleDescription);

      //Add dependencies, first checking the dependency exists, throw if one doesn't.
      if (bundleDependencies is ArrayInstance)
      {
        var arrDependencies = bundleDependencies as ArrayInstance;
        var i = 0;
        foreach (var dependency in arrDependencies.ElementValues)
        {
          var strDependency = TypeConverter.ToString(dependency);
          if (m_registeredBundles.ContainsKey(strDependency) == false)
            throw new JavaScriptException(this.Engine, "Error",
                                          String.Format("The specified dependency does not exist: {0}", strDependency));

          scriptBundle.AddDependency(strDependency, i.ToString(CultureInfo.InvariantCulture));
          i++;
        }
      }
      else if (bundleDependencies is ObjectInstance)
      {
        var objDependencies = bundleDependencies as ObjectInstance;
        foreach (var property in objDependencies.Properties)
        {
          if (m_registeredBundles.ContainsKey(property.Name) == false)
            throw new JavaScriptException(this.Engine, "Error",
                                          String.Format("The specified dependency does not exist: {0}", property.Name));

          scriptBundle.AddDependency(property.Name, TypeConverter.ToString(property.Value));
        }
      }
      else if (bundleDependencies != Null.Value && bundleDependencies != Undefined.Value && bundleDependencies != null)
      {
        var strDependency = TypeConverter.ToString(bundleDependencies);
        if (m_registeredBundles.ContainsKey(strDependency) == false)
          throw new JavaScriptException(this.Engine, "Error",
                                        String.Format("The specified dependency does not exist: {0}", strDependency));
        scriptBundle.AddDependency(strDependency, "0");
      }

      //No need to check existance -- an advanced scenario might be that a script creates another script and only includes it when the bundle is required.
      if (scriptReferences is ArrayInstance)
      {
        var arrScriptReferences = scriptReferences as ArrayInstance;
        foreach (var scriptReference in arrScriptReferences.ElementValues)
        {
          var strScriptReference = TypeConverter.ToString(scriptReference);
          if (scriptReference == Null.Value || scriptReference == Undefined.Value || strScriptReference.IsNullOrWhiteSpace())
            throw new JavaScriptException(this.Engine, "Error", String.Format("Script References must not contain empty strings. ({0})", strBundleName));

          scriptBundle.AddScriptReference(strScriptReference);
        }
      }
      else if (scriptReferences is FunctionInstance)
      {
        scriptBundle.ScriptFunction = scriptReferences as FunctionInstance;
      }
      else
      {
        var strScriptReference = TypeConverter.ToString(scriptReferences);
        if (scriptReferences == Null.Value || scriptReferences == Undefined.Value || strScriptReference.IsNullOrWhiteSpace())
          throw new JavaScriptException(this.Engine, "Error",
                                        String.Format("At least one script reference must be defined in order to define a bundle. ({0})", strBundleName));

        if (run != null && run != Null.Value && run != Undefined.Value && TypeConverter.ToBoolean(run))
        {
          var result = this.Engine.Evaluate("include('" + strScriptReference + "');");
          if (result is FunctionInstance)
          {
            scriptBundle.ScriptFunction = result as FunctionInstance;
          }
          else
          {
            throw new JavaScriptException(this.Engine, "Error",
                                          String.Format("When specifying a to run the script immediately, the eval of the specified script reference should only return a function. ({0})", strBundleName));
          }
        }

        scriptBundle.AddScriptReference(strScriptReference);
      }

      RegisterBundle(scriptBundle);
    }

    /// <summary>
    /// Lists all Bundles currently registered.
    /// </summary>
    /// <returns></returns>
    [JSFunction(Name = "listBundles")]
    public ObjectInstance ListBundles()
    {
      var result = this.Engine.Object.Construct();
      foreach (var bundleName in m_registeredBundles.Keys.OrderBy(k => k))
      {
        var bundle = m_registeredBundles[bundleName];
        if (bundle.IsSystemBundle)
          result.SetPropertyValue(bundleName, "*" + bundle.BundleDescription, false);
        else
          result.SetPropertyValue(bundleName, bundle.BundleDescription, false);
      }
      return result;
    }

    [JSFunction(Name = "listInstalledBundles")]
    public ObjectInstance ListInstalledBundles()
    {
      var result = this.Engine.Object.Construct();
      foreach (var bundle in m_installedBundles.Keys.OrderBy(k => k.BundleName))
      {
        if (bundle.IsSystemBundle)
          result.SetPropertyValue(bundle.BundleName, "*" + bundle.BundleDescription, false);
        else
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
