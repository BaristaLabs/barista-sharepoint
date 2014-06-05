namespace Barista.SharePoint
{
    using Barista.Bundles;
    using Barista.Library;
    using Barista.Newtonsoft.Json;
    using Barista.SharePoint.Bundles;
    using Barista.SharePoint.Library;
    using Barista.SharePoint.Search.Bundles;
    using Jurassic;
    using Jurassic.Library;
    using System;

    public class SPBaristaScriptEngineFactory : ScriptEngineFactory
    {
        /// <summary>
        /// Returns a new instance of a script engine object with all runtime objects available.
        /// </summary>
        /// <returns></returns>
        public override ScriptEngine GetScriptEngine(WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
        {
            isNewScriptEngineInstance = false;
            errorInInitialization = false;

            //Based on the instancing mode, either retrieve the ScriptEngine from the desired store, or create a new ScriptEngine instance.
            ScriptEngine engine;
            switch (SPBaristaContext.Current.Request.InstanceMode)
            {
                case BaristaInstanceMode.PerCall:
                    //Always create a new instance of the script engine.
                    engine = new ScriptEngine();
                    isNewScriptEngineInstance = true;
                    break;
                case BaristaInstanceMode.Single:
                    engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromRuntimeCache(SPBaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
                    break;
                case BaristaInstanceMode.PerSession:
                    engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromSession(SPBaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
                    break;
                default:
                    throw new NotImplementedException("The instance mode of " + SPBaristaContext.Current.Request.InstanceMode + " is currently not supported.");
            }

            if (SPBaristaContext.Current.Request.ForceStrict)
            {
                engine.ForceStrictMode = true;
            }

            if (isNewScriptEngineInstance)
            {
                var console = new FirebugConsole(engine)
                {
                    Output = new SPBaristaConsoleOutput(engine)
                };

                //Register Bundles.
                var instance = new BaristaSharePointGlobal(engine);

                if (webBundle != null)
                    instance.Common.RegisterBundle(webBundle);

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
                //instance.Common.RegisterBundle(new SharePointSearchBundle());
                instance.Common.RegisterBundle(new SharePointPublishingBundle());
                instance.Common.RegisterBundle(new SharePointContentMigrationBundle());
                instance.Common.RegisterBundle(new SharePointTaxonomyBundle());
                instance.Common.RegisterBundle(new SharePointWorkflowBundle());
                instance.Common.RegisterBundle(new SPActiveDirectoryBundle());
                instance.Common.RegisterBundle(new SPDocumentBundle());
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

                //Global Types
                engine.SetGlobalValue("barista", instance);

                //engine.SetGlobalValue("file", new FileSystemInstance(engine));

                engine.SetGlobalValue("Guid", new GuidConstructor(engine));
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
                if (String.IsNullOrEmpty(SPBaristaContext.Current.Request.InstanceInitializationCode))
                    return engine;

                var initializationScriptSource =
                    new BaristaScriptSource(SPBaristaContext.Current.Request.InstanceInitializationCode,
                        SPBaristaContext.Current.Request.InstanceInitializationCodePath);

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

                    switch (SPBaristaContext.Current.Request.InstanceMode)
                    {
                        case BaristaInstanceMode.Single:
                            BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                                SPBaristaContext.Current.Request.InstanceName);
                            break;
                        case BaristaInstanceMode.PerSession:
                            BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                                SPBaristaContext.Current.Request.InstanceName);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime,
                        "An internal error occured while evaluating script: ");
                    errorInInitialization = true;
                    switch (SPBaristaContext.Current.Request.InstanceMode)
                    {
                        case BaristaInstanceMode.Single:
                            BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                                SPBaristaContext.Current.Request.InstanceName);
                            break;
                        case BaristaInstanceMode.PerSession:
                            BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(
                                SPBaristaContext.Current.Request.InstanceName);
                            break;
                    }
                    throw;
                }
            }

            return engine;
        }
    }
}
