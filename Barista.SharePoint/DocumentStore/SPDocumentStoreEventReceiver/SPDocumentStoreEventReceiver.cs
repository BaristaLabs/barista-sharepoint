namespace Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver
{
  using Barista.DocumentStore;
  using Microsoft.SharePoint;
  using System;

  /// <summary>
  /// Raises events when Document Store Entity/EntityParts/Attachments are Added/Updated/Deleted.
  /// </summary>
  public class SPDocumentStoreEventReceiver : SPItemEventReceiver
  {

    #region SPItemEventReceiver Overrides

    /// <summary>
    /// An item was added.
    /// </summary>
    public override void ItemAdded(SPItemEventProperties properties)
    {
      base.ItemAdded(properties);
      ProcessEvent(properties);
    }

    /// <summary>
    /// An item was updated.
    /// </summary>
    public override void ItemUpdated(SPItemEventProperties properties)
    {
      base.ItemUpdated(properties);
      ProcessEvent(properties);
    }

    /// <summary>
    /// An item was deleted.
    /// </summary>
    public override void ItemDeleted(SPItemEventProperties properties)
    {
      base.ItemDeleted(properties);

      if (properties.BeforeUrl.EndsWith(Constants.DocumentSetEntityPartExtension) == false)
        return;

      if (properties.BeforeUrl.EndsWith(Constants.DocumentStoreDefaultEntityPartFileName))
      {
        var guidString =
          properties.BeforeUrl
                    .Replace("/" + Constants.DocumentStoreDefaultEntityPartFileName, "")
                    .Substring(properties.BeforeUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

        var entityId = new Guid(guidString);
        EntityDeleted(entityId);
      }
      else
      {
        var partName =
          properties.BeforeUrl
                    .Substring(properties.BeforeUrl.LastIndexOf("/", System.StringComparison.Ordinal) + 1)
                    .Replace(Constants.DocumentSetEntityPartExtension, "");

        var urlWithoutPartName =
          properties.BeforeUrl
                    .Replace("/" + partName + Constants.DocumentSetEntityPartExtension, "");

        var guidString =
          urlWithoutPartName.Substring(urlWithoutPartName.LastIndexOf("/", StringComparison.Ordinal) + 1);

        var entityId = new Guid(guidString);
        EntityPartDeleted(entityId, partName);
      }
    }

    /// <summary>
    /// An item was moved.
    /// </summary>
    /// <param name="properties"></param>
    public override void ItemFileMoved(SPItemEventProperties properties)
    {
      base.ItemFileMoved(properties);
      ProcessEvent(properties);
    }

    protected virtual void ProcessEvent(SPItemEventProperties properties)
    {
      //TODO: Should there be folder events too? What happens to the files in a folder when a folder is deleted?

      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      var attachmentContentTypeId = new SPContentTypeId(Constants.AttachmentDocumentContentTypeId);

      if (properties.ListItem.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        //Do Nothing. This is the actual Document Set.
      }
      else if (properties.ListItem.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
      {
        if (properties.ListItem.File.Name == Constants.DocumentStoreDefaultEntityPartFileName)
        {
          switch (properties.EventType)
          {
            case SPEventReceiverType.ItemAdded:
              {
                var entity = SPDocumentStoreHelper.MapEntityFromSPListItem(properties.ListItem, properties.ListItem.File,
                                                                           null);
                EntityAdded(entity);
              }
              break;
            case SPEventReceiverType.ItemUpdated:
              {
                var entity = SPDocumentStoreHelper.MapEntityFromSPListItem(properties.ListItem, properties.ListItem.File,
                                                                           null);
                EntityUpdated(entity);
              }
              break;
          }
        }
        else
        {
          var entityPart = SPDocumentStoreHelper.MapEntityPartFromSPFile(properties.ListItem.File, null);

          switch (properties.EventType)
          {
            case SPEventReceiverType.ItemAdded:
              EntityPartAdded(entityPart);
              break;
            case SPEventReceiverType.ItemUpdated:
              EntityPartUpdated(entityPart);
              break;
          }
        }
      }
      else if (properties.ListItem.ContentTypeId.IsChildOf(attachmentContentTypeId))
      {
        var attachment = SPDocumentStoreHelper.MapAttachmentFromSPFile(properties.ListItem.File);

        switch (properties.EventType)
        {
          case SPEventReceiverType.ItemAdded:
            AttachmentAdded(attachment);
            break;
          case SPEventReceiverType.ItemUpdated:
            AttachmentUpdated(attachment);
            break;
        }
      }
      else if (properties.ListItem.File != null &&
               properties.ListItem.ContentTypeId.IsChildOf(SPBuiltInContentTypeId.Document) &&
               properties.ListItem.File.Name == Constants.DocumentStoreDefaultEntityPartFileName)
      {
        //Apparently the Default documents in a Doc Set are initially added as a "Document"
        var entity = SPDocumentStoreHelper.MapEntityFromSPListItem(properties.ListItem, properties.ListItem.File, null);
        switch (properties.EventType)
        {
          case SPEventReceiverType.ItemAdded:
            EntityAdded(entity);
            break;
        }
      }
    }

    #endregion

    #region Virtual Methods

    protected virtual void EntityAdded(Entity entity)
    {
    }

    protected virtual void EntityPartAdded(EntityPart entityPart)
    {
    }

    protected virtual void AttachmentAdded(Attachment attachment)
    {
    }

    protected virtual void EntityUpdated(Entity entity)
    {
    }

    protected virtual void EntityPartUpdated(EntityPart entityPart)
    {
    }

    protected virtual void AttachmentUpdated(Attachment attachment)
    {
    }

    /// <summary>
    /// Occurs when an entity is deleted from a SPDocumentStore.
    /// </summary>
    /// <param name="entityId"></param>
    protected virtual void EntityDeleted(Guid entityId)
    {
    }

    protected virtual void EntityPartDeleted(Guid entityId, string partName)
    {
    }

    protected virtual void AttachmentDeleted(Guid entityId, string fileName)
    {
    }

    protected virtual void EntityMoved(Entity entity, Folder oldFolder, Folder newFolder)
    {
    }

    #endregion

  }
}