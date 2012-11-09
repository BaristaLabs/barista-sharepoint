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
  using System.Runtime.InteropServices;
  using System.ServiceModel;
  using System.Web;

  [Guid("9B4C0B5C-8A42-401A-9ACB-42EA6246E960")]
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
  internal sealed class BaristaServiceApplication : SPIisWebServiceApplication, IBaristaServiceApplication
  {
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
      var response = new BrewResponse();

      SetBaristaContextFromRequest(request);

      WebBundle webBundle = new WebBundle(request, response);
      var engine = GetScriptEngine(webBundle);

      object result = null;
      try
      {
        result = engine.Evaluate(request.Code);
      }
      catch (Exception ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An error occured while evaluating script: ");
        throw;
      }

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
      response.SetContentsFromResultObject(engine, result,isRaw);
      return response;
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    public void Exec(BrewRequest request)
    {
      var response = new BrewResponse();

      SetBaristaContextFromRequest(request);

      WebBundle webBundle = new WebBundle(request, response);
      var engine = GetScriptEngine(webBundle);

      try
      {
        engine.Execute(request.Code);
      }
      catch (Exception ex)
      {
        BaristaDiagnosticsService.Local.LogException(ex, BaristaDiagnosticCategory.Runtime, "An error occured while executing script: ");
        throw;
      }
    }

    private void SetBaristaContextFromRequest(BrewRequest request)
    {
      BaristaContext context = new BaristaContext();

      if (request.ExtendedProperties != null && request.ExtendedProperties.ContainsKey("SPSiteId"))
      {
        Guid siteId = new Guid(request.ExtendedProperties["SPSiteId"]);

        if (siteId != Guid.Empty)
          context.Site = new SPSite(siteId);

        if (request.ExtendedProperties.ContainsKey("SPWebId"))
        {
          Guid webId = new Guid(request.ExtendedProperties["SPWebId"]);

          if (webId != Guid.Empty)
            context.Web = context.Site.OpenWeb(webId);

          if (request.ExtendedProperties.ContainsKey("SPListId"))
          {
            Guid listId = new Guid(request.ExtendedProperties["SPListId"]);

            if (listId != Guid.Empty)
              context.List = context.Web.Lists[listId];

            if (request.ExtendedProperties.ContainsKey("SPViewId"))
            {
              Guid viewId = new Guid(request.ExtendedProperties["SPViewId"]);

              if (viewId != Guid.Empty)
                context.View = context.List.Views[viewId];
            }
          }

          if (request.ExtendedProperties.ContainsKey("SPListItemUrl"))
          {
            string url = request.ExtendedProperties["SPListItemUrl"];

            if (String.IsNullOrEmpty(url) == false)
              context.ListItem = context.Web.GetListItem(url);
          }

          if (request.ExtendedProperties.ContainsKey("SPFileId"))
          {
            Guid fileId = new Guid(request.ExtendedProperties["SPFileId"]);

            if (fileId != Guid.Empty)
              context.File = context.Web.GetFile(fileId);
          }
        }
      }
      BaristaContext.Current = context;
    }

    /// <summary>
    /// Returns a new instance of a script engine object with all runtime objects available.
    /// </summary>
    /// <returns></returns>
    private ScriptEngine GetScriptEngine(WebBundle webBundle)
    {
      var engine = new Jurassic.ScriptEngine();
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

      //Global Types

      //engine.SetGlobalValue("file", new FileSystemInstance(engine));

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
        bool isHiveFile;
        string code;
        if (SPHelper.TryGetSPFileAsString(scriptUrl, out code, out isHiveFile))
        {
          code = SPHelper.ReplaceTokens(code);
          engine.Execute(code);
          return;
        }

        throw new JavaScriptException(engine, "Error", "Could not locate the specified script file:  " + scriptUrl);
      }));

      engine.SetGlobalFunction("include", new Func<string, object>((scriptUrl) =>
      {
        bool isHiveFile;
        string code;
        if (SPHelper.TryGetSPFileAsString(scriptUrl, out code, out isHiveFile))
        {
          code = SPHelper.ReplaceTokens(code);
          return engine.Evaluate(code);
        }

        throw new JavaScriptException(engine, "Error", "Could not locate the specified script file:  " + scriptUrl);
      }));
      return engine;
    }
    #endregion
  }
}
