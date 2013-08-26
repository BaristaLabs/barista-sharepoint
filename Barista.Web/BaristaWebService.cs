namespace Barista.Web
{
  using System.Threading;
  using Barista.Bundles;
  using Barista.Extensions;
  using Barista.Framework;
  using Barista.Helpers;
  using Barista.Jurassic;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Text;
  using System.Web;

  /// <summary>
  /// Represents the Barista WCF service endpoint that responds to REST requests.
  /// </summary>
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerCall,
    ConcurrencyMode = ConcurrencyMode.Multiple)]
  [RawJsonRequestBehavior]
  public class BaristaWebService : IBaristaWebService
  {
    #region Service Operations
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType(RestOnly = true)]
    public void Coffee()
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      string codePath;
      var code = Grind();

      code = Tamp(code, out codePath);

      Brew(code, codePath);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void CoffeeAuLait(Stream stream)
    {
      TakeOrder();

      string codePath;
      var code = Grind();

      code = Tamp(code, out codePath);

      Brew(code, codePath);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType(RestOnly = true)]
    public Stream Espresso()
    {
      if (WebOperationContext.Current == null)
        throw new InvalidOperationException("This service operation is intended to be invoked only by REST clients.");

      TakeOrder();

      string codePath;
      var code = Grind();

      code = Tamp(code, out codePath);

      return Pull(code, codePath);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream Latte(Stream stream)
    {
      TakeOrder();

      string codePath;
      var code = Grind();

      code = Tamp(code, out codePath);

      return Pull(code, codePath);
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Takes an order. E.g. Asserts that the current web is valid, the user has read permissions on the current web, and the current web is a trusted location.
    /// </summary>
    private static void TakeOrder()
    {
      //Do nothing in this impl.
    }

    /// <summary>
    /// Grinds the beans. E.g. Attempts to retrieve the code value from the request.
    /// </summary>
    private static string Grind()
    {
      string code = null;

      //If the request has a header named "X-Barista-Code" use that first.
      if (WebOperationContext.Current != null)
      {
        var requestHeaders = WebOperationContext.Current.IncomingRequest.Headers;
        var codeHeaderKey = requestHeaders.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "X-Barista-Code");
        if (codeHeaderKey != null)
          code = requestHeaders[codeHeaderKey];
      }

      //If the request has a query string parameter named "c" use that.
      if (String.IsNullOrEmpty(code) && WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
      {
        var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        var codeKey = requestQueryParameters.AllKeys.FirstOrDefault(k => k != null && k.ToLowerInvariant() == "c");
        if (codeKey != null)
          code = requestQueryParameters[codeKey];
      }

      //If the request contains a form variable named "c" or "code" use that.
      if (String.IsNullOrEmpty(code) && HttpContext.Current.Request.HttpMethod == "POST")
      {
        var form = HttpContext.Current.Request.Form;
        var formKey = form.AllKeys.FirstOrDefault(k => k != null && (k.ToLowerInvariant() == "c" || k.ToLowerInvariant() == "code"));
        if (formKey != null)
          code = form[formKey];
      }

      //If the request contains a Message header named "c" or "code in the appropriate namespace, use that.
      if (String.IsNullOrEmpty(code) && OperationContext.Current != null)
      {
        var headers = OperationContext.Current.IncomingMessageHeaders;

        if (headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "code"))
          code = headers.GetHeader<string>("code", Barista.Constants.ServiceNamespace);
        else if (String.IsNullOrEmpty(code) && headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "c"))
          code = headers.GetHeader<string>("c", Barista.Constants.ServiceNamespace);
      }

      //Otherwise, use the body of the post as code.
      if (String.IsNullOrEmpty(code) &&
        HttpContext.Current.Request.HttpMethod == "POST" &&
        OperationContext.Current != null &&
        OperationContext.Current.RequestContext.RequestMessage.IsEmpty == false)
      {
        var body = OperationContext.Current.RequestContext.RequestMessage.GetBody<byte[]>();
        var bodyString = Encoding.UTF8.GetString(body);

        if (bodyString.IsNullOrWhiteSpace() == false)
        {
          //Try using JSON encoding.
          try
          {
            var jsonFormBody = JObject.Parse(bodyString);
            var codeProperty =
              jsonFormBody.Properties()
                          .FirstOrDefault(p => p.Name.ToLowerInvariant() == "code" || p.Name.ToLowerInvariant() == "c");
            if (codeProperty != null)
              code = codeProperty.Value.ToObject<string>();
          }
          catch
          {
            /* Do Nothing */
          }

          //Try using form encoding
          if (String.IsNullOrEmpty(code))
          {
            try
            {
              var form = HttpUtility.ParseQueryString(bodyString);
              var formKey =
                form.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "c" || k.ToLowerInvariant() == "code");
              if (formKey != null)
                code = form[formKey];
            }
            catch
            {
              /* Do Nothing */
            }
          }
        }
      }

      if (String.IsNullOrEmpty(code))
        throw new InvalidOperationException("Code must be specified either through the 'X-Barista-Code' header, through a 'c' or 'code' soap message header in the http://barista/services/v1 namespace, a 'c' query string parameter or the 'code' or 'c' form field that contains either a literal script declaration or a relative or absolute path to a script file.");


      //Determine if a language is specified.
      string language = null;

      //If the request has a header named "X-Barista-Code-Language" use that first.
      if (WebOperationContext.Current != null)
      {
        var requestHeaders = WebOperationContext.Current.IncomingRequest.Headers;
        var languageHeaderKey = requestHeaders.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "X-Barista-Code-Language");
        if (languageHeaderKey != null)
          language = requestHeaders[languageHeaderKey];
      }

      //If the request has a query string parameter named "lang" use that.
      if (String.IsNullOrEmpty(language) && WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.UriTemplateMatch != null)
      {
        var requestQueryParameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

        var languageKey = requestQueryParameters.AllKeys.FirstOrDefault(k => k != null && k.ToLowerInvariant() == "lang");
        if (languageKey != null)
          language = requestQueryParameters[languageKey];
      }

      //If the request contains a form variable named "lang" use that.
      if (String.IsNullOrEmpty(language) && HttpContext.Current.Request.HttpMethod == "POST")
      {
        var form = HttpContext.Current.Request.Form;
        var formKey = form.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "lang");
        if (formKey != null)
          language = form[formKey];
      }

      if (String.IsNullOrEmpty(language) != true)
      {
        switch (language.ToLowerInvariant())
        {
          case "typescript":
            //Call the command-line compiler and retrieve the javascript.

            //TODO: What about tsc.exe errors? Parse output? Must pass TSC.exe a temp folder location...
            throw new NotImplementedException();
        }
      }

      return code;
    }

    /// <summary>
    /// Tamps the ground coffee. E.g. Parses the code and makes it ready to be executed (brewed).
    /// </summary>
    /// <param name="code"></param>
    /// <param name="scriptPath"></param>
    /// <returns></returns>
    private static string Tamp(string code, out string scriptPath)
    {
      scriptPath = String.Empty;

      //If the code looks like a path, attempt to retrieve a code file and use the contents of that file as the code.
      if (code.StartsWith("~"))
      {
        var mappedPath = HttpContext.Current.Request.MapPath(code);
        if (File.Exists(mappedPath))
        {
          scriptPath = mappedPath;
          code = File.ReadAllText(mappedPath);
        }
      }
      else
      {
        var path = "API";
        var configPathKey =
          ConfigurationManager.AppSettings.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "barista_scriptpath");
        if (configPathKey != null)
        {
          var configPath = ConfigurationManager.AppSettings[configPathKey];
          if (String.IsNullOrEmpty(configPath) == false)
          {
            path = configPath;
          }
        }

        if (path.IsValidPath() && code.IsValidPath())
        {
          path = Path.Combine(path, code);
          if (File.Exists(path))
          {
            scriptPath = path;
            code = File.ReadAllText(path);
          }
        }
      }

      //Replace any tokens in the code.
      //TODO: re-implement this if appropriate.
      HttpRequest request = HttpContext.Current.Request;
      var serverRelativeWebUrl = request.Url.AbsoluteUri.Replace(request.Url.AbsolutePath, String.Empty);
      code = code.Replace("{WebUrl}", serverRelativeWebUrl);

      //code = SPHelper.ReplaceTokens(SPContext.Current, code);

      return code;
    }

    /// <summary>
    /// Brews a cup of coffee. E.g. Executes the specified script.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codePath"></param>
    private static void Brew(string code, string codePath)
    {
      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;
      request.CodePath = codePath;

      if (String.IsNullOrEmpty(request.InstanceInitializationCode) == false)
      {
        string instanceInitializationCodePath;
        request.InstanceInitializationCode = Tamp(request.InstanceInitializationCode, out instanceInitializationCodePath);
        request.InstanceInitializationCodePath = instanceInitializationCodePath;
      }

      Exec(request);
    }

    /// <summary>
    /// Pulls a shot of espresso. E.g. Executes the specified script and sets the appropriate values on the response object.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codePath"></param>
    /// <returns></returns>
    private static Stream Pull(string code, string codePath)
    {
      var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
      request.Code = code;
      request.CodePath = codePath;

      if (String.IsNullOrEmpty(request.InstanceInitializationCode) == false)
      {
        string instanceInitializationCodePath;
        request.InstanceInitializationCode = Tamp(request.InstanceInitializationCode, out instanceInitializationCodePath);
        request.InstanceInitializationCodePath = instanceInitializationCodePath;
      }

      var result = Eval(request);

      var setHeader = true;
      if (WebOperationContext.Current != null)
      {
        result.ModifyWebOperationContext(WebOperationContext.Current.OutgoingResponse);
        setHeader = false;
      }

      result.ModifyHttpResponse(HttpContext.Current.Response, setHeader);

      if (result.Content == null)
        return null;

      var resultStream = new MemoryStream(result.Content);
      return resultStream;
    }
    #endregion

    #region Implementation
    public static BrewResponse Eval(BrewRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      var response = new BrewResponse
      {
        ContentType = request.ContentType
      };

      BaristaContext.Current = new BaristaContext(request, response);

      Mutex syncRoot = null;

      if (BaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
      {
        syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceName);
      }

      var webBundle = new BaristaWebBundle();
      var source = new BaristaScriptSource(request.Code, request.CodePath);

      if (syncRoot != null)
        syncRoot.WaitOne();

      try
      {
        bool isNewScriptEngineInstance;
        bool errorInInitialization;

        var scriptEngineFactory = new BaristaScriptEngineFactory();
        var engine = scriptEngineFactory.GetScriptEngine(webBundle, out isNewScriptEngineInstance, out errorInInitialization);

        if (errorInInitialization)
          return response;

        try
        {
          var result = engine.Evaluate(source);

          var isRaw = false;

          //If the web instance has been initialized on the web bundle, use the value set via script, otherwise use defaults.
          if (webBundle.WebInstance == null || webBundle.WebInstance.Response.AutoDetectContentType)
          {
            response.ContentType = BrewResponse.AutoDetectContentTypeFromResult(result, response.ContentType);

            var arrayResult = result as Barista.Library.Base64EncodedByteArrayInstance;
            if (arrayResult != null && arrayResult.FileName.IsNullOrWhiteSpace() == false && response.Headers != null && response.Headers.ContainsKey("Content-Disposition") == false)
            {
              var br = BrowserUserAgentParser.GetDefault();
              var clientInfo = br.Parse(request.UserAgent);

              if (clientInfo.UserAgent.Family == "IE" && (clientInfo.UserAgent.Major == "7" || clientInfo.UserAgent.Major == "8"))
                response.Headers.Add("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(arrayResult.FileName));
              else if (clientInfo.UserAgent.Family == "Safari")
                response.Headers.Add("Content-Disposition", "attachment; filename=" + arrayResult.FileName);
              else
                response.Headers.Add("Content-Disposition", "attachment; filename=\"" + HttpUtility.UrlEncode(arrayResult.FileName) + "\"");
            }
          }

          if (webBundle.WebInstance != null)
          {
            isRaw = webBundle.WebInstance.Response.IsRaw;
          }

          response.SetContentsFromResultObject(engine, result, isRaw);
        }
        catch (JavaScriptException ex)
        {
          //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
          scriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(engine, ex, response);
        }
        catch (Exception ex)
        {
          //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occurred while evaluating script: ");
          scriptEngineFactory.UpdateResponseWithExceptionDetails(ex, response);
        }
        finally
        {
          //Cleanup
          // ReSharper disable RedundantAssignment
          engine = null;
          // ReSharper restore RedundantAssignment

          if (BaristaContext.Current != null)
            BaristaContext.Current.Dispose();

          BaristaContext.Current = null;
        }
      }
      finally
      {
        if (syncRoot != null)
          syncRoot.ReleaseMutex();
      }

      return response;
    }

    public static void Exec(BrewRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      var response = new BrewResponse
      {
        ContentType = request.ContentType
      };

      //Set the current context with information from the current request and response.
      BaristaContext.Current = new BaristaContext(request, response);

      //If we're not executing with Per-Call instancing, create a mutex to synchronize against.
      Mutex syncRoot = null;
      if (BaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
      {
        syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceName);
      }

      var webBundle = new BaristaWebBundle();
      var source = new BaristaScriptSource(request.Code, request.CodePath);

      if (syncRoot != null)
        syncRoot.WaitOne();

      try
      {
        bool isNewScriptEngineInstance;
        bool errorInInitialization;

        var scriptEngineFactory = new BaristaScriptEngineFactory();
        var engine = scriptEngineFactory.GetScriptEngine(webBundle, out isNewScriptEngineInstance, out errorInInitialization);

        if (errorInInitialization)
          return;

        try
        {
            engine.Execute(source);
        }
        catch (JavaScriptException ex)
        {
          //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException,
          //                                             "A JavaScript exception was thrown while evaluating script: ");
          scriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(engine, ex, response);
        }
        catch (Exception ex)
        {
          //BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime,
          //                                             "An internal error occured while executing script: ");
          scriptEngineFactory.UpdateResponseWithExceptionDetails(ex, response);
        }
        finally
        {
          //Cleanup
          // ReSharper disable RedundantAssignment
          engine = null;
          // ReSharper restore RedundantAssignment

          if (BaristaContext.Current != null)
            BaristaContext.Current.Dispose();

          BaristaContext.Current = null;
        }
      }
      finally
      {
        if (syncRoot != null)
          syncRoot.ReleaseMutex();
      }
    }
    #endregion
  }
}
