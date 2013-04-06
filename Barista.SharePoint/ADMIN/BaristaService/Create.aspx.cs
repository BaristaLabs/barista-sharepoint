namespace Barista.SharePoint
{
  using Barista.SharePoint.Services;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.WebControls;
  using System;
  using System.Web.UI.WebControls;

  public partial class Create : LayoutsPageBase
  {
    // page web controls
    protected IisWebServiceApplicationPoolSection ApplicationPoolSelection;
    protected InputFormTextBox ServiceAppName;
    protected CheckBox DefaultServiceApp;

    // wire up event on the OK button
    protected override void OnInit(EventArgs e)
    {
      base.OnInit(e);

      // get reference to OK button on dialog master page & wire up handler to it's OK button
      ((DialogMaster)this.Page.Master).OkButton.Click += new EventHandler(OkButton_Click);
    }

    void OkButton_Click(object sender, EventArgs e)
    {
      // create the service app
      SetupBaristaServiceApp();

      // finish call
      SendResponseForPopUI();
    }

    private void SetupBaristaServiceApp()
    {
      // create a long running op..
      using (SPLongOperation op = new SPLongOperation(this))
      {
        op.Begin();

        try
        {
          // get reference to the installed service
          BaristaService service = SPFarm.Local.Services.GetValue<BaristaService>();

          // create the service application
          BaristaServiceApplication serviceApp = CreateServiceApplication(service);

          // if the service instance isn't running, start it up
          StartServiceInstances();

          // create service app proxy
          CreateServiceApplicationProxy(serviceApp);
        }
        catch (Exception e)
        {
          throw new SPException("Error creating Barista service application.", e);
        }
      }
    }

    private BaristaServiceApplication CreateServiceApplication(BaristaService service)
    {
      // create service app
      BaristaServiceApplication serviceApp = BaristaServiceApplication.Create(
          ServiceAppName.Text,
          service,
          ApplicationPoolSelection.GetOrCreateApplicationPool());
      serviceApp.Update();

      // start it if it isn't already started
      if (serviceApp.Status != SPObjectStatus.Online)
        serviceApp.Status = SPObjectStatus.Online;

      // configure service app endpoint
      serviceApp.AddServiceEndpoint(string.Empty, SPIisWebServiceBindingType.Http);
      serviceApp.Update(true);

      // now provision the service app
      serviceApp.Provision();
      return serviceApp;
    }

    private void CreateServiceApplicationProxy(BaristaServiceApplication serviceApp)
    {
      // get reference to the installed service proxy
      BaristaServiceProxy serviceProxy = SPFarm.Local.ServiceProxies.GetValue<BaristaServiceProxy>();

      // create service app proxy
      BaristaServiceApplicationProxy serviceAppProxy = new BaristaServiceApplicationProxy(
          ServiceAppName.Text + " Proxy",
          serviceProxy,
          serviceApp.Uri);
      serviceAppProxy.Update(true);

      // provision service app proxy
      serviceAppProxy.Provision();

      // start it if it isn't already started
      if (serviceAppProxy.Status != SPObjectStatus.Online)
        serviceAppProxy.Status = SPObjectStatus.Online;
      serviceAppProxy.Update(true);

      // add the proxy to the default group if selected
      if (DefaultServiceApp.Checked)
      {
        SPServiceApplicationProxyGroup defaultGroup = SPServiceApplicationProxyGroup.Default;
        defaultGroup.Add(serviceAppProxy);
        defaultGroup.Update(true);
      }
    }

    private static void StartServiceInstances()
    {
      // loop through all service instances on the current server and see if the one for
      //      this service app is running/not
      foreach (SPServiceInstance serviceInstance in SPServer.Local.ServiceInstances)
      {
        BaristaServiceInstance baristaServiceInstance = serviceInstance as BaristaServiceInstance;

        // if this one isn't running, start it up
        if ((baristaServiceInstance != null) &&
                (baristaServiceInstance.Status != SPObjectStatus.Online))
          baristaServiceInstance.Status = SPObjectStatus.Online;
      }
    }

    private Guid ApplicationId
    {
      get
      {
        if (Request.QueryString != null)
        {
          string s = base.Request.QueryString["appid"];
          if (string.IsNullOrEmpty(s))
            return Guid.Empty;

          try
          {
            return new Guid(s);
          }
          catch (FormatException)
          {
            throw new Exception();
          }
        }
        return Guid.Empty;
      }
    }
  }
}
