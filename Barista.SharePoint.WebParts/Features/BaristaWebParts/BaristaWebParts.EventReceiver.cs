namespace Barista.SharePoint.WebParts.Features.BaristaWebParts
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

    [Guid("f2e230e2-a2d0-4d9f-b44d-d171d66b1af6")]
    public class BaristaWebPartsEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            //Ensure that the Barista farm feature has been activated.
            var isBaristaServiceFeatureActivated =
              SPWebService.ContentService.Features[new Guid("90fb8db4-2b5f-4de7-882b-6faba092942c")] != null;

            if (isBaristaServiceFeatureActivated == false)
                throw new SPException(
                  "The Farm-Level Barista Service Feature (90fb8db4-2b5f-4de7-882b-6faba092942c) must first be activated.");
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
