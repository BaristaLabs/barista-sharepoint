namespace Barista.SharePoint
{
  using Barista.SharePoint.ServiceManagement;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.PowerShell;
  using System;
  using System.Management.Automation;

  [Cmdlet(VerbsCommon.New, "BaristaWebSocketsService", SupportsShouldProcess = true)]
  public class NewBaristaWebSocketsService : SPCmdlet
  {
    #region cmdlet parameters
    [Parameter(Mandatory = true)]
    [ValidateNotNullOrEmpty]
    public SPManagedAccountPipeBind ManagedAccount;
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

      // ensure can hit local server
      var server = SPServer.Local;
      if (server == null)
      {
        ThrowTerminatingError(new InvalidOperationException("SharePoint local server not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
        return;
      }
      #endregion

      //Verify that the web sockets service doesn't already exist
      var webSocketsService =
                SPFarm.Local.Services.GetValue<BaristaWebSocketsService>(
                BaristaWebSocketsService.NtServiceName);

      if (webSocketsService != null)
      {
        WriteError(new InvalidOperationException("Barista Web Sockets Service already exists."),
            ErrorCategory.ResourceExists,
            webSocketsService);
        WriteObject(webSocketsService);
        return;
      }

      webSocketsService = new BaristaWebSocketsService(SPFarm.Local, ManagedAccount.Read());
      webSocketsService.Update();

      // provision the service
      webSocketsService.Provision();

      // pass service back to PowerShell
      WriteObject(webSocketsService);
    }
  }
}