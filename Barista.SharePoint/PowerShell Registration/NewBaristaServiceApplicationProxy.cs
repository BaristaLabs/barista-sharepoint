namespace Barista.SharePoint
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Microsoft.SharePoint.PowerShell;
  using System.Management.Automation;
  using Microsoft.SharePoint.Administration;
  using Barista.SharePoint.Services;

  [Cmdlet(VerbsCommon.New, "BaristaServiceApplicationProxy", SupportsShouldProcess = true)]
  public class NewBaristaServiceApplicationProxy : SPCmdlet
  {
    private Uri _uri;

    #region cmdlet parameters
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string Name;

    [Parameter(Mandatory = true, ParameterSetName = "Uri")]
    [ValidateNotNullOrEmpty]
    public string Uri
    {
      get { return _uri.ToString(); }
      set { _uri = new Uri(value); }
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
      SPFarm farm = SPFarm.Local;
      if (farm == null)
      {
        ThrowTerminatingError(new InvalidOperationException("SharePoint farm not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
      }

      // ensure proxy installed
      BaristaServiceProxy serviceProxy = farm.ServiceProxies.GetValue<BaristaServiceProxy>();
      if (serviceProxy == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Service Proxy not found (likely not installed)."), ErrorCategory.NotInstalled, this);
        SkipProcessCurrentRecord();
      }

      // ensure can hit service application
      BaristaServiceApplicationProxy existingServiceAppProxy = serviceProxy.ApplicationProxies.GetValue<BaristaServiceApplicationProxy>();
      if (existingServiceAppProxy != null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Service Application Proxy already exists."), ErrorCategory.ResourceExists, this);
        SkipProcessCurrentRecord();
      }
      #endregion

      Uri serviceApplicationAddress = null;
      if (ParameterSetName == "Uri")
        serviceApplicationAddress = _uri;
      else if (ParameterSetName == "ServiceApplication")
      {
        // make sure can get a refernce to service app
        SPServiceApplication serviceApp = ServiceApplication.Read();
        if (serviceApp == null)
        {
          WriteError(new InvalidOperationException("Service application not found."), ErrorCategory.ResourceExists, serviceApp);
          SkipProcessCurrentRecord();
        }

        // make sure can connect to service app
        ISharedServiceApplication sharedServiceApp = serviceApp as ISharedServiceApplication;
        if (sharedServiceApp == null)
        {
          WriteError(new InvalidOperationException("Service application not found."), ErrorCategory.ResourceExists, serviceApp);
          SkipProcessCurrentRecord();
        }

        serviceApplicationAddress = sharedServiceApp.Uri;
      }
      else
        ThrowTerminatingError(new InvalidOperationException("Invalid parameter set."), ErrorCategory.InvalidArgument, this);

      // create the service app proxy
      if ((serviceApplicationAddress != null) && ShouldProcess(this.Name))
      {
        BaristaServiceApplicationProxy serviceAppProxy = new BaristaServiceApplicationProxy(
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
}