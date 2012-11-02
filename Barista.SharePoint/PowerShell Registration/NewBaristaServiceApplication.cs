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

  [Cmdlet(VerbsCommon.New, "BaristaServiceApplication", SupportsShouldProcess = true)]
  public class NewBaristaServiceApplication : SPCmdlet
  {
    #region cmdlet parameters
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public string Name;

    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public SPIisWebServiceApplicationPoolPipeBind ApplicationPool;
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

      // ensure can hit local server
      SPServer server = SPServer.Local;
      if (server == null)
      {
        ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
      }

      // ensure can hit service application
      BaristaService service = farm.Services.GetValue<BaristaService>();
      if (service == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Service not found (likely not installed)."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
      }

      // ensure can hit app pool
      SPIisWebServiceApplicationPool appPool = this.ApplicationPool.Read();
      if (appPool == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Application pool not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
      }
      #endregion

      // verify a service app doesn't already exist
      BaristaServiceApplication existingServiceApp = service.Applications.GetValue<BaristaServiceApplication>();
      if (existingServiceApp != null)
      {
        WriteError(new InvalidOperationException("Barista Service Application already exists."),
            ErrorCategory.ResourceExists,
            existingServiceApp);
        SkipProcessCurrentRecord();
      }

      // create & provision the service app
      if (ShouldProcess(this.Name))
      {
        BaristaServiceApplication serviceApp = BaristaServiceApplication.Create(
            this.Name,
            service,
            appPool);

        // provision the service app
        serviceApp.Provision();

        // pass service app back to the PowerShell
        WriteObject(serviceApp);
      }
    }
  }
}