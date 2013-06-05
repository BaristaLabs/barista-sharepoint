﻿namespace Barista
{
  using Barista.Bundles;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using System;

  public sealed class BaristaScriptEngineFactory : ScriptEngineFactory
  {
    public override Jurassic.ScriptEngine GetScriptEngine(Bundles.WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
    {
      isNewScriptEngineInstance = false;
      errorInInitialization = false;

      //Based on the instancing mode, either retrieve the ScriptEngine from the desired store, or create a new ScriptEngine instance.
      ScriptEngine engine;
      switch (BaristaContext.Current.Request.InstanceMode)
      {
        case BaristaInstanceMode.PerCall:
          //Always create a new instance of the script engine.
          engine = new ScriptEngine();
          isNewScriptEngineInstance = true;
          break;
        //case BaristaInstanceMode.Single:
        //  engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
        //  break;
        //case BaristaInstanceMode.PerSession:
        //  engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromSession(BaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
        //  break;
        default:
          throw new NotImplementedException("The instance mode of " + BaristaContext.Current.Request.InstanceMode + " is currently not supported.");
      }

      if (BaristaContext.Current.Request.ForceStrict)
      {
        engine.ForceStrictMode = true;
      }

      if (isNewScriptEngineInstance)
      {
        //TODO: Add a console that uses NUnit
        //var console = new FirebugConsole(engine)
        //{
        //  Output = new BaristaConsoleOutput(engine)
        //};

        //Register Bundles.
        var instance = new BaristaGlobal(engine.Object.InstancePrototype);

        instance.Common.RegisterBundle(webBundle);
        instance.Common.RegisterBundle(new StringBundle());
        instance.Common.RegisterBundle(new SugarBundle());
        instance.Common.RegisterBundle(new MomentBundle());
        instance.Common.RegisterBundle(new MustacheBundle());
        instance.Common.RegisterBundle(new LinqBundle());
        instance.Common.RegisterBundle(new JsonDataBundle());
        instance.Common.RegisterBundle(new ActiveDirectoryBundle());
        instance.Common.RegisterBundle(new DocumentBundle());
        //instance.Common.RegisterBundle(new K2Bundle());
        instance.Common.RegisterBundle(new UtilityBundle());
        //instance.Common.RegisterBundle(new DocumentStoreBundle());
        instance.Common.RegisterBundle(new SimpleInheritanceBundle());
        instance.Common.RegisterBundle(new SqlDataBundle());
        instance.Common.RegisterBundle(new StateMachineBundle());
        instance.Common.RegisterBundle(new DeferredBundle());
        //instance.Common.RegisterBundle(new TfsBundle());
        //instance.Common.RegisterBundle(new BaristaSearchIndexBundle());
        instance.Common.RegisterBundle(new WebAdministrationBundle());

        //Global Types
        engine.SetGlobalValue("Barista", instance);

        //engine.SetGlobalValue("file", new FileSystemInstance(engine));

        engine.SetGlobalValue("Guid", new GuidConstructor(engine));
        engine.SetGlobalValue("Uri", new UriConstructor(engine));
        engine.SetGlobalValue("Base64EncodedByteArray", new Base64EncodedByteArrayConstructor(engine));

        //engine.SetGlobalValue("console", console);

        //Map Barista functions to global functions.
        engine.Execute(@"var help = function(obj) { return Barista.help(obj); };
var require = function(name) { return Barista.common.require(name); };
var listBundles = function() { return Barista.common.listBundles(); };
var include = function(scriptUrl) { return Barista.SharePoint.include(scriptUrl); };");

        //Execute any instance initialization code.
        if (String.IsNullOrEmpty(BaristaContext.Current.Request.InstanceInitializationCode) == false)
        {
          var initializationScriptSource = new BaristaScriptSource(BaristaContext.Current.Request.InstanceInitializationCode, BaristaContext.Current.Request.InstanceInitializationCodePath);

          try
          {
            engine.Execute(initializationScriptSource);
          }
          catch (JavaScriptException ex)
          {
            //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
            UpdateResponseWithJavaScriptExceptionDetails(ex, BaristaContext.Current.Response);
            errorInInitialization = true;

            //switch (BaristaContext.Current.Request.InstanceMode)
            //{
            //  case BaristaInstanceMode.Single:
            //    BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
            //    break;
            //  case BaristaInstanceMode.PerSession:
            //    BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
            //    break;
            //}
          }
          catch (Exception ex)
          {
            //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occured while evaluating script: ");
            errorInInitialization = true;
            //switch (BaristaContext.Current.Request.InstanceMode)
            //{
            //  case BaristaInstanceMode.Single:
            //    BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
            //    break;
            //  case BaristaInstanceMode.PerSession:
            //    BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
            //    break;
            //}
            throw;
          }
        }
      }

      return engine;
    }
  }
}