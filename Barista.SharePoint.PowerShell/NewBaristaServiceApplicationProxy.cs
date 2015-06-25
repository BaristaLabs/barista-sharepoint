namespace Barista.SharePoint
{
  using System;
  using Microsoft.SharePoint.PowerShell;
  using System.Management.Automation;
  using Microsoft.SharePoint.Administration;
  using Barista.SharePoint.Services;

  [Cmdlet(VerbsCommon.New, "BaristaServiceApplicationProxy", SupportsShouldProcess = true)]
  public class NewBaristaServiceApplicationProxy : SPCmdlet
  {
    private Uri m_uri;

    #region cmdlet parameters
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string Name;

    [Parameter(Mandatory = true, ParameterSetName = "Uri")]
    [ValidateNotNullOrEmpty]
    public string Uri
    {
      get { return m_uri.ToString(); }
      set { m_uri = new Uri(value); }
    }

    [Parameter(Mandatory = true, ParameterSetName = "ServiceApplication")]
    [ValidateNotNullOrEmpty]
    public SPServiceApplicationPipeBind ServiceApplication;
    #endregion

    protected override bool RequireUserFarmAdmin()
    {
      return true;
    }

    protected override void InternalProcessRecord()
    {
      #region validation stuff
      // ensure can hit farm
      var farm = SPFarm.Local;
      if (farm == null)
      {
        ThrowTerminatingError(new InvalidOperationException("SharePoint farm not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
        return;
      }

      // ensure proxy installed
      var serviceProxy = BaristaHelper.GetBaristaServiceProxy(farm);
      if (serviceProxy == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Service Proxy not found (likely not installed)."), ErrorCategory.NotInstalled, this);
        SkipProcessCurrentRecord();
        return;
      }

      // ensure can hit service application
      var existingServiceAppProxy = serviceProxy.ApplicationProxies.GetValue<BaristaServiceApplicationProxy>();
      if (existingServiceAppProxy != null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Service Application Proxy already exists."), ErrorCategory.ResourceExists, this);
        SkipProcessCurrentRecord();
        return;
      }
      #endregion

      Uri serviceApplicationAddress = null;
      switch (ParameterSetName)
      {
        case "Uri":
          serviceApplicationAddress = m_uri;
          break;
        case "ServiceApplication":
          {
            // make sure can get a refernce to service app
            var serviceApp = ServiceApplication.Read();
            if (serviceApp == null)
            {
              WriteError(new InvalidOperationException("Service application not found."), ErrorCategory.ResourceExists, null);
              SkipProcessCurrentRecord();
              return;
            }

            // make sure can connect to service app
            var sharedServiceApp = serviceApp as ISharedServiceApplication;
            if (sharedServiceApp == null)
            {
              WriteError(new InvalidOperationException("Service application not found."), ErrorCategory.ResourceExists, serviceApp);
              SkipProcessCurrentRecord();
              return;
            }

            serviceApplicationAddress = sharedServiceApp.Uri;
          }
          break;
        default:
          ThrowTerminatingError(new InvalidOperationException("Invalid parameter set."), ErrorCategory.InvalidArgument, this);
          break;
      }

      // create the service app proxy
      if ((serviceApplicationAddress == null) || !ShouldProcess(this.Name))
        return;

      var serviceAppProxy = new BaristaServiceApplicationProxy(
        this.Name,
        serviceProxy,
        serviceApplicationAddress);

      // provision the service app proxy
      serviceAppProxy.Provision();

      // pass service app proxy back to the PowerShell
      WriteObject(serviceAppProxy);
    }
  }
}