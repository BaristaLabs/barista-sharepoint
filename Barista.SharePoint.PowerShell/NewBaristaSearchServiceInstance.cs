namespace Barista.SharePoint
{
  using Barista.SharePoint.ServiceManagement;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.PowerShell;
  using System;
  using System.Management.Automation;

  [Cmdlet(VerbsCommon.New, "BaristaSearchServiceInstance", SupportsShouldProcess = true)]
  public class NewBaristaSearchServiceInstance : SPCmdlet
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
      //  Check if the local server already has a BaristaSearchServiceInstance.
      //
      var searchServiceInstance =
          SPServer.Local.ServiceInstances.GetValue<BaristaSearchServiceInstance>(
          BaristaSearchServiceInstance.ServiceInstanceName);

      if (searchServiceInstance != null)
      {
        WriteError(new InvalidOperationException("Barista Search Service Instance already exists."),
            ErrorCategory.ResourceExists,
            searchServiceInstance);

        WriteObject(searchServiceInstance);
        return;
      }

      //
      //  Next, check to see if the farm knows about the Web Sockets Service.
      //
      var baristaSearchService =
          SPFarm.Local.Services.GetValue<BaristaSearchService>(
          BaristaSearchService.NtServiceName);
      if (baristaSearchService == null)
      {
        ThrowTerminatingError(new InvalidOperationException("Barista Search Service not found (likely not installed)."), ErrorCategory.ResourceUnavailable, this);
        SkipProcessCurrentRecord();
        return;
      }

      //
      //  Now create the baristaSearchServiceInstance on the local server.
      //
      var baristaSearchServiceInstance = new BaristaSearchServiceInstance(baristaSearchService);
      baristaSearchServiceInstance.Provision(true);
      baristaSearchServiceInstance.Update();
      
      WriteObject(baristaSearchServiceInstance);
    }
  }
}