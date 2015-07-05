namespace Barista.SharePoint.Services
{
    using Barista.SharePoint.Extensions;
    using Barista.Extensions;
    using Barista.Framework;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Client.Services;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Web;
    using HttpUtility = Barista.Helpers.HttpUtility;

    /// <summary>
    /// Represents the Barista WCF service endpoint that responds to REST requests.
    /// </summary>
    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
      InstanceContextMode = InstanceContextMode.PerCall,
      ConcurrencyMode = ConcurrencyMode.Multiple)]
    [RawJsonRequestBehavior]
    public class BaristaV8WebService : IBaristaWebService
    {
        #region Service Operations

        /// <summary>
        /// Executes the specified script and does not return a result.
        /// </summary>
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
        [DynamicResponseType(RestOnly = true)]
        public void CoffeeWild()
        {
            Coffee();
        }

        /// <summary>
        /// Overload for coffee to allow http POSTS.
        /// </summary>
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
        [DynamicResponseType]
        public void CoffeeAuLaitWild(Stream stream)
        {
            CoffeeAuLait(stream);
        }

        /// <summary>
        /// Evaluates the specified script.
        /// </summary>
        /// <returns></returns>
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
        [DynamicResponseType(RestOnly = true)]
        public Stream EspressoWild()
        {
            return Espresso();
        }

        /// <summary>
        /// Expresso overload to support having code contained in the body of a http POST.
        /// </summary>
        /// <returns></returns>
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

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType]
        public Stream LatteWild(Stream stream)
        {
            return Latte(stream);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Takes an order. E.g. Asserts that the current web is valid, the user has read permissions on the current web, and the current web is a trusted location.
        /// </summary>
        private static void TakeOrder()
        {
            if (SPContext.Current == null || SPContext.Current.Web == null)
                throw new InvalidOperationException("Cannot execute Barista: Barista must execute within the context of a SharePoint Web.");

            if (SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.Open) == false)
                throw new InvalidOperationException("Cannot execute Barista: Access Denied - The current user does not have access to the current web.");

            BaristaHelper.EnsureExecutionInTrustedLocation();
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
            if (String.IsNullOrEmpty(code) && HttpContext.Current.Request.HttpMethod == "POST")
            {
                var body = HttpContext.Current.Request.InputStream.ToByteArray();
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

            //If the code looks like a uri, attempt to retrieve a code file and use the contents of that file as the code.
            if (Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
            {
                Uri codeUri;
                if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
                {
                    string scriptFilePath;
                    bool isHiveFile;
                    String codeFromfile;
                    if (SPHelper.TryGetSPFileAsString(code, out scriptFilePath, out codeFromfile, out isHiveFile))
                    {
                        if (isHiveFile == false)
                        {
                            var lockDownMode = SPContext.Current.Web.GetProperty("BaristaLockdownMode") as string;
                            if (String.IsNullOrEmpty(lockDownMode) == false && lockDownMode.ToLowerInvariant() == "BaristaContentLibraryOnly")
                            {
                                //TODO: implement this.
                            }
                        }

                        scriptPath = scriptFilePath;
                        code = codeFromfile;
                    }
                }
            }

            //Replace any tokens in the code.
            code = SPHelper.ReplaceTokens(SPContext.Current, code);

            return code;
        }

        /// <summary>
        /// Brews a cup of coffee. E.g. Executes the specified script.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codePath"></param>
        private static void Brew(string code, string codePath)
        {
            var client = new BaristaServiceClient(SPServiceContext.Current);

            var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
            request.ScriptEngineFactory = "Barista.SharePoint.SPBaristaV8ScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52";
            request.Code = code;
            request.CodePath = codePath;

            var instanceSettings = request.ParseInstanceSettings();

            if (String.IsNullOrEmpty(instanceSettings.InstanceInitializationCode) == false)
            {
                string instanceInitializationCodePath;
                request.InstanceInitializationCode = Tamp(instanceSettings.InstanceInitializationCode, out instanceInitializationCodePath);
                request.InstanceInitializationCodePath = instanceInitializationCodePath;
            }

            request.SetExtendedPropertiesFromCurrentSPContext();

            client.Exec(request);

            //abandon the session and set the ASP.net session cookie to nothing.
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
            }
        }

        /// <summary>
        /// Pulls a shot of espresso. E.g. Executes the specified script and sets the appropriate values on the response object.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codePath"></param>
        /// <returns></returns>
        private static Stream Pull(string code, string codePath)
        {
            var client = new BaristaServiceClient(SPServiceContext.Current);

            var request = BrewRequest.CreateServiceApplicationRequestFromHttpRequest(HttpContext.Current.Request);
            request.ScriptEngineFactory = "Barista.SharePoint.SPBaristaV8ScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52";
            request.Code = code;
            request.CodePath = codePath;

            var instanceSettings = request.ParseInstanceSettings();

            if (String.IsNullOrEmpty(instanceSettings.InstanceInitializationCode) == false)
            {
                string instanceInitializationCodePath;
                instanceSettings.InstanceInitializationCode = Tamp(instanceSettings.InstanceInitializationCode, out instanceInitializationCodePath);
                instanceSettings.InstanceInitializationCodePath = instanceInitializationCodePath;
            }

            request.SetExtendedPropertiesFromCurrentSPContext();

            var result = client.Eval(request);

            var setHeaders = true;
            if (WebOperationContext.Current != null)
            {
                result.ModifyOutgoingWebResponse(WebOperationContext.Current.OutgoingResponse);
                setHeaders = false;
            }

            result.ModifyHttpResponse(HttpContext.Current.Response, setHeaders);

            if (result.Content == null)
                return null;

            //abandon the session and set the ASP.net session cookie to nothing.
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Abandon();
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
            }

            var resultStream = new MemoryStream(result.Content);
            return resultStream;
        }
        #endregion
    }
}
