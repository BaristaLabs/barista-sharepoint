namespace Barista.SharePoint.WorkflowActivities.Features.BaristaWorkflowActivities
{
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("0cec4877-b55d-4c1c-aa4a-2d95b52dc4ae")]
    public class BaristaWorkflowActivitiesEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            //Ensure that the Barista farm feature has been activated.
            var isBaristaServiceFeatureActivated =
              SPWebService.ContentService.Features[new Guid("90fb8db4-2b5f-4de7-882b-6faba092942c")] != null;

            if (isBaristaServiceFeatureActivated == false)
                throw new SPException(
                  "The Farm-Level Barista Service Feature (90fb8db4-2b5f-4de7-882b-6faba092942c) must first be activated.");

            //Register the web.config modification for the workflow activity
            try
            {
                var contentService = SPWebService.ContentService;
                var assemblyValue = typeof(BaristaEvalAction).Assembly.FullName;
                var namespaceValue = typeof(BaristaEvalAction).Namespace;
                var modification = CreateBaristaWorkflowActivityWebConfigModification(assemblyValue, namespaceValue);
                contentService.WebConfigModifications.Add(modification);

                // Serialize the Web application state and propagate changes across the farm. 
                contentService.Update();

                // Save Web.config changes.
                contentService.ApplyWebConfigModifications();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            //Unregister the web.config modification for the workflow activity
            try
            {
                var contentService = SPWebService.ContentService;
                var assemblyValue = typeof(BaristaEvalAction).Assembly.FullName;
                var namespaceValue = typeof(BaristaEvalAction).Namespace;
                var modification = CreateBaristaWorkflowActivityWebConfigModification(assemblyValue, namespaceValue);
                contentService.WebConfigModifications.Remove(modification);

                // Serialize the Web application state and propagate changes across the farm. 
                contentService.Update();

                // Save Web.config changes.
                contentService.ApplyWebConfigModifications();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private static SPWebConfigModification CreateBaristaWorkflowActivityWebConfigModification(string assembly, string assemblyNamespace)
        {
            return new SPWebConfigModification
            {
                Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode,
                Name = String.Format("authorizedType[@Assembly='{0}'][@Namespace='{1}'][@TypeName='*'][@Authorized='True']", assembly, assemblyNamespace),
                Path = "configuration/System.Workflow.ComponentModel.WorkflowCompiler/authorizedTypes",
                Owner = assemblyNamespace,
                Sequence = 0U,
                Value = String.Format("<authorizedType Assembly='{0}' Namespace='{1}' TypeName='*' Authorized='True' />", assembly, assemblyNamespace)
            };
        }
    }
}
