namespace Barista.SharePoint.Services
{
  using Barista.Bundles;
  using Barista.Library;
  using Barista.SharePoint.Bundles;
  using Barista.SharePoint.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.ServiceModel;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;

  [Guid("9B4C0B5C-8A42-401A-9ACB-42EA6246E960")]
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
  internal sealed class BaristaServiceApplication : SPIisWebServiceApplication, IBaristaServiceApplication
  {
    #region Constants
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
    #endregion
    [Persisted]
    private int m_settings;
    public int Settings
    {
      get { return m_settings; }
      set { m_settings = value; }
    }

    public BaristaServiceApplication()
      : base() { }

    private BaristaServiceApplication(string name, BaristaService service, SPIisWebServiceApplicationPool appPool)
      : base(name, service, appPool) { }

    public static BaristaServiceApplication Create(string name, BaristaService service, SPIisWebServiceApplicationPool appPool)
    {
      #region validation
      if (name == null) throw new ArgumentNullException("name");
      if (service == null) throw new ArgumentNullException("service");
      if (appPool == null) throw new ArgumentNullException("appPool");
      #endregion

      // create the service application
      BaristaServiceApplication serviceApplication = new BaristaServiceApplication(name, service, appPool);
      serviceApplication.Update();

      // register the supported endpoints
      serviceApplication.AddServiceEndpoint("http", SPIisWebServiceBindingType.Http);
      serviceApplication.AddServiceEndpoint("https", SPIisWebServiceBindingType.Https, "secure");

      return serviceApplication;
    }

    #region service application details
    protected override string DefaultEndpointName
    {
      get { return "http"; }
    }

    public override string TypeName
    {
      get { return "Barista Service Application"; }
    }

    protected override string InstallPath
    {
      get { return SPUtility.GetGenericSetupPath(@"WebServices\Barista"); }
    }

    protected override string VirtualPath
    {
      get { return "Barista.svc"; }
    }

    public override Guid ApplicationClassId
    {
      get { return new Guid("9B4C0B5C-8A42-401A-9ACB-42EA6246E960"); }
    }

    public override Version ApplicationVersion
    {
      get { return new Version("1.0.0.0"); }
    }
    #endregion

    #region Service Application UI

    public override SPAdministrationLink ManageLink
    {
      get
      { return new SPAdministrationLink("/_admin/BaristaService/Manage.aspx"); }
    }

    public override SPAdministrationLink PropertiesLink
    {
      get
      { return new SPAdministrationLink("/_admin/BaristaService/Manage.aspx"); }
    }

    #endregion

    #region IBaristaServiceApplication implementation
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public BrewResponse Eval(BrewRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      var response = new BrewResponse();

      BaristaContext.Current = new BaristaContext(request, response);

      WebBundle webBundle = new WebBundle();
      var engine = GetScriptEngine(webBundle);

      object result = null;
      try
      {
        var source = new BaristaScriptSource(request.Code, request.CodePath);
        result = engine.Evaluate(source);

        var isRaw = false;

        //If the web instance has been initialized on the web bundle, use the value set via script, otherwise use defaults.
        if (webBundle.WebInstance == null || webBundle.WebInstance.Response.AutoDetectContentType)
        {
          response.ContentType = BrewResponse.AutoDetectContentTypeFromResult(result);
        }

        if (webBundle.WebInstance != null)
        {
          isRaw = webBundle.WebInstance.Response.IsRaw;
        }

        var stringified = JSONObject.Stringify(engine, result, null, null);
        response.SetContentsFromResultObject(engine, result, isRaw);
      }
      catch (JavaScriptException ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
        UpdateResponseWithJavaScriptExceptionDetails(ex, response);
      }
      catch (Exception ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occured while evaluating script: ");
        throw;
      }
      finally
      {
        //Cleanup
        engine = null;

        if (BaristaContext.Current != null)
          BaristaContext.Current.Dispose();

        BaristaContext.Current = null;
      }

      return response;
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void Exec(BrewRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      var response = new BrewResponse();

      BaristaContext.Current = new BaristaContext(request, response);

      WebBundle webBundle = new WebBundle();
      var engine = GetScriptEngine(webBundle);

      try
      {
        var source = new BaristaScriptSource(request.Code, request.CodePath);
        engine.Execute(source);
      }
      catch (JavaScriptException ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
        UpdateResponseWithJavaScriptExceptionDetails(ex, response);
      }
      catch (Exception ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occured while executing script: ");
        throw;
      }
      finally
      {
        //Cleanup
        engine = null;

        if (BaristaContext.Current != null)
          BaristaContext.Current.Dispose();

        BaristaContext.Current = null;
      }
    }

    /// <summary>
    /// Returns a new instance of a script engine object with all runtime objects available.
    /// </summary>
    /// <returns></returns>
    private ScriptEngine GetScriptEngine(WebBundle webBundle)
    {
      var engine = new Jurassic.ScriptEngine();

      if (BaristaContext.Current.Request.ForceStrict)
      {
        engine.ForceStrictMode = true;
      }

      var console = new FirebugConsole(engine);
      console.Output = new BaristaConsoleOutput(engine);

      //Register Bundles.
      Common common = new Common();
      common.RegisterBundle(webBundle);
      common.RegisterBundle(new MustacheBundle());
      common.RegisterBundle(new LinqBundle());
      common.RegisterBundle(new JsonDataBundle());
      common.RegisterBundle(new SharePointBundle());
      common.RegisterBundle(new ActiveDirectoryBundle());
      common.RegisterBundle(new DocumentBundle());
      common.RegisterBundle(new K2Bundle());
      common.RegisterBundle(new UtilityBundle());
      common.RegisterBundle(new UlsLogBundle());
      common.RegisterBundle(new DocumentStoreBundle());
      common.RegisterBundle(new SimpleInheritanceBundle());
      common.RegisterBundle(new SqlDataBundle());
      common.RegisterBundle(new StateMachineBundle());

      //Global Types

      //engine.SetGlobalValue("file", new FileSystemInstance(engine));

      engine.SetGlobalValue("Guid", new GuidConstructor(engine));
      engine.SetGlobalValue("Uri", new UriConstructor(engine));
      engine.SetGlobalValue("Deferred", new DeferredConstructor(engine));
      engine.SetGlobalValue("Base64EncodedByteArrayInstance", new Base64EncodedByteArrayConstructor(engine));

      engine.SetGlobalValue("console", console);

      //Functions
      engine.SetGlobalFunction("help", new Func<object, object>(obj => Help.GenerateHelpJsonForObject(engine, obj)));
      engine.SetGlobalFunction("require", new Func<string, object>(obj => common.Require(engine, obj)));
      engine.SetGlobalFunction("listBundles", new Func<object>(() => common.List(engine)));

      engine.SetGlobalFunction("delay", new Action<int>((millisecondsTimeout) => { System.Threading.Thread.Sleep(millisecondsTimeout); }));
      engine.SetGlobalFunction("waitAll", new Action<object, object>((deferreds, timeout) => { DeferredInstance.WaitAll(deferreds, timeout); }));

      engine.SetGlobalFunction("include", new Action<string>((scriptUrl) =>
      {
        var source = new SPFileScriptSource(engine, scriptUrl);

        engine.Execute(source);
      }));

      engine.SetGlobalFunction("replaceJsonReferences", new Func<object, object>((o) =>
      {
        return ReplaceJsonReferences(o);
      }));
      return engine;
    }

    private object ReplaceJsonReferences(object o)
    {
      var dictionary = new Dictionary<string,ObjectInstance>();
      return ReplaceJsonReferences(o, dictionary);
    }

    private object ReplaceJsonReferences(object o, Dictionary<string, ObjectInstance> dictionary)
    {
      if (o is ArrayInstance)
      {
        var array = o as ArrayInstance;
        for (int i = 0; i < array.ElementValues.Count(); i++)
        {
          
          array[i] = ReplaceJsonReferences(array[i], dictionary);
        }
      }
      else if (o is ObjectInstance)
      {
        var obj = o as ObjectInstance;
        var properties = obj.Properties.ToList();

        //If there's only one property named "$ref" and it's value is a key that exists in the dictionary, return the value.
        if (properties.Count == 1 && properties[0].Name == "$ref" && dictionary.ContainsKey((string)properties[0].Value))
          return dictionary[(string)properties[0].Value];

        var idProperty = properties.Where(p => p.Name == "$id").FirstOrDefault();
        if (idProperty != null && dictionary.ContainsKey((string)idProperty.Value) == false)
        {
          var str = JSONObject.Stringify(obj.Engine, obj, null, null);
          var clone = JSONObject.Parse(obj.Engine, str, null) as ObjectInstance;

          if (clone.HasProperty("$id"))
            clone.Delete("$id", false);

          dictionary.Add((string)idProperty.Value, clone);
        }

        foreach (var property in properties)
        {
          obj.SetPropertyValue(property.Name, ReplaceJsonReferences(property.Value, dictionary), false);
        }
      }
      return o;
    }

    private void UpdateResponseWithJavaScriptExceptionDetails(JavaScriptException exception, BrewResponse response)
    {
      ObjectInstance errorObject = exception.ErrorObject as ObjectInstance;

      response.StatusCode = System.Net.HttpStatusCode.BadRequest;

      response.AutoDetectContentType = false;
      response.ContentType = "text/html";

      var message = errorObject.GetPropertyValue("message") as string;
      var stack = errorObject.GetPropertyValue("stack") as string;
      if (String.IsNullOrEmpty(stack) == false)
        stack = stack.Replace("at ", "at<br/>");
      var resultMessage = String.Format(JavaScriptExceptionMessage, exception.Name, message, exception.FunctionName, exception.LineNumber, exception.SourcePath, stack);
      response.Content = Encoding.UTF8.GetBytes(resultMessage);
    }
    #endregion
  }
}
