namespace Barista.SharePoint.Features.BaristaDocumentStore
{
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using System;
  using System.Linq;
  using System.Runtime.InteropServices;
  using global::Barista.SharePoint.DocumentStore;

  /// <summary>
  /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
  /// </summary>
  /// <remarks>
  /// The GUID attached to this class may be used during packaging and should not be modified.
  /// </remarks>
  [Guid("6ca8eee8-ab24-48e0-9f39-bba9dea18621")]
  public partial class BaristaDocumentStoreFeatureReceiver : SPFeatureReceiver
  {
    public override void FeatureActivated(SPFeatureReceiverProperties properties)
    {
      //Ensure that the Barista farm feature has been activated.
      var isBaristaServiceFeatureActivated =
        SPWebService.ContentService.Features[new Guid("90fb8db4-2b5f-4de7-882b-6faba092942c")] != null;

      if (isBaristaServiceFeatureActivated == false)
        throw new SPException(
          "The Farm-Level Barista Service Feature (90fb8db4-2b5f-4de7-882b-6faba092942c) must first be activated.");

      //Ensure that the Document Set feature has been activated.
// ReSharper disable PossibleNullReferenceException
      var isDocumentSetFeatureActivated =
       (properties.Feature.Parent as SPSite).Features[new Guid("3bae86a2-776d-499d-9db8-fa4cdc7884f8")] != null;
// ReSharper restore PossibleNullReferenceException

      if (isDocumentSetFeatureActivated == false)
        throw new SPException(
          "The Document Set Feature (3bae86a2-776d-499d-9db8-fa4cdc7884f8) must first be activated.");

      base.FeatureActivated(properties);
    }

    public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
    {
      //Remove all _cts/Document Store Entity folders...
      var spSite = properties.Feature.Parent as SPSite;
      if (spSite == null)
        return;

      var ctsFolder = spSite.RootWeb.RootFolder.SubFolders["_cts"];
      PermissionsHelper.ToggleEventFiring(false);

      try
      {
        var dsEntityFolders = ctsFolder.SubFolders.OfType<SPFolder>().Where(f => f.Name.StartsWith("Document Store Entity")).ToList();
        foreach (var folder in dsEntityFolders)
        {
          ctsFolder.SubFolders.Delete(folder.Url);
        }

        var dsEntityPartFolders = ctsFolder.SubFolders.OfType<SPFolder>().Where(f => f.Name.StartsWith("Document Store Entity Part")).ToList();
        foreach (var folder in dsEntityPartFolders)
        {
          ctsFolder.SubFolders.Delete(folder.Url);
        }

        var dsEntityAttachmentFolders = ctsFolder.SubFolders.OfType<SPFolder>().Where(f => f.Name.StartsWith("Document Store Attachment")).ToList();
        foreach (var folder in dsEntityAttachmentFolders)
        {
          ctsFolder.SubFolders.Delete(folder.Url);
        }
      }
      finally
      {
        PermissionsHelper.ToggleEventFiring(true);
      }

      base.FeatureDeactivating(properties);
    }
  }
}
