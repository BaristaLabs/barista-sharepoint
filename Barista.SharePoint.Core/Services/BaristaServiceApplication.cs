namespace Barista.SharePoint.Services
{
    using Barista.Engine;
    using Barista.Extensions;
    using Barista.Helpers;
    using Barista.SharePoint.Bundles;
    using Jurassic;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using Barista.Newtonsoft.Json;
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using Barista.Newtonsoft.Json.Linq;

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
                ContentType = request.Headers.ContentType
            };

            SPBaristaContext.Current = new SPBaristaContext(request, response);
            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            Mutex syncRoot = null;

            if (instanceSettings.InstanceMode != BaristaInstanceMode.PerCall)
            {
                syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + instanceSettings.InstanceName);
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
                    //Default execution timeout to be 60 seconds.
                    if (request.ExecutionTimeout <= 0)
                        request.ExecutionTimeout = 60 * 1000;

                    object result = null;
                    using (new SPMonitoredScope("Barista Script Eval", request.ExecutionTimeout,
                              new SPCriticalTraceCounter(),
                              new SPExecutionTimeCounter(request.ExecutionTimeout),
                              new SPRequestUsageCounter(),
                              new SPSqlQueryCounter()))
                    {
                        //var mre = new ManualResetEvent(false);

                        //var scopeEngine = engine;
                        //var actionThread = new Thread(() =>
                        //{
                        //    result = scopeEngine.Evaluate(source); //always call endinvoke
                        //    mre.Set();
                        //});

                        //actionThread.Start();
                        //mre.WaitOne(TimeSpan.FromMilliseconds(request.ExecutionTimeout));
                        //if (actionThread.IsAlive)
                        //    actionThread.Abort();

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
                            var clientInfo = br.Parse(request.Headers.UserAgent);

                            if (clientInfo.UserAgent.Family == "IE" && (clientInfo.UserAgent.Major == "7" || clientInfo.UserAgent.Major == "8"))
                                response.Headers.Add("Content-Disposition", "attachment; filename=" + Barista.Helpers.HttpUtility.UrlEncode(arrayResult.FileName));
                            else if (clientInfo.UserAgent.Family == "Safari")
                                response.Headers.Add("Content-Disposition", "attachment; filename=" + arrayResult.FileName);
                            else
                                response.Headers.Add("Content-Disposition", "attachment; filename=\"" + Barista.Helpers.HttpUtility.UrlEncode(arrayResult.FileName) + "\"");
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
                ContentType = request.Headers.ContentType
            };

            //Set the current context with information from the current request and response.
            SPBaristaContext.Current = new SPBaristaContext(request, response);
            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            //If we're not executing with Per-Call instancing, create a mutex to synchronize against.
            Mutex syncRoot = null;
            if (instanceSettings.InstanceMode != BaristaInstanceMode.PerCall)
            {
                syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + instanceSettings.InstanceName);
            }

            SPBaristaContext.Current.WebBundle = new SPWebBundle();
            var source = new BaristaScriptSource(request.Code, request.CodePath);

            if (syncRoot != null)
                syncRoot.WaitOne();

            try
            {
                bool isNewScriptEngineInstance;
                bool errorInInitialization;

                var baristaScriptEngineFactoryType = Type.GetType("Barista.SharePoint.SPBaristaScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
                    true);

                if (baristaScriptEngineFactoryType == null)
                    throw new InvalidOperationException("Unable to locate the SPBraistaScriptEngineFactory");

                var scriptEngineFactory =
                    (ScriptEngineFactory)Activator.CreateInstance(baristaScriptEngineFactoryType);

                var engine = scriptEngineFactory.GetScriptEngine(SPBaristaContext.Current.WebBundle, out isNewScriptEngineInstance,
                                                                 out errorInInitialization);

                if (engine == null)
                    throw new InvalidOperationException("Unable to obtain an instance of a Script Engine.");

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

        /// <summary>
        /// Lists the packages installed in the /WebServices/Barista/bin/Bundles folder
        /// </summary>
        /// <returns></returns>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public string ListPackages()
        {
            var objResult = BaristaHelper.ListPackages();

            return objResult.ToString(Formatting.Indented);
        }

        /// <summary>
        /// Expand the specified bundle package to the [hive]/WebServices/Barista/bin/Bundles folder.
        /// </summary>
        /// <remarks>
        ///  The caller should take care of calling this on each application server in the farm.
        /// </remarks>
        /// <param name="bundlePackage"></param>
        /// <returns></returns>
        [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
        public string AddPackage(byte[] bundlePackage)
        {
            var objResult = new JObject
            {
                {"!machineName", Environment.MachineName}
            };

            //Get the name/version of the bundle
            using(var ms = new MemoryStream(bundlePackage))
            {
                var package = GetPackageFileInPackage(ms);

                if (package == null)
                    throw new InvalidOperationException("Unable to locate a package.json file within the bundle package. The package.json file must be located at the root of the zip file and be valid Json.");

                if (!Package.IsValidPackage(package))
                    throw new InvalidOperationException("The bundle package contained an invalid package.json file.");

                var pkg = package.ToObject<Package>();
                var bundlePath = Path.Combine(InstallPath, "bin/Bundles");
                var pkgPath = Path.Combine(bundlePath, pkg.Name + "_" + pkg.Version);

                try
                {
                    var packageDirectory = new DirectoryInfo(pkgPath);
                    if (packageDirectory.Exists)
                        packageDirectory.Delete(true);

                    packageDirectory.Create();
                    ExtractZipFile(ms, packageDirectory.ToString());
                    objResult.Add(packageDirectory.Name, JToken.FromObject(pkg));
                }
                catch(Exception ex)
                {
                    objResult.Add("error", JToken.FromObject(ex));
                }

                return objResult.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Returns the package file (package.json) in the archive. If an error or no package.json file exists, null is returned.
        /// </summary>
        /// <param name="archiveIn"></param>
        /// <returns></returns>
        private static JObject GetPackageFileInPackage(Stream archiveIn)
        {
            ZipFile zf = null;
            try
            {
                zf = new ZipFile(archiveIn);

                foreach (var zipEntry in zf.OfType<ZipEntry>().Where(zipEntry => zipEntry.IsFile && string.Equals("package.json", zipEntry.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    using (var zipEntryStream = zf.GetInputStream(zipEntry))
                    {
                        using (var sr = new StreamReader(zipEntryStream))
                        {
                            using (var jr = new JsonTextReader(sr))
                            {
                                try
                                {
                                    var result = JObject.Load(jr);
                                    return result;
                                }
                                catch
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = false;
                    zf.Close(); // Ensure we release resources
                }

                archiveIn.Position = 0;
            }

            return null;
        }

        private static void ExtractZipFile(Stream archiveIn, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                zf = new ZipFile(archiveIn);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }

                    var entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    var buffer = new byte[4096];     // 4K is optimum
                    var zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    var fullZipToPath = System.IO.Path.Combine(outFolder, entryFileName);
                    var directoryName = System.IO.Path.GetDirectoryName(fullZipToPath);
                    if (directoryName == null)
                        throw new InvalidOperationException("Unable to obtain the directory.");

                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = false;
                    zf.Close(); // Ensure we release resources
                }

                archiveIn.Position = 0;
            }
        }
    }
}