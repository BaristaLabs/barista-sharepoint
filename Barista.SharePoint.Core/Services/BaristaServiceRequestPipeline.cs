namespace Barista.SharePoint.Services
{
    using Barista.Extensions;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.SharePoint.Extensions;
    using Microsoft.SharePoint;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Web;

    public sealed class BaristaServiceRequestPipeline
    {
        /// <summary>
        /// Takes an order. E.g. Asserts that the current web is valid, the user has read permissions on the current web, and the current web is a trusted location.
        /// </summary>
        public static void TakeOrder()
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
        public static string Grind(Stream requestBody, bool shouldThrowIfNotFound = true)
        {
            string code = null;

            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            //If the request has a header named "X-Barista-Code" use that first.
            var requestHeaders = webContext.IncomingRequest.Headers;
            var codeHeaderKey = requestHeaders.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "X-Barista-Code".ToLowerInvariant());
            if (codeHeaderKey != null)
                code = requestHeaders[codeHeaderKey];

            //If the request has a query string parameter named "c" use that.
            if (string.IsNullOrEmpty(code) && webContext.IncomingRequest.UriTemplateMatch != null)
            {
                var requestQueryParameters = webContext.IncomingRequest.UriTemplateMatch.QueryParameters;

                var codeKey = requestQueryParameters.AllKeys.FirstOrDefault(k => k != null && k.ToLowerInvariant() == "c");
                if (codeKey != null)
                    code = requestQueryParameters[codeKey];
            }

            //If the request contains a Message header named "c" or "code in the appropriate namespace, use that.
            if (string.IsNullOrEmpty(code))
            {
                var headers = OperationContext.Current.IncomingMessageHeaders;

                if (headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "code"))
                    code = headers.GetHeader<string>("code", Barista.Constants.ServiceNamespace);
                else if (string.IsNullOrEmpty(code) && headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "c"))
                    code = headers.GetHeader<string>("c", Barista.Constants.ServiceNamespace);
            }

            //Otherwise, use the body of the post as code.
            if (string.IsNullOrEmpty(code) && requestBody != null && webContext.IncomingRequest.Method == "POST")
            {
                var body = requestBody.ToByteArray();
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
                    if (string.IsNullOrEmpty(code))
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

            //Last Chance: attempt to use everything after eval/ or exec/ in the url as the code
            if (string.IsNullOrEmpty(code))
            {
                var url = webContext.IncomingRequest.UriTemplateMatch;

                if (url == null)
                    throw new InvalidOperationException("UriTemplateMatch of the incoming request was null.");

                var firstWildcardPathSegment = url.WildcardPathSegments.Join("/");
                if (!firstWildcardPathSegment.IsNullOrWhiteSpace())
                    code = firstWildcardPathSegment;
            }

            if (shouldThrowIfNotFound && string.IsNullOrEmpty(code))
                throw new InvalidOperationException("Code must be specified either through the 'X-Barista-Code' header, through a 'c' or 'code' soap message header in the http://barista/services/v1 namespace, a 'c' query string parameter or the 'code' or 'c' form field that contains either a literal script declaration or a relative or absolute path to a script file.");

            return code;
        }


        /// <summary>
        /// Tamps the ground coffee. E.g. Parses the code and makes it ready to be executed (brewed).
        /// </summary>
        /// <param name="code"></param>
        /// <param name="scriptPath"></param>
        /// <returns></returns>
        public static string Tamp(string code, out string scriptPath)
        {
            scriptPath = string.Empty;
            if (String.IsNullOrWhiteSpace(code))
                return String.Empty;

            //If the code looks like a uri to a .js or .jsx file, attempt to retrieve a code file and use the contents of that file as the code.
            if (code.Length < 2048 &&
                (code.EndsWith(".js", StringComparison.OrdinalIgnoreCase) || code.EndsWith(".jsx", StringComparison.OrdinalIgnoreCase)) &&
                Uri.IsWellFormedUriString(code, UriKind.RelativeOrAbsolute))
            {
                Uri codeUri;
                if (Uri.TryCreate(code, UriKind.RelativeOrAbsolute, out codeUri))
                {
                    string scriptFilePath;
                    bool isHiveFile;
                    string codeFromfile;
                    if (SPHelper.TryGetSPFileAsString(code, out scriptFilePath, out codeFromfile, out isHiveFile))
                    {
                        scriptPath = scriptFilePath;
                        code = codeFromfile;
                    }
                }
            }

            //If the request has a header named "X-Barista-ReplaceSPTokensInCode" then replace.
            var webContext = WebOperationContext.Current;

            if (webContext != null)
            {
                var requestHeaders = webContext.IncomingRequest.Headers;
                var codeHeaderKey =
                    requestHeaders.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "X-Barista-ReplaceSPTokensInCode".ToLowerInvariant());

                //If the header exists, replace any tokens in the code.
                if (!codeHeaderKey.IsNullOrWhiteSpace())
                {
                    code = SPHelper.ReplaceTokens(SPContext.Current, code);
                }
            }

            return code;
        }

        /// <summary>
        /// Brews a cup of coffee. E.g. Executes the specified script.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="codePath"></param>
        /// <param name="requestBody"></param>
        public static void Brew(string code, string codePath, Stream requestBody, string scriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52")
        {
            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            var request = BrewRequest.CreateBrewRequestFromIncomingWebRequest(webContext.IncomingRequest, requestBody, OperationContext.Current);
            request.ScriptEngineFactory = scriptEngineFactory;
            request.Code = code;
            request.CodePath = codePath;

            var instanceSettings = request.ParseInstanceSettings();

            if (string.IsNullOrEmpty(instanceSettings.InstanceInitializationCode) == false)
            {
                string instanceInitializationCodePath;
                request.InstanceInitializationCode = Tamp(instanceSettings.InstanceInitializationCode, out instanceInitializationCodePath);
                request.InstanceInitializationCodePath = instanceInitializationCodePath;
            }

            request.SetExtendedPropertiesFromCurrentSPContext();
            var client = new BaristaServiceClient(SPServiceContext.Current);
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
        /// <param name="requestBody"></param>
        /// <returns></returns>
        public static Message Pull(string code, string codePath, Stream requestBody, string scriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52")
        {
            var webContext = WebOperationContext.Current;
        
            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            var request = BrewRequest.CreateBrewRequestFromIncomingWebRequest(webContext.IncomingRequest, requestBody, OperationContext.Current);
            request.ScriptEngineFactory = scriptEngineFactory;
            request.Code = code;
            request.CodePath = codePath;

            var instanceSettings = request.ParseInstanceSettings();

            if (string.IsNullOrEmpty(instanceSettings.InstanceInitializationCode) == false)
            {
                string instanceInitializationCodePath;
                request.InstanceInitializationCode = Tamp(instanceSettings.InstanceInitializationCode, out instanceInitializationCodePath);
                request.InstanceInitializationCodePath = instanceInitializationCodePath;
            }

            request.SetExtendedPropertiesFromCurrentSPContext();

            //Make a call to the Barista Service application to handle the request.
            var client = new BaristaServiceClient(SPServiceContext.Current);
            var result = client.Eval(request);

            //abandon the session and set the ASP.net session cookie to nothing.
            var cookies = new List<IBaristaCookie>();
            cookies.AddRange(result.Cookies);

            var existingCookie = cookies.FirstOrDefault(c => c != null && c.Name == "ASP.NET_SessionId");
            if (existingCookie != null)
                cookies.Remove(existingCookie);
            cookies.Add(new Biscotti("ASP.NET_SessionId", ""));
            result.Cookies = cookies;

            result.ModifyOutgoingWebResponse(webContext.OutgoingResponse);

            //if (HttpContext.Current != null && HttpContext.Current.Session != null)
            //{
            //    HttpContext.Current.Session.Abandon();
            //    HttpContext.Current.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", ""));
            //}

            return webContext.CreateStreamResponse(
                stream =>
                {
                    if (result.Content == null)
                        return;

                    using (var writer = new BinaryWriter(stream))
                    {
                        writer.Write(result.Content);
                    }
                },
                result.ContentType ?? string.Empty);
        }
    }
}
