namespace Barista.SharePoint
{
    using Barista.Bundles;
    using Barista.Engine;
    using Barista.Library;
    using Barista.Newtonsoft.Json;
    using Barista.SharePoint.Bundles;
    using Barista.SharePoint.Library;
    using Barista.SharePoint.Search.Bundles;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using Barista.DDay.iCal;
    using Ninject;
    using Barista.Extensions;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.SharePoint.Annotations;
    using Ninject.Planning.Bindings;

    public class SPBaristaJurassicScriptEngineFactory : ScriptEngineFactory
    {
        /// <summary>
        /// Returns a new instance of a script engine object with all runtime objects available.
        /// </summary>
        /// <returns></returns>
        public override IScriptEngine GetScriptEngine(WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
        {
            isNewScriptEngineInstance = false;
            errorInInitialization = false;

            //Based on the instancing mode, either retrieve the ScriptEngine from the desired store, or create a new ScriptEngine instance.
            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            ScriptEngine engine;
            switch (instanceSettings.InstanceMode)
            {
                case BaristaInstanceMode.PerCall:
                    //Always create a new instance of the script engine.
                    engine = new ScriptEngine();
                    isNewScriptEngineInstance = true;
                    break;
                case BaristaInstanceMode.Single:
                    engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromRuntimeCache(instanceSettings.InstanceName, out isNewScriptEngineInstance);
                    break;
                case BaristaInstanceMode.PerSession:
                    engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromSession(instanceSettings.InstanceName, out isNewScriptEngineInstance);
                    break;
                default:
                    throw new NotImplementedException("The instance mode of " + instanceSettings.InstanceMode + " is currently not supported.");
            }

            if (SPBaristaContext.Current.Request.ShouldForceStrict())
            {
                engine.ForceStrictMode = true;
            }

            if (!isNewScriptEngineInstance)
                return engine;

            var console = new FirebugConsole(engine)
            {
                Output = new SPBaristaConsoleOutput(engine)
            };

            //Register Bundles.
            var instance = new BaristaSharePointGlobal(engine);

            if (webBundle != null)
                instance.Common.RegisterBundle(webBundle);

            var binDirectory = "";
            if (HttpRuntime.AppDomainAppId != null)
                binDirectory = HttpRuntime.BinDirectory;

            instance.Common.RegisterBundle(new StringBundle());
            instance.Common.RegisterBundle(new SugarBundle());
            instance.Common.RegisterBundle(new SucraloseBundle());
            instance.Common.RegisterBundle(new LoDashBundle());
            instance.Common.RegisterBundle(new SPWebOptimizationBundle());
            instance.Common.RegisterBundle(new MomentBundle());
            instance.Common.RegisterBundle(new MustacheBundle());
            instance.Common.RegisterBundle(new LinqBundle());
            instance.Common.RegisterBundle(new JsonDataBundle());
            instance.Common.RegisterBundle(new SharePointBundle());
            instance.Common.RegisterBundle(new SharePointSearchBundle());
            instance.Common.RegisterBundle(new SharePointPublishingBundle());
            instance.Common.RegisterBundle(new SharePointContentMigrationBundle());
            instance.Common.RegisterBundle(new SharePointTaxonomyBundle());
            instance.Common.RegisterBundle(new SharePointWorkflowBundle());
            instance.Common.RegisterBundle(new SPActiveDirectoryBundle());
            instance.Common.RegisterBundle(new SPDocumentBundle());
            instance.Common.RegisterBundle(new DiagnosticsBundle());
            instance.Common.RegisterBundle(new iCalBundle());
            instance.Common.RegisterBundle(new SmtpBundle());
            instance.Common.RegisterBundle(new K2Bundle());
            instance.Common.RegisterBundle(new UtilityBundle());
            instance.Common.RegisterBundle(new UlsLogBundle());
            instance.Common.RegisterBundle(new DocumentStoreBundle());
            instance.Common.RegisterBundle(new SimpleInheritanceBundle());
            instance.Common.RegisterBundle(new SqlDataBundle());
            instance.Common.RegisterBundle(new StateMachineBundle());
            instance.Common.RegisterBundle(new DeferredBundle());
            instance.Common.RegisterBundle(new TfsBundle());
            instance.Common.RegisterBundle(new BaristaSearchIndexBundle());
            instance.Common.RegisterBundle(new WebAdministrationBundle());
            instance.Common.RegisterBundle(new UnitTestingBundle());
            instance.Common.RegisterBundle(new WkHtmlToPdf.Library.WkHtmlToPdfBundle(binDirectory));

            //Let's do some DI
            var kernel = new StandardKernel();
            BaristaHelper.BindBaristaBundles(kernel, Path.Combine(binDirectory, "bundles"));

            //Let's get information about the approved packages
            var approvedPackages = new Dictionary<string, IList<ApprovedBundleInfo>>();
            try
            {
                var baristaServiceApplication = BaristaHelper.GetCurrentServiceApplication();
                if (baristaServiceApplication.Properties.ContainsKey("BaristaPackageApprovals"))
                {
                    var packageApprovals =
                        Convert.ToString(baristaServiceApplication.Properties["BaristaPackageApprovals"]);
                    if (!packageApprovals.IsNullOrWhiteSpace())
                    {
                        approvedPackages = GetApprovedPackages(JObject.Parse(packageApprovals));
                    }
                }
            }
            catch(Exception)
            {
                //Do Nothing...
            }

            foreach (var bundle in kernel.GetAll<IBundle>(m => IsApprovedPackage(m, approvedPackages)))
            {
                instance.Common.RegisterBundle(bundle);
            }

            //Global Types
            engine.SetGlobalValue("barista", instance);

            //engine.SetGlobalValue("file", new FileSystemInstance(engine));

            engine.SetGlobalValue("Guid", new GuidConstructor(engine));
            engine.SetGlobalValue("HashTable", new HashtableConstructor(engine));
            engine.SetGlobalValue("Uri", new UriConstructor(engine));
            engine.SetGlobalValue("Encoding", new EncodingInstance(engine.Object.InstancePrototype));

            engine.SetGlobalValue("NetworkCredential", new NetworkCredentialConstructor(engine));
            engine.SetGlobalValue("Base64EncodedByteArray", new Base64EncodedByteArrayConstructor(engine));

            engine.SetGlobalValue("console", console);

                
            //If we came from the Barista event receiver, set the appropriate context.
            if (
                SPBaristaContext.Current.Request != null &&
                SPBaristaContext.Current.Request.ExtendedProperties != null &&
                SPBaristaContext.Current.Request.ExtendedProperties.ContainsKey("SPItemEventProperties"))
            {
                var properties =
                    SPBaristaContext.Current.Request.ExtendedProperties["SPItemEventProperties"];

                var itemEventProperties = JsonConvert.DeserializeObject<BaristaItemEventProperties>(properties);
                engine.SetGlobalValue("CurrentItemEventProperties",
                    new BaristaItemEventPropertiesInstance(engine.Object.InstancePrototype, itemEventProperties));
            }

            //Map Barista functions to global functions.
            engine.Execute(@"var help = function(obj) { return barista.help(obj); };
var require = function(name) { return barista.common.require(name); };
var listBundles = function() { return barista.common.listBundles(); };
var define = function() { return barista.common.define(arguments[0], arguments[1], arguments[2], arguments[3]); };
var include = function(scriptUrl) { return barista.include(scriptUrl); };");

            //Execute any instance initialization code.
            if (String.IsNullOrEmpty(instanceSettings.InstanceInitializationCode))
                return engine;

            var initializationScriptSource =
                new StringScriptSource(SPBaristaContext.Current.Request.InstanceInitializationCode, SPBaristaContext.Current.Request.InstanceInitializationCodePath);

            try
            {
                engine.Execute(initializationScriptSource);
            }
            catch (JavaScriptException ex)
            {
                BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException,
                    "A JavaScript exception was thrown while evaluating script: ");
                UpdateResponseWithJavaScriptExceptionDetails(engine, ex, SPBaristaContext.Current.Response);
                errorInInitialization = true;

                switch (instanceSettings.InstanceMode)
                {
                    case BaristaInstanceMode.Single:
                        BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                            instanceSettings.InstanceName);
                        break;
                    case BaristaInstanceMode.PerSession:
                        BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                            instanceSettings.InstanceName);
                        break;
                }
            }
            catch (Exception ex)
            {
                BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime,
                    "An internal error occured while evaluating script: ");
                errorInInitialization = true;
                switch (instanceSettings.InstanceMode)
                {
                    case BaristaInstanceMode.Single:
                        BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                            instanceSettings.InstanceName);
                        break;
                    case BaristaInstanceMode.PerSession:
                        BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                            instanceSettings.InstanceName);
                        break;
                }
                throw;
            }

            return engine;
        }

        private static bool IsApprovedPackage(IBindingMetadata m, Dictionary<string, IList<ApprovedBundleInfo>> approvedPackages)
        {
            var packageId = m.Get<string>("baristaPackageId");
            if (!approvedPackages.ContainsKey(packageId))
                return false;

            var bundleInfo = approvedPackages[packageId];

            var bundleTypeFullName = m.Get<string>("bundleTypeFullName");
            var assemblyPath =  m.Get<string>("assemblyPath");
            var assemblyFullName =  m.Get<string>("assemblyFullName");
            var assemblyHash = m.Get<string>("assemblyHash");

            return
                bundleInfo.Any(
                    bi => string.Equals(bundleTypeFullName, bi.FullTypeName, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(assemblyPath, bi.AssemblyPath, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(assemblyFullName, bi.AssemblyFullName, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(assemblyHash, bi.AssemblyHash, StringComparison.OrdinalIgnoreCase));
        }

        private static Dictionary<string, IList<ApprovedBundleInfo>> GetApprovedPackages(JObject approvedPackages)
        {
            var result = new Dictionary<string, IList<ApprovedBundleInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in approvedPackages.Properties().Where(p => p.Value is JObject))
            {
                var value = prop.Value as JObject;
                if (value == null)
                    continue;

                JToken approvalLevel;
                if (!value.TryGetValue("approvalLevel", out approvalLevel))
                    continue;

                if (!string.Equals(approvalLevel.ToString(), "approved", StringComparison.OrdinalIgnoreCase))
                    continue;

                JToken packageInfo;
                if (!value.TryGetValue("packageInfo", out packageInfo))
                    continue;

                var packageInfoObj = packageInfo as JObject;
                if (packageInfoObj == null)
                    continue;

                JToken bundles;
                if (!packageInfoObj.TryGetValue("bundles", out bundles))
                    continue;

                var approvedBundleInfo = bundles.ToObject<IList<ApprovedBundleInfo>>();

                if (result.ContainsKey(prop.Name))
                    result[prop.Name].AddRange(approvedBundleInfo);
                else
                    result.Add(prop.Name, approvedBundleInfo);
            }

            return result;
        }

        [UsedImplicitly]
        private class ApprovedBundleInfo
        {
            [JsonProperty("bundleTypeFullName")]
            public string FullTypeName
            {
                get;
                set;
            }

            [JsonProperty("assemblyPath")]
            public string AssemblyPath
            {
                get;
                set;
            }

            [JsonProperty("assemblyFullName")]
            public string AssemblyFullName
            {
                get;
                set;
            }

            [JsonProperty("assemblyHash")]
            public string AssemblyHash
            {
                get;
                set;
            }
        }
    }
}
