namespace Barista.SharePoint.Services
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using Barista.Newtonsoft.Json;
    using Barista.SharePoint.Extensions;
    using Barista.Extensions;
    using Barista.Framework;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
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
    public class BaristaWebService : IBaristaRestService
    {
        #region Service Operations

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public void Exec(Stream requestBody)
        {
            ExecWild(requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public void ExecWild(Stream requestBody)
        {
            TakeOrder();

            string codePath;
            var code = Grind(requestBody);

            code = Tamp(code, out codePath);

            Brew(code, codePath, requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message Eval(Stream requestBody)
        {
            return EvalWild(requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message EvalWild(Stream requestBody)
        {
            TakeOrder();

            string codePath;
            var code = Grind(requestBody);

            code = Tamp(code, out codePath);

            return Pull(code, codePath, requestBody);
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message Status()
        {
            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            var result = new JObject();

            var environment = new JObject
            {
                {"commandLine", Environment.CommandLine},
                {"currentDirectory", Environment.CurrentDirectory},
                {"machineName", Environment.MachineName},
                {"newLine", Environment.NewLine},
                {
                    "osVersion", new JObject
                    {
                        {"platform", Environment.OSVersion.Platform.ToString()},
                        {"servicePack", Environment.OSVersion.ServicePack},
                        {"version", Environment.OSVersion.Version.ToString()},
                        {"versionString", Environment.OSVersion.VersionString}
                    }
                },
                {"processorCount", Environment.ProcessorCount},
                {"systemDirectory", Environment.SystemDirectory},
                {"tickCount", Environment.TickCount},
                {"userDomainName", Environment.UserDomainName},
                {"userInteractive", Environment.UserInteractive},
                {"userName", Environment.UserName},
                {"version", Environment.Version.ToString()},
                {"workingSet", Environment.WorkingSet}
            };


            result.Add("environment", environment);

            var sharepoint = new JObject();

            var servers = new JArray();
            foreach(var spServer in SPFarm.Local.Servers)
            {
                var server = new JObject
                {
                    {"id", spServer.Id},
                    {"address", spServer.Address},
                    {"name", spServer.Name},
                    {"needsUpgrade", spServer.NeedsUpgrade},
                    {"needsUpgradeIncludeChildren", spServer.NeedsUpgradeIncludeChildren},
                    {"version", spServer.Version},
                    {"role", spServer.Role.ToString()}
                };

                servers.Add(server);
            }
            sharepoint.Add("serversInFarm", servers);

            var baristaService = BaristaHelper.GetBaristaService(SPFarm.Local);
            sharepoint.Add("baristaService", GetSharePointServiceRepresentation(baristaService));

            var baristaSearchService = BaristaHelper.GetBaristaSearchService(SPFarm.Local);
            sharepoint.Add("baristaSearchService", GetSharePointServiceRepresentation(baristaSearchService));

            result.Add("sharepoint", sharepoint);

            
            //trusted locations config

            return webContext.CreateStreamResponse(
                stream =>
                {
                    var js = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };

                    using(var sw = new StreamWriter(stream))
                    {
                        js.Serialize(sw, result);
                    }
                },
                "application/json");
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message ListBundles()
        {
            var webContext = WebOperationContext.Current;
            var baristaProxies =
                SPServiceContext.Current.GetProxies(typeof(BaristaServiceApplicationProxy))
                    .OfType<BaristaServiceApplicationProxy>();

            var objResult = new JArray();
            foreach (var proxy in baristaProxies)
            {
                using (new SPServiceContextScope(SPServiceContext.Current))
                {
                    var result = proxy.ListBundles();
                    objResult.Add(JToken.Parse(result));
                }
            }

            return webContext.CreateStreamResponse(
                stream =>
                {
                    var js = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };

                    using (var sw = new StreamWriter(stream))
                    {
                        js.Serialize(sw, objResult);
                    }
                },
                "application/json");
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        [DynamicResponseType(RestOnly = true)]
        public Message DeployBundle(Stream requestBody)
        {
            var webContext = WebOperationContext.Current;
            var baristaProxies =
                SPServiceContext.Current.GetProxies(typeof (BaristaServiceApplicationProxy))
                    .OfType<BaristaServiceApplicationProxy>();

            var objResult = new JArray();
            foreach(var proxy in baristaProxies)
            {
                using (new SPServiceContextScope(SPServiceContext.Current))
                {
                    var result = proxy.DeployBundle(requestBody.ToByteArray());
                    objResult.Add(JToken.Parse(result));
                }
            }

            return webContext.CreateStreamResponse(
                stream =>
                {
                    var js = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };

                    using (var sw = new StreamWriter(stream))
                    {
                        js.Serialize(sw, objResult);
                    }

                },
                "application/json");
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
        private static string Grind(Stream requestBody)
        {
            string code = null;

            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            //If the request has a header named "X-Barista-Code" use that first.
            var requestHeaders = webContext.IncomingRequest.Headers;
            var codeHeaderKey = requestHeaders.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "X-Barista-Code");
            if (codeHeaderKey != null)
                code = requestHeaders[codeHeaderKey];

            //If the request has a query string parameter named "c" use that.
            if (String.IsNullOrEmpty(code) && webContext.IncomingRequest.UriTemplateMatch != null)
            {
                var requestQueryParameters = webContext.IncomingRequest.UriTemplateMatch.QueryParameters;

                var codeKey = requestQueryParameters.AllKeys.FirstOrDefault(k => k != null && k.ToLowerInvariant() == "c");
                if (codeKey != null)
                    code = requestQueryParameters[codeKey];
            }

            //If the request contains a Message header named "c" or "code in the appropriate namespace, use that.
            if (String.IsNullOrEmpty(code))
            {
                var headers = OperationContext.Current.IncomingMessageHeaders;
                
                if (headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "code"))
                    code = headers.GetHeader<string>("code", Barista.Constants.ServiceNamespace);
                else if (String.IsNullOrEmpty(code) && headers.Any(h => h.Namespace == Barista.Constants.ServiceNamespace && h.Name == "c"))
                    code = headers.GetHeader<string>("c", Barista.Constants.ServiceNamespace);
            }

            //Otherwise, use the body of the post as code.
            if (String.IsNullOrEmpty(code) && webContext.IncomingRequest.Method == "POST")
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

            //Last Chance: attempt to use everything after eval/ or exec/ in the url as the code
            if (String.IsNullOrEmpty(code))
            {
                var url = webContext.IncomingRequest.UriTemplateMatch;

                if (url == null)
                    throw new InvalidOperationException("UriTemplateMatch of the incoming request was null.");

                var firstWildcardPathSegment = url.WildcardPathSegments.FirstOrDefault();
                if (!firstWildcardPathSegment.IsNullOrWhiteSpace())
                    code = firstWildcardPathSegment;
            }

            if (String.IsNullOrEmpty(code))
                throw new InvalidOperationException("Code must be specified either through the 'X-Barista-Code' header, through a 'c' or 'code' soap message header in the http://barista/services/v1 namespace, a 'c' query string parameter or the 'code' or 'c' form field that contains either a literal script declaration or a relative or absolute path to a script file.");

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
        /// <param name="requestBody"></param>
        private static void Brew(string code, string codePath, Stream requestBody)
        {
            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            var request = BrewRequest.CreateBrewRequestFromIncomingWebRequest(webContext.IncomingRequest, requestBody, OperationContext.Current);
            request.ScriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52";
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
        private static Message Pull(string code, string codePath, Stream requestBody)
        {
            var webContext = WebOperationContext.Current;

            if (webContext == null)
                throw new InvalidOperationException("Current WebOperationContext is null.");

            var request = BrewRequest.CreateBrewRequestFromIncomingWebRequest(webContext.IncomingRequest, requestBody, OperationContext.Current);
            request.ScriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52";
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

        private JObject GetSharePointServiceRepresentation(SPService service)
        {
            var objBaristaService = new JObject
            {
                {"displayName", service.DisplayName},
                {"id", service.Id},
                {"status", service.Status.ToString()}
            };

            var serviceApplications = new JArray();
            foreach (var serviceApplication in service.Applications)
            {
                var objServiceApplication = new JObject
                {
                    {"displayName", serviceApplication.DisplayName},
                    {"id", serviceApplication.Id},
                    {"status", serviceApplication.Status.ToString()}
                };

                var serviceInstances = new JArray();
                foreach (var serviceInstance in serviceApplication.ServiceInstances)
                {
                    var objServiceInstance = new JObject
                    {
                        {"id", serviceInstance.Id},
                        {"serverAddress", serviceInstance.Server.Address},
                        {"name", serviceInstance.Server.Name},
                        {"status", serviceInstance.Status.ToString()}
                    };
                    
                    serviceInstances.Add(objServiceInstance);
                }

                objServiceApplication.Add("serviceInstances", serviceInstances);
                serviceApplications.Add(objServiceApplication);
            }
            objBaristaService.Add("serviceApplications", serviceApplications);

            var localServiceInstances = new JArray();
            foreach(var instance in service.Instances)
            {
                var objServiceInstance = new JObject
                    {
                        {"id", instance.Id},
                        {"serverAddress", instance.Server.Address},
                        {"name", instance.Server.Name},
                        {"status", instance.Status.ToString()}
                    };
                localServiceInstances.Add(objServiceInstance);
            }
            objBaristaService.Add("serviceInstances", localServiceInstances);

            return objBaristaService;
        }
        #endregion
    }
}
