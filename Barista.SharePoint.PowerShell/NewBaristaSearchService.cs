namespace Barista.SharePoint
{
  using Barista.SharePoint.ServiceManagement;
  using Microsoft.SharePoint.Administration;
  using Microsoft.SharePoint.PowerShell;
  using System;
  using System.Management.Automation;

  [Cmdlet(VerbsCommon.New, "BaristaSearchService", SupportsShouldProcess = true)]
  public class NewBaristaSearchService : SPCmdlet
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

      //Verify that the search service doesn't already exist
      var searchService =
                SPFarm.Local.Services.GetValue<BaristaSearchService>(
                BaristaSearchService.NtServiceName);

      if (searchService != null)
      {
        WriteError(new InvalidOperationException("Barista Search Service already exists."),
            ErrorCategory.ResourceExists,
            searchService);
        WriteObject(searchService);
        return;
      }

      searchService = new BaristaSearchService(SPFarm.Local, ManagedAccount.Read());
      searchService.Update();

      // provision the service
      searchService.Provision();

      //Check for and copy a property bag setting
      if (farm.Properties.ContainsKey("BaristaSearchIndexDefinitions"))
      {
        searchService.Properties.Add("BaristaSearchIndexDefinitions", farm.Properties["BaristaSearchIndexDefinitions"]);
        searchService.Update();
      }

      // pass service back to PowerShell
      WriteObject(searchService);
    }
  }
}