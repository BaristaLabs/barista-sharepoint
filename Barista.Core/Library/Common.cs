namespace Barista.Library
{
    using System.Globalization;
    using Barista.Bundles;
    using Jurassic;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Jurassic.Library;
    using Barista.Extensions;

    /// <summary>
    /// Provides functionality similar to CommonJS
    /// </summary>
    [Serializable]
    public class Common : ObjectInstance
    {
        private readonly Dictionary<BundleInfo, IBundle> m_registeredBundles = new Dictionary<BundleInfo, IBundle>();
        private readonly ConcurrentDictionary<IBundle, object> m_installedBundles = new ConcurrentDictionary<IBundle, object>();

        public Common(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public IDictionary<BundleInfo, IBundle> RegisteredBundles
        {
            get
            {
                return m_registeredBundles;
            }
        }

        public IDictionary<IBundle, object> InstalledBundles
        {
            get
            {
                return m_installedBundles;
            }
        }

        /// <summary>
        /// Installs the current bundle within the script engine, optionally returning a result.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        [JSFunction(Name = "require")]
        public object Require(string name, object assemblyName)
        {
            string strAssemblyName = null;
            if (assemblyName != Undefined.Value && assemblyName != Null.Value)
                strAssemblyName = TypeConverter.ToString(assemblyName);

            var bundle = GetBundle(name, strAssemblyName);

            //If we didn't retrieve a bundle, attempt to get a known type by fully-qualified assembly name.
            if (bundle == null)
            {
                var type = Type.GetType(name, false, true);
                if (type == null)
                    throw new JavaScriptException(Engine, "Error", "The specified bundle does not exist: " + name);

                bundle = Activator.CreateInstance(type) as IBundle;
                RegisterBundle(bundle);
            }

            if (bundle == null)
                throw new JavaScriptException(Engine, "Error",
                                              "A bundle was specified, but the instance of the bundle was null.");

            //Insure that bundles are only installed once.
            if (m_installedBundles.ContainsKey(bundle))
                return m_installedBundles[bundle];

            if (m_installedBundles.ContainsKey(bundle))
                return m_installedBundles[bundle];

            var result = bundle.InstallBundle(Engine);
            m_installedBundles.TryAdd(bundle, result);
            return result;
        }

        [JSFunction(Name = "define")]
        public void Define(object bundleName, object scriptReferences, object bundleDependencies, object bundleDescription, object run)
        {
            var strBundleName = TypeConverter.ToString(bundleName);

            if (bundleName == Undefined.Value || bundleName == Null.Value || strBundleName.IsNullOrWhiteSpace())
                throw new JavaScriptException(Engine, "Error", "A unique bundle name or type name must be specified as the first argument.");


            if ((scriptReferences == Undefined.Value || scriptReferences == null) &&
                (bundleDependencies == Undefined.Value || bundleDependencies == null) &&
                (bundleDescription == Undefined.Value || bundleDescription == null))
            {
                var strTypeName = TypeConverter.ToString(bundleName);

                var type = Type.GetType(strTypeName, false, true);
                if (type == null)
                    throw new JavaScriptException(Engine, "Error", string.Format("The specified type could not be located. {0}", strTypeName));

                var bundle = Activator.CreateInstance(type) as IBundle;
                if (bundle == null)
                    throw new JavaScriptException(Engine, "Error", string.Format("The specified type did not implement the IBundle interface. {0}", strTypeName));

                RegisterBundle(bundle);
                return;
            }

            var existingBundle = GetBundle(strBundleName, null);

            if (existingBundle != null)
                throw new JavaScriptException(Engine, "Error", string.Format("Unable to define bundle: A bundle with the specified name has already been registered. {0}", strBundleName));

            var scriptBundle = new ScriptBundle
            {
                BundleName = strBundleName,
            };

            if (bundleDescription != Null.Value && bundleDescription != Undefined.Value)
                scriptBundle.BundleDescription = TypeConverter.ToString(bundleDescription);

            //Add dependencies, first checking the dependency exists, throw if one doesn't.
            var instance = bundleDependencies as ArrayInstance;
            if (instance != null)
            {
                var arrDependencies = instance;
                var i = 0;
                foreach (var dependency in arrDependencies.ElementValues)
                {
                    var strDependency = TypeConverter.ToString(dependency);
                    var existingDependency = GetBundle(strDependency, null);

                    if (existingDependency != null)
                        throw new JavaScriptException(Engine, "Error",
                                                      string.Format("The specified dependency does not exist: {0}", strDependency));

                    scriptBundle.AddDependency(strDependency, i.ToString(CultureInfo.InvariantCulture));
                    i++;
                }
            }
            else
            {
                var dependencies = bundleDependencies as ObjectInstance;
                if (dependencies != null)
                {
                    var objDependencies = dependencies;
                    foreach (var property in objDependencies.Properties)
                    {
                        var existingDependency = GetBundle(property.Name, null);
                        if (existingDependency == null)
                            throw new JavaScriptException(Engine, "Error",
                                string.Format("The specified dependency does not exist: {0}", property.Name));

                        scriptBundle.AddDependency(property.Name, TypeConverter.ToString(property.Value));
                    }
                }
                else if (bundleDependencies != Null.Value && bundleDependencies != Undefined.Value && bundleDependencies != null)
                {
                    var strDependency = TypeConverter.ToString(bundleDependencies);
                    var existingDependency = GetBundle(strDependency, null);
                    if (existingDependency == null)
                        throw new JavaScriptException(Engine, "Error",
                            string.Format("The specified dependency does not exist: {0}", strDependency));

                    scriptBundle.AddDependency(strDependency, "0");
                }
            }

            //No need to check existance -- an advanced scenario might be that a script creates another script and only includes it when the bundle is required.
            var references = scriptReferences as ArrayInstance;
            if (references != null)
            {
                var arrScriptReferences = references;
                foreach (var scriptReference in arrScriptReferences.ElementValues)
                {
                    var strScriptReference = TypeConverter.ToString(scriptReference);
                    if (scriptReference == Null.Value || scriptReference == Undefined.Value || strScriptReference.IsNullOrWhiteSpace())
                        throw new JavaScriptException(Engine, "Error", string.Format("Script References must not contain empty strings. ({0})", strBundleName));

                    scriptBundle.AddScriptReference(strScriptReference);
                }
            }
            else
            {
                var function = scriptReferences as FunctionInstance;
                if (function != null)
                {
                    scriptBundle.ScriptFunction = function;
                }
                else
                {
                    var strScriptReference = TypeConverter.ToString(scriptReferences);
                    if (scriptReferences == Null.Value || scriptReferences == Undefined.Value || strScriptReference.IsNullOrWhiteSpace())
                        throw new JavaScriptException(Engine, "Error",
                            string.Format("At least one script reference must be defined in order to define a bundle. ({0})", strBundleName));

                    if (run != null && run != Null.Value && run != Undefined.Value && TypeConverter.ToBoolean(run))
                    {
                        var result = Engine.Evaluate("include('" + strScriptReference + "');");
                        if (result is FunctionInstance)
                        {
                            scriptBundle.ScriptFunction = result as FunctionInstance;
                        }
                        else
                        {
                            throw new JavaScriptException(Engine, "Error",
                                string.Format("When specifying a to run the script immediately, the eval of the specified script reference should only return a function. ({0})", strBundleName));
                        }
                    }

                    scriptBundle.AddScriptReference(strScriptReference);
                }
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
            var result = Engine.Object.Construct();
            foreach (var bundleInfo in m_registeredBundles.Keys.OrderBy(k => k.BundleName).ThenBy(k => k.AssemblyName.ToString()))
            {
                var bundle = m_registeredBundles[bundleInfo];
                var name = bundleInfo.BundleName;
                var nameSuffix = 0;
                while (result.HasProperty(GetBundleName(name, nameSuffix)))
                    nameSuffix++;

                result.SetPropertyValue(GetBundleName(name, nameSuffix),
                    bundle.IsSystemBundle
                        ? string.Format("* {0} ({1})", bundle.BundleDescription, bundleInfo.AssemblyName)
                        : string.Format("{0} ({1}) ", bundle.BundleDescription, bundleInfo.AssemblyName), false);
            }
            return result;
        }

        private string GetBundleName(string name, int nameSuffix)
        {
            if (nameSuffix < 1)
                return name;
            return string.Format("{0} ({1})", name, nameSuffix);
        }

        [JSFunction(Name = "listInstalledBundles")]
        public ObjectInstance ListInstalledBundles()
        {
            var result = Engine.Object.Construct();
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
            var bundleInfo = new BundleInfo
            {
                BundleName = bundle.BundleName,
                AssemblyName = bundle.GetType().Assembly.GetName()
            };

            if (m_registeredBundles.ContainsKey(bundleInfo))
                throw new InvalidOperationException("The specified bundle has already been registered.");

            m_registeredBundles.Add(bundleInfo, bundle);
        }

        private IBundle GetBundle(string bundleName, string assemblyName)
        {
            //If assemblyName is not specified, get the bundle with the highest assembly version.
            if (assemblyName.IsNullOrWhiteSpace())
            {
                var bundle = m_registeredBundles.Values
                    .Where(b => b.BundleName == bundleName)
                    .OrderByDescending(b => b.GetType().Assembly.GetName(false).Version)
                    .FirstOrDefault();

                return bundle;
            }

            var namedBundle = m_registeredBundles.Values
                   .FirstOrDefault(b => b.BundleName == bundleName && string.Equals(assemblyName, b.GetType().Assembly.GetName(false).ToString(), StringComparison.OrdinalIgnoreCase));

            return namedBundle;
        }

        public sealed class BundleInfo : IEquatable<BundleInfo>
        {
            
            public string BundleName
            {
                get;
                set;
            }

            public AssemblyName AssemblyName
            {
                get;
                set;
            }


            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is BundleInfo && Equals((BundleInfo)obj);
            }

            public bool Equals(BundleInfo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(BundleName, other.BundleName) && Equals(AssemblyName, other.AssemblyName);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((BundleName != null ? BundleName.GetHashCode() : 0) * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
                }
            }

            public static bool operator ==(BundleInfo left, BundleInfo right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(BundleInfo left, BundleInfo right)
            {
                return !Equals(left, right);
            }
        }
    }
}
