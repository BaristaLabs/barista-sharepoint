namespace Barista.SharePoint
{
  using Barista.SharePoint.ServiceManagement;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.PowerShell;
  using System;
  using System.Management.Automation;

  [Cmdlet(VerbsCommon.New, "BaristaWebSocketsServiceInstance", SupportsShouldProcess = true)]
  public class NewBaristaWebSocketsServiceInstance : SPCmdlet
  {
    #region cmdlet parameters

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

      //
      //  Check if the local server already has a BaristaWebSocketsServiceInstance.
      //
      var webSocketsServiceInstance =
          SPServer.Local.ServiceInstances.GetValue<BaristaWebSocketsServiceInstance>(
          BaristaWebSocketsServiceInstance.ServiceInstanceName);

      if (webSocketsServiceInstance != null)
      {
        WriteError(new InvalidOperationException("Barista Web Sockets Service Instance already exists."),
            ErrorCategory.ResourceExists,
            webSocketsServiceInstance);

        WriteObject(webSocketsServiceInstance);
        return;
      }

      //
      //  Next, check to see if the farm knows about the Web Sockets Service.
      //
      var baristaWebSocketsService =
          SPFarm.Local.Services.GetValue<BaristaWebSocketsService>(
          BaristaWebSocketsService.NtServiceName);
      if (baristaWebSocketsService == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Web Sockets Service not found (likely not installed)."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
        return;
      }

      //
      //  Now create the baristaWebSocketsServiceInstance on the local server.
      //
      var baristaWebSocketsServiceInstance = new BaristaWebSocketsServiceInstance(baristaWebSocketsService);
      baristaWebSocketsServiceInstance.Provision(true);
      baristaWebSocketsServiceInstance.Update();
      
      WriteObject(baristaWebSocketsServiceInstance);
    }
  }
}