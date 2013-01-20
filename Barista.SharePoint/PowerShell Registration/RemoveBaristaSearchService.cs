namespace Barista.SharePoint
{
  using Barista.SharePoint.ServiceManagement;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.PowerShell;
  using System;
  using System.Management.Automation;

  [Cmdlet(VerbsCommon.Remove, "BaristaSearchService", SupportsShouldProcess = true)]
  public class RemoveBaristaSearchService : SPCmdlet
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
      //  Get the farm's Barista Search Service.
      //
      var baristaSearchService =
          SPFarm.Local.Services.GetValue<BaristaSearchService>(
          BaristaSearchService.NtServiceName);

      if (baristaSearchService == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Search Service not found."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
        return;
      }

      //
      //  Remove all existing service instances.
      //
      foreach (var serviceInstance in baristaSearchService.Instances)
      {
        serviceInstance.Unprovision();
        serviceInstance.Delete();
      }

      baristaSearchService.Unprovision();
      baristaSearchService.Delete();
    }
  }
}