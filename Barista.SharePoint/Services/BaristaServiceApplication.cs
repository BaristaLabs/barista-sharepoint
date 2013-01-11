namespace Barista.SharePoint.Services
{
  using Barista.SharePoint.Bundles;
  using Barista.SharePoint.Search;
  using Jurassic;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Runtime.InteropServices;
  using System.ServiceModel;
  using System.Threading;

  [Guid("9B4C0B5C-8A42-401A-9ACB-42EA6246E960")]
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
  public sealed class BaristaServiceApplication : SPIisWebServiceApplication, IBaristaServiceApplication
  {
    #region Constants

    private const string IndexServerAffinityKey = "Barista_IndexServerAffinity";

    #endregion

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

      Mutex syncRoot = null;

      if (BaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
      {
        syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceName);
      }

      var webBundle = new WebBundle();
      var source = new BaristaScriptSource(request.Code, request.CodePath);

      if (syncRoot != null)
        syncRoot.WaitOne();

      try
      {
        bool isNewScriptEngineInstance;
        bool errorInInitialization;
        var engine = SPScriptEngineFactory.GetScriptEngine(webBundle, out isNewScriptEngineInstance, out errorInInitialization);

        if (errorInInitialization)
          return response;

        try
        {
          var result = engine.Evaluate(source);

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

          response.SetContentsFromResultObject(engine, result, isRaw);
        }
        catch (JavaScriptException ex)
        {
          BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
          SPScriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(ex, response);
        }
        catch (Exception ex)
        {
          BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occurred while evaluating script: ");
          throw;
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

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void Exec(BrewRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      var response = new BrewResponse();

      //Set the current context with information from the current request and response.
      BaristaContext.Current = new BaristaContext(request, response);

      //If we're not executing with Per-Call instancing, create a mutex to synchronize against.
      Mutex syncRoot = null;
      if (BaristaContext.Current.Request.InstanceMode != BaristaInstanceMode.PerCall)
      {
        syncRoot = new Mutex(false, "Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceName);
      }

      WebBundle webBundle = new WebBundle();
      var source = new BaristaScriptSource(request.Code, request.CodePath);

      if (syncRoot != null)
        syncRoot.WaitOne();

      try
      {
        bool isNewScriptEngineInstance;
        bool errorInInitialization;
        var engine = SPScriptEngineFactory.GetScriptEngine(webBundle, out isNewScriptEngineInstance, out errorInInitialization);

        if (errorInInitialization)
          return;

        try
        {
          engine.Execute(source);
        }
        catch (JavaScriptException ex)
        {
          BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.JavaScriptException, "A JavaScript exception was thrown while evaluating script: ");
          SPScriptEngineFactory.UpdateResponseWithJavaScriptExceptionDetails(ex, response);
        }
        catch (Exception ex)
        {
          BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An internal error occured while executing script: ");
          throw;
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

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void AddObjectToIndex(string indexUrl, bool createIndex, string json)
    {
      using (var site = new SPSite(indexUrl))
      {
        using (var web = site.OpenWeb())
        {
          var list = web.GetList(indexUrl);

          //If the root folder of the specified list has the index server affinity property defined, check it against the current machine name.
          if (list.RootFolder.Properties.ContainsKey(IndexServerAffinityKey))
          {
            var serverName = list.RootFolder.Properties[IndexServerAffinityKey] as string;
            if (serverName == null ||
                String.Compare(serverName, Environment.MachineName, StringComparison.InvariantCultureIgnoreCase) != 0)
              return;
          }

          var indexWriter = LuceneHelper.GetIndexWriterSingleton(list.RootFolder, createIndex);
          var jObj = JObject.Parse(json);
          LuceneHelper.AddObjectToIndex(indexWriter, jObj);
        }
      }
    }

    #endregion
  }
}
