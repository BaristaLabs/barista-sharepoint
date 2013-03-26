namespace Barista.SharePoint
{
  using System.Linq;
  using System.Net;
  using System.Text;
  using Barista.Bundles;
  using Barista.Library;
  using Barista.SharePoint.Bundles;
  using Barista.SharePoint.Library;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;

  public static class SPScriptEngineFactory
  {
    private const string JavaScriptExceptionMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #492B29}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>JavaScript Error</TITLE></HEAD><BODY>
<DIV id=""content"">
<P class=""heading1"">JavaScript Error</P>
<BR/>
<P class=""intro"">The JavaScript being executed on the server threw an exception ({0}). The exception message is '{1}'.</P>
<p class=""intro"">Function Name: <span id=""functionName"">{2}</span><br/>
Line Number: <span id=""lineNumber"">{3}</span><br/>
Source Path: <span id=""sourcePath"">{4}</span></p>
<P class=""intro""/>
<P class=""intro"">The exception stack trace is:</P>
<P class=""intro stackTrace"">{5}</P>
</DIV>
</BODY></HTML>
";
    private const string JavaScriptTimeoutMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #A68300}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>Barista Script Timeout</TITLE></HEAD><BODY>
<DIV id=""content"">
<P class=""heading1"">Barista Script Timeout</P>
<BR/>
<P class=""intro"">The script being executed exceeded the maximum allowable timeout as set by an Administrator. ({0}ms).</P>
</DIV>
</BODY></HTML>
";

    private const string ExceptionMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<HTML><HEAD>
<STYLE type=""text/css"">
#content{{ FONT-SIZE: 0.7em; PADDING-BOTTOM: 2em; MARGIN-LEFT: 30px}}
BODY{{MARGIN-TOP: 0px; MARGIN-LEFT: 0px; COLOR: #000000; FONT-FAMILY: Verdana; BACKGROUND-COLOR: white}}
P{{MARGIN-TOP: 0px; MARGIN-BOTTOM: 12px; COLOR: #000000; FONT-FAMILY: Verdana}}
PRE{{BORDER-RIGHT: #f0f0e0 1px solid; PADDING-RIGHT: 5px; BORDER-TOP: #f0f0e0 1px solid; MARGIN-TOP: -5px; PADDING-LEFT: 5px; FONT-SIZE: 1.2em; PADDING-BOTTOM: 5px; BORDER-LEFT: #f0f0e0 1px solid; PADDING-TOP: 5px; BORDER-BOTTOM: #f0f0e0 1px solid; FONT-FAMILY: Courier New; BACKGROUND-COLOR: #e5e5cc}}
.heading1{{MARGIN-TOP: 0px; PADDING-LEFT: 15px; FONT-WEIGHT: normal; FONT-SIZE: 26px; MARGIN-BOTTOM: 0px; PADDING-BOTTOM: 3px; MARGIN-LEFT: -30px; WIDTH: 100%; COLOR: #ffffff; PADDING-TOP: 10px; FONT-FAMILY: Tahoma; BACKGROUND-COLOR: #A60000}}
.intro{{MARGIN-LEFT: -15px}}</STYLE>
<TITLE>Request Error</TITLE></HEAD><BODY>
<DIV id=""content"">
<P class=""heading1"">Error</P>
<BR/>
<P class=""intro"">The server encountered an error processing the request. The exception message is '{0}'.</P>
<P class=""intro"">The exception stack trace is:</P>
<P class=""intro stackTrace"">{1}</P>
</DIV>
</BODY></HTML>
";

    /// <summary>
    /// Returns a new instance of a script engine object with all runtime objects available.
    /// </summary>
    /// <returns></returns>
    public static ScriptEngine GetScriptEngine(WebBundle webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
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
        case BaristaInstanceMode.Single:
          engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
          break;
        case BaristaInstanceMode.PerSession:
          engine = BaristaSharePointGlobal.GetOrCreateScriptEngineInstanceFromSession(BaristaContext.Current.Request.InstanceName, out isNewScriptEngineInstance);
          break;
        default:
          throw new NotImplementedException("The instance mode of " + BaristaContext.Current.Request.InstanceMode + " is currently not supported.");
      }

      if (BaristaContext.Current.Request.ForceStrict)
      {
        engine.ForceStrictMode = true;
      }

      if (isNewScriptEngineInstance)
      {
        var console = new FirebugConsole(engine)
        {
          Output = new BaristaConsoleOutput(engine)
        };

        //Register Bundles.
        var instance = new BaristaGlobal(engine.Object.InstancePrototype);

        instance.Common.RegisterBundle(webBundle);
        instance.Common.RegisterBundle(new StringBundle());
        instance.Common.RegisterBundle(new MomentBundle());
        instance.Common.RegisterBundle(new MustacheBundle());
        instance.Common.RegisterBundle(new LinqBundle());
        instance.Common.RegisterBundle(new JsonDataBundle());
        instance.Common.RegisterBundle(new SharePointBundle());
        instance.Common.RegisterBundle(new ActiveDirectoryBundle());
        instance.Common.RegisterBundle(new DocumentBundle());
        instance.Common.RegisterBundle(new K2Bundle());
        instance.Common.RegisterBundle(new UtilityBundle());
        instance.Common.RegisterBundle(new UlsLogBundle());
        instance.Common.RegisterBundle(new DocumentStoreBundle());
        instance.Common.RegisterBundle(new SimpleInheritanceBundle());
        instance.Common.RegisterBundle(new SqlDataBundle());
        instance.Common.RegisterBundle(new StateMachineBundle());
        instance.Common.RegisterBundle(new DeferredBundle());
        instance.Common.RegisterBundle(new TfsBundle());

        //Global Types
        engine.SetGlobalValue("Barista", instance);
        instance.SetPropertyValue("SharePoint", new BaristaSharePointGlobal(engine.Object.InstancePrototype), true);

        //engine.SetGlobalValue("file", new FileSystemInstance(engine));

        engine.SetGlobalValue("Guid", new GuidConstructor(engine));
        engine.SetGlobalValue("Uri", new UriConstructor(engine));
        engine.SetGlobalValue("Base64EncodedByteArray", new Base64EncodedByteArrayConstructor(engine));

        engine.SetGlobalValue("console", console);

        //If we came from the Barista event receiver, set the appropriate context.
        if (
          BaristaContext.Current.Request != null &&
          BaristaContext.Current.Request.ExtendedProperties != null &&
          BaristaContext.Current.Request.ExtendedProperties.ContainsKey(
            Constants.BaristaItemEventReceiverCodePropertyBagKey))
        {
          var properties =
            BaristaContext.Current.Request.ExtendedProperties[
              Constants.BaristaItemEventReceiverCodePropertyBagKey];

          var itemEventProperties = JsonConvert.DeserializeObject<BaristaItemEventProperties>(properties);
          engine.SetGlobalValue("CurrentItemEventProperties", new BaristaItemEventPropertiesInstance(engine.Object.InstancePrototype, itemEventProperties));
        }

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
            BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
            UpdateResponseWithJavaScriptExceptionDetails(ex, BaristaContext.Current.Response);
            errorInInitialization = true;

            switch (BaristaContext.Current.Request.InstanceMode)
            {
              case BaristaInstanceMode.Single:
                BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
                break;
              case BaristaInstanceMode.PerSession:
                BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
                break;
            }
          }
          catch (Exception ex)
          {
            BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occured while evaluating script: ");
            errorInInitialization = true;
            switch (BaristaContext.Current.Request.InstanceMode)
            {
              case BaristaInstanceMode.Single:
                BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
                break;
              case BaristaInstanceMode.PerSession:
                BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
                break;
            }
            throw;
          }
        }
      }

      return engine;
    }

    public static void UpdateResponseWithJavaScriptExceptionDetails(JavaScriptException exception, BrewResponse response)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");

      if (response == null)
        throw new ArgumentNullException("response");

      var errorObject = exception.ErrorObject as ObjectInstance;

      if (errorObject == null)
        return;

      var message = errorObject.GetPropertyValue("message") as string;
      var stack = errorObject.GetPropertyValue("stack") as string;
      if (String.IsNullOrEmpty(stack) == false)
        stack = stack.Replace("at ", "at<br/>");

      response.StatusCode = HttpStatusCode.BadRequest;
      response.StatusDescription = message;
      response.AutoDetectContentType = false;
      response.ContentType = "text/html";

      var resultMessage = String.Format(JavaScriptExceptionMessage, exception.Name, message, exception.FunctionName, exception.LineNumber, exception.SourcePath, stack);
      response.Content = Encoding.UTF8.GetBytes(resultMessage);
    }

    public static void UpdateResponseWithExceptionDetails(Exception exception, BrewResponse response)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");

      if (response == null)
        throw new ArgumentNullException("response");

      if (exception is AggregateException)
      {
        var exceptions = (exception as AggregateException).InnerExceptions;

        //TODO: Make this better.
        exception = exceptions.First();
      }

      var message = exception.Message;
      var stack = exception.StackTrace;
      if (String.IsNullOrEmpty(stack) == false)
        stack = stack.Replace("at ", "at<br/>");

      response.StatusCode = HttpStatusCode.BadRequest;
      response.StatusDescription = message;
      response.AutoDetectContentType = false;
      response.ContentType = "text/html";

      var resultMessage = String.Format(ExceptionMessage, exception.Message, stack);
      response.Content = Encoding.UTF8.GetBytes(resultMessage);
    }

    public static void UpdateResponseWithTimeoutDetails(int executionTimeout, BrewResponse response)
    {
      response.StatusCode = HttpStatusCode.BadRequest;
      response.StatusDescription =
        "The script being executed exceeded the maximum allowable timeout as set by an Administrator. ({0}ms).";
      response.AutoDetectContentType = false;
      response.ContentType = "text/html";

      var resultMessage = String.Format(JavaScriptTimeoutMessage, executionTimeout);
      response.Content = Encoding.UTF8.GetBytes(resultMessage);
    }
  }
}
