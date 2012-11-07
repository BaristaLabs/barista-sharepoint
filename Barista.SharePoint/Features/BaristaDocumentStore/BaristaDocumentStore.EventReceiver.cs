namespace Barista.SharePoint.Features.BaristaDocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.WebPartPages;
  using Barista.SharePoint.DocumentStore;

  /// <summary>
  /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
  /// </summary>
  /// <remarks>
  /// The GUID attached to this class may be used during packaging and should not be modified.
  /// </remarks>
  [Guid("6ca8eee8-ab24-48e0-9f39-bba9dea18621")]
  public class BaristaDocumentStoreEventReceiver : SPFeatureReceiver
  {
    public override void FeatureActivated(SPFeatureReceiverProperties properties)
    {
      base.FeatureActivated(properties);
    }

    public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
    {
      //Remove all _cts/Document Store Entity folders...
      var spSite = properties.Feature.Parent as SPSite;
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
