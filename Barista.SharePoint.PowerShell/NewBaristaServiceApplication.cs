namespace Barista.SharePoint
{
    using System;
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

            // ensure can hit service application
            var service = BaristaHelper.GetBaristaService(farm);
            if (service == null)
            {
                ThrowTerminatingError(new InvalidOperationException("Barista Service not found (likely not installed)."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
                return;
            }

            // ensure can hit app pool
            var appPool = ApplicationPool.Read();
            if (appPool == null)
            {
                ThrowTerminatingError(new InvalidOperationException("Application pool not found."), ErrorCategory.ResourceUnavailable, this);
                SkipProcessCurrentRecord();
            }
            #endregion

            // verify a service app doesn't already exist
            var existingServiceApp = service.Applications.GetValue<BaristaServiceApplication>();
            if (existingServiceApp != null)
            {
                WriteError(new InvalidOperationException("Barista Service Application already exists."),
                    ErrorCategory.ResourceExists,
                    existingServiceApp);
                SkipProcessCurrentRecord();
            }

            // create & provision the service app
            if (!ShouldProcess(Name))
                return;

            var serviceApp = BaristaServiceApplication.Create(
              Name,
              service,
              appPool);

            // provision the service app
            serviceApp.Provision();

            //Check for and copy farm default property bag settings to the service app.
            if (farm.Properties.ContainsKey("BaristaTrustedLocations"))
            {
                serviceApp.Properties.Add("BaristaTrustedLocations", farm.Properties["BaristaTrustedLocations"]);
                serviceApp.Update();
            }

            if (farm.Properties.ContainsKey("BaristaSearchIndexDefinitions"))
            {
                serviceApp.Properties.Add("BaristaSearchIndexDefinitions", farm.Properties["BaristaSearchIndexDefinitions"]);
                serviceApp.Update();
            }

            // pass service app back to the PowerShell
            WriteObject(serviceApp);
        }
    }
}