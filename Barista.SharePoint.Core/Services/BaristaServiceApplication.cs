namespace Barista.SharePoint.Services
{
    using Barista.Engine;
    using Barista.Extensions;
    using Barista.Helpers;
    using Barista.SharePoint.Bundles;
    using Jurassic;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Web;

    [Guid("9B4C0B5C-8A42-401A-9ACB-42EA6246E960")]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public sealed class BaristaServiceApplication : SPIisWebServiceApplication, IBaristaServiceApplication
    {
        #region Fields
        [Persisted]
        private int m_settings;

        #endregion

        public int Settings
        {
            get { return m_settings; }
            set { m_settings = value; }
        }

        public BaristaServiceApplication()
        { }

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
            var serviceApplication = new BaristaServiceApplication(name, service, appPool);
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
            get { return SPUtility.GetVersionedGenericSetupPath(@"WebServices\Barista", SPUtility.ContextCompatibilityLevel); }
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
            {
                return new SPAdministrationLink(String.Concat("/_admin/BaristaService/Manage.aspx?appid=", Id));
            }
        }

        public override SPAdministrationLink PropertiesLink
        {
            get
            {
                return new SPAdministrationLink(String.Concat("/_admin/BaristaService/Manage.aspx?appid=", Id));
            }
        }

        #endregion

        #region IBaristaServiceApplication implementation
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public BrewResponse Eval(BrewRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var response = new BrewResponse
            {
                ContentType = request.ContentType
            };

            SPBaristaContext.Current = new SPBaristaContext(request, response);

            Mutex syncRoot = null;

            if (SPBaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
            {
                syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + SPBaristaContext.Current.Request.InstanceName);
            }

            var webBundle = new SPWebBundle();
            var source = new BaristaScriptSource(request.Code, request.CodePath);

            if (syncRoot != null)
                syncRoot.WaitOne();

            try
            {
                bool isNewScriptEngineInstance;
                bool errorInInitialization;

                if (request.ScriptEngineFactory.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("A ScriptEngineFactory must be specified as part of a BrewRequest.");

                var baristaScriptEngineFactoryType = Type.GetType(request.ScriptEngineFactory, true);

                if (baristaScriptEngineFactoryType == null)
                    throw new InvalidOperationException("Unable to locate the specified ScriptEngineFactory: " + request.ScriptEngineFactory);

                var scriptEngineFactory =
                    (ScriptEngineFactory)Activator.CreateInstance(baristaScriptEngineFactoryType);

                var engine = scriptEngineFactory.GetScriptEngine(webBundle, out isNewScriptEngineInstance,
                    out errorInInitialization);

                if (engine == null)
                    throw new InvalidOperationException("Unable to obtain a script engine instance.");

                if (errorInInitialization)
                    return response;

                try
                {
                    object result;
                    using (new SPMonitoredScope("Barista Script Eval", 110000,
                              new SPCriticalTraceCounter(),
                              new SPExecutionTimeCounter(11000),
                              new SPRequestUsageCounter(),
                              new SPSqlQueryCounter()))
                    {
                        result = engine.Evaluate(source);
                    }

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
                    try
                    {
                        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException,
                          "A JavaScript exception was thrown while evaluating script: ");
                    }
                    catch
                    {
                        //Do Nothing...
                    }

                    scriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(engine, ex, response);
                }
                catch (Exception ex)
                {
                    try
                    {
                        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime,
                          "An internal error occurred while evaluating script: ");
                    }
                    catch
                    {
                        //Do Nothing...
                    }
                    scriptEngineFactory.UpdateResponseWithExceptionDetails(ex, response);
                }
                finally
                {
                    var engineDisposable = engine as IDisposable;
                    if (engineDisposable != null)
                        engineDisposable.Dispose();                      

                    //Cleanup
                    // ReSharper disable RedundantAssignment
                    engine = null;
                    // ReSharper restore RedundantAssignment

                    if (SPBaristaContext.Current != null)
                        SPBaristaContext.Current.Dispose();

                    SPBaristaContext.Current = null;
                }
            }
            finally
            {
                if (syncRoot != null)
                    syncRoot.ReleaseMutex();
            }

            return response;
        }

        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public void Exec(BrewRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var response = new BrewResponse
            {
                ContentType = request.ContentType
            };

            //Set the current context with information from the current request and response.
            SPBaristaContext.Current = new SPBaristaContext(request, response);

            //If we're not executing with Per-Call instancing, create a mutex to synchronize against.
            Mutex syncRoot = null;
            if (SPBaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
            {
                syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + SPBaristaContext.Current.Request.InstanceName);
            }

            SPBaristaContext.Current.WebBundle = new SPWebBundle();
            var source = new BaristaScriptSource(request.Code, request.CodePath);

            if (syncRoot != null)
                syncRoot.WaitOne();

            try
            {
                bool isNewScriptEngineInstance;
                bool errorInInitialization;

                if (request.ScriptEngineFactory.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("A ScriptEngineFactory must be specified as part of a BrewRequest.");

                var baristaScriptEngineFactoryType = Type.GetType(request.ScriptEngineFactory, true);

                if (baristaScriptEngineFactoryType == null)
                    throw new InvalidOperationException("Unable to locate the specified ScriptEngineFactory: " + request.ScriptEngineFactory);

                var scriptEngineFactory =
                    (ScriptEngineFactory)Activator.CreateInstance(baristaScriptEngineFactoryType);

                var engine = scriptEngineFactory.GetScriptEngine(SPBaristaContext.Current.WebBundle, out isNewScriptEngineInstance,
                    out errorInInitialization);

                if (engine == null)
                    throw new InvalidOperationException("Unable to obtain a script engine instance.");

                if (errorInInitialization)
                    return;

                try
                {
                    using (new SPMonitoredScope("Barista Script Exec", 110000,
                                                new SPCriticalTraceCounter(),
                                                new SPExecutionTimeCounter(),
                                                new SPRequestUsageCounter(),
                                                new SPSqlQueryCounter()))
                    {
                        engine.Evaluate(source);
                    }
                }
                catch (JavaScriptException ex)
                {
                    try
                    {
                        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException,
                          "A JavaScript exception was thrown while evaluating script: ");
                    }
                    catch
                    {
                        //Do Nothing...
                    }
                    scriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(engine, ex, response);
                }
                catch (Exception ex)
                {
                    try
                    {
                        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime,
                          "An internal error occured while executing script: ");
                    }
                    catch
                    {
                        //Do Nothing...
                    }

                    scriptEngineFactory.UpdateResponseWithExceptionDetails(ex, response);
                }
                finally
                {
                    //Cleanup
                    // ReSharper disable RedundantAssignment
                    engine = null;
                    // ReSharper restore RedundantAssignment

                    if (SPBaristaContext.Current != null)
                        SPBaristaContext.Current.Dispose();

                    SPBaristaContext.Current = null;
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