namespace Barista.SharePoint
{
    using Barista.DocumentStore;
    using Barista.SharePoint.Services;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.PowerShell;
    using System;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    [Cmdlet("Deploy", "BaristaPackage", SupportsShouldProcess = true)]
    public class DeployBaristaPackage : SPCmdlet
    {
        #region cmdlet parameters
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public SPServiceContextPipeBind ServiceContext;

        [Parameter(ParameterSetName = "PackagePath", Mandatory = true)]
        public string PackagePath
        {
            get;
            set;
        }
        #endregion

        protected override bool RequireUserFarmAdmin()
        {
            return true;
        }

        protected override void InternalProcessRecord()
        {
            // get the specified service context
            var serviceContext = ServiceContext.Read();
            if (serviceContext == null)
            {
                WriteError(new InvalidOperationException("Invalid service context."), ErrorCategory.ResourceExists, null);
                return;
            }
            if (!File.Exists(this.PackagePath))
            {
                WriteError(new InvalidOperationException("Unable to locate the package at " + this.PackagePath), ErrorCategory.InvalidArgument, null);
                return;
            }

            var baristaProxies =
                serviceContext.GetProxies(typeof(BaristaServiceApplicationProxy))
                    .OfType<BaristaServiceApplicationProxy>();

            var result = new StringBuilder();
            using (var fs = File.Open(this.PackagePath, FileMode.Open, FileAccess.Read))
            {
                var packageData = fs.ReadToEnd();
                foreach (var proxy in baristaProxies)
                {
                    using (new SPServiceContextScope(serviceContext))
                    {
                        var packageResult = proxy.AddPackage(packageData);
                        result.Append(packageResult);
                        result.AppendLine();
                    }
                }
            }

            WriteObject(result.ToString());
        }
    }
}