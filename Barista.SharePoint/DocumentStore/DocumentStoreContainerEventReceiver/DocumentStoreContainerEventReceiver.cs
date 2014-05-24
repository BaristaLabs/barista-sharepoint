namespace Barista.SharePoint.DocumentStore.DocumentStoreContainerEventReceiver
{
  using Microsoft.SharePoint;
  using System;
  using System.Linq;

  /// <summary>
  /// List Events
  /// </summary>
  public class DocumentStoreContainerEventReceiver : SPListEventReceiver
  {
    /// <summary>
    /// A list has been added.
    /// </summary>
    public override void ListAdded(SPListEventProperties properties)
    {
      base.ListAdding(properties);

      if (properties.FeatureId != new Guid("1e084611-a8c5-449c-a1f0-841a56ee2712") || properties.TemplateId != 10001)
        return;

      //Make the entity part content types not visible on the new button.
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      var attachmentContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityAttachmentContentTypeId);

      var contentTypes = properties.List.RootFolder.ContentTypeOrder.ToList();

      foreach (var ct in contentTypes.ToList())
      {
        var shouldHide = false;
        if (ct.Id.IsChildOf(documentStoreEntityPartContentTypeId))
          shouldHide = true;
        else if (ct.Id.IsChildOf(attachmentContentTypeId))
          shouldHide = true;
        else if (ct.Id.IsChildOf(SPBuiltInContentTypeId.LinkToDocument))
          shouldHide = true;

        if (!shouldHide)
          continue;

        contentTypes.Remove(ct);
      }
      properties.List.RootFolder.UniqueContentTypeOrder = contentTypes;
      properties.List.RootFolder.Update();
      
    }
  }
}