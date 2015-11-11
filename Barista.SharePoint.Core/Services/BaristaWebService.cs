namespace Barista.SharePoint.Services
{
    using Barista.Extensions;
    using Barista.Framework;
    using Barista.Newtonsoft.Json;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Client.Services;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

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
            BaristaServiceRequestPipeline.TakeOrder();

            string codePath;
            var code = BaristaServiceRequestPipeline.Grind(requestBody);

            code = BaristaServiceRequestPipeline.Tamp(code, out codePath);

            BaristaServiceRequestPipeline.Brew(code, codePath, requestBody);
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
            BaristaServiceRequestPipeline.TakeOrder();

            string codePath;
            var code = BaristaServiceRequestPipeline.Grind(requestBody);

            code = BaristaServiceRequestPipeline.Tamp(code, out codePath);

            return BaristaServiceRequestPipeline.Pull(code, codePath, requestBody);
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
                {"workingSet", Environment.WorkingSet},
                {"currentThreadId", System.Threading.Thread.CurrentThread.ManagedThreadId }
            };

            //try
            //{
            //    var cpuCounter = new PerformanceCounter
            //    {
            //        CategoryName = "Processor",
            //        CounterName = "% Processor Time",
            //        InstanceName = "_Total"
            //    };

            //    var firstValue = cpuCounter.NextValue();
            //    System.Threading.Thread.Sleep(1000);
            //    environment.Add("processorPct", cpuCounter.NextValue());

            //    PerformanceCounter memoryCounter = new PerformanceCounter
            //    {
            //        CategoryName = "Memory",
            //        CounterName = "Available MBytes"
            //    };

            //    environment.Add("availableMBytes", memoryCounter.NextValue());
            //}
            //catch {/* Do Nothing */}

            result.Add("environment", environment);

            var webContextObject = new JObject();

            var queryParameters = new JObject();
            var requestQueryParameters = webContext.IncomingRequest.UriTemplateMatch.QueryParameters;
            foreach (var key in requestQueryParameters.AllKeys)
                queryParameters.Add(key, requestQueryParameters[key]);
            webContextObject.Add("queryParameters", queryParameters);

            var headers = new JObject();
            var requestHeaders = webContext.IncomingRequest.Headers;
            foreach (var key in requestHeaders.AllKeys)
                headers.Add(key, requestHeaders[key]);
            webContextObject.Add("headers", headers);

            result.Add("webContext", webContextObject);

            var sharepoint = new JObject();

            var servers = new JArray();
            foreach(var spServer in SPFarm.Local.Servers)
            {
                JObject server;
                try
                {
                    server = new JObject
                    {
                        {"id", spServer.Id},
                        {"address", spServer.Address},
                        {"name", spServer.Name},
                        {"needsUpgrade", spServer.NeedsUpgrade},
                        //{"needsUpgradeIncludeChildren", spServer.NeedsUpgradeIncludeChildren},
                        {"version", spServer.Version},
                        {"role", spServer.Role.ToString()}
                    };
                }
                catch (Exception ex)
                {
                    server = new JObject
                    {
                        {"id", spServer.Id},
                        {"address", spServer.Address},
                        {"name", spServer.Name},
                        {"exception", ex.Message},
                        {"stackTrace", ex.StackTrace}
                    };
                }

                servers.Add(server);
            }

            sharepoint.Add("serversInFarm", servers);

            try
            {
                sharepoint.Add("SPBuildVersion", SPFarm.Local.BuildVersion.ToString());
                var baristaService = BaristaHelper.GetBaristaService(SPFarm.Local);
                sharepoint.Add("baristaService", GetSharePointServiceRepresentation(baristaService));
            }
            catch
            {
                //Do Nothing
            }

            try
            {
                var baristaSearchService = BaristaHelper.GetBaristaSearchService(SPFarm.Local);
                sharepoint.Add("baristaSearchService", GetSharePointServiceRepresentation(baristaSearchService));
            }
            catch
            {
                //Do Nothing
            }

            result.Add("sharepoint", sharepoint);

            
            //trusted locations config

            var codeValue = BaristaServiceRequestPipeline.Grind(null, false);
            string scriptPath;
            var whatIf = new JObject();
            whatIf.Add("codeValue", codeValue);
            whatIf.Add("codeContents", BaristaServiceRequestPipeline.Tamp(codeValue, out scriptPath));
            whatIf.Add("finalScriptPath", scriptPath);
            result.Add("whatIf", whatIf);

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
        public Message ListPackages()
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
                    var result = proxy.ListPackages();
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
        public Message DeployPackage(Stream requestBody)
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
                    var result = proxy.AddPackage(requestBody.ToByteArray());
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
