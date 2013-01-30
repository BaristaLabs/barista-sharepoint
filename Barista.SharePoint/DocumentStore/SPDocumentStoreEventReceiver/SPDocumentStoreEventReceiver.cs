namespace Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver
{
  using Barista.DocumentStore;
  using Microsoft.Office.DocumentManagement.DocumentSets;
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

      string guidString;

      //If the Url ends with "default.dsep", presume it's an entity deletion.
      if (properties.BeforeUrl.EndsWith(Constants.DocumentStoreDefaultEntityPartFileName, StringComparison.InvariantCultureIgnoreCase))
      {
        guidString =
          properties.BeforeUrl
                    .Replace("/" + Constants.DocumentStoreDefaultEntityPartFileName, "")
                    .Substring(properties.BeforeUrl.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

        try
        {
          var entityId = new Guid(guidString);
          using (var site = properties.OpenSite())
          {
            using (var web = properties.OpenWeb())
            {
              var context = new BaristaContext(site, web);
              EntityDeleted(context, entityId);
              return;
            }
          }
        }
        catch (Exception) { /* Do Nothing */ }
      }

      //If the above failed, and the Url ends with ".dsep", presume it's an entity part deletion.
      if (properties.BeforeUrl.EndsWith(Constants.DocumentSetEntityPartExtension, StringComparison.InvariantCultureIgnoreCase))
      {
        var partName =
          properties.BeforeUrl
                    .Substring(properties.BeforeUrl.LastIndexOf("/", System.StringComparison.InvariantCulture) + 1)
                    .Replace(Constants.DocumentSetEntityPartExtension, "");

        var urlWithoutPartName =
          properties.BeforeUrl
                    .Replace("/" + partName + Constants.DocumentSetEntityPartExtension, "");

        guidString =
          urlWithoutPartName.Substring(urlWithoutPartName.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

        try
        {
          var entityId = new Guid(guidString);

          using (var site = properties.OpenSite())
          {
            using (var web = properties.OpenWeb())
            {
              var context = new BaristaContext(site, web);
              EntityPartDeleted(context, entityId, partName);
              return;
            }
          }
        }
        catch (Exception) { /* Do Nothing */ }
      }

      //If the last fragment is a Guid, presume it's an entity deletion.
      guidString =
        properties.BeforeUrl
                  .Substring(properties.BeforeUrl.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

      try
      {
        var entityId = new Guid(guidString);

        using (var site = properties.OpenSite())
        {
          using (var web = properties.OpenWeb())
          {
            var context = new BaristaContext(site, web);
            EntityDeleted(context, entityId);
            return;
          }
        }
      }
      catch (Exception) { /* Do Nothing */ }

      //If it wasn't a guid, presume an attachment.
      var fileName =
        properties.BeforeUrl
                  .Substring(properties.BeforeUrl.LastIndexOf("/", System.StringComparison.InvariantCulture) + 1);

      var urlWithoutFileName =
        properties.BeforeUrl
                  .Replace("/" + fileName, "");

      guidString =
        urlWithoutFileName.Substring(urlWithoutFileName.LastIndexOf("/", StringComparison.InvariantCulture) + 1);

      try
      {
        var attachmentEntityId = new Guid(guidString);
        using (var site = properties.OpenSite())
        {
          using (var web = properties.OpenWeb())
          {
            var context = new BaristaContext(site, web);
            AttachmentDeleted(context, attachmentEntityId, fileName);
// ReSharper disable RedundantJumpStatement
            return;
// ReSharper restore RedundantJumpStatement
          }
        }
      }
      catch (Exception) { /* Do Nothing */ }

      //At this point, I don't know what it is (Folder or an additional file not in an document set), continue...
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

    /// <summary>
    /// Processes events.
    /// </summary>
    /// <param name="properties"></param>
    protected virtual void ProcessEvent(SPItemEventProperties properties)
    {
      //TODO: Should there be folder events too? What happens to the files in a folder when a folder is deleted?

      var documentStoreEntityContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityContentTypeId);
      var documentStoreEntityPartContentTypeId = new SPContentTypeId(Constants.DocumentStoreEntityPartContentTypeId);
      var attachmentContentTypeId = new SPContentTypeId(Constants.AttachmentDocumentContentTypeId);

      if (properties.ListItem.ContentTypeId.IsChildOf(documentStoreEntityContentTypeId))
      {
        //Note: This is the actual Document Set.
        switch (properties.EventType)
        {
          case SPEventReceiverType.ItemFileMoved:
            {
              var documentSet = DocumentSet.GetDocumentSet(properties.ListItem.Folder);
              var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, null);

              using (var site = properties.OpenSite())
              {
                using (var web = properties.OpenWeb())
                {
                  var before = web.GetFolder(properties.BeforeUrl);
                  var after = web.GetFolder(properties.AfterUrl);
                  var oldFolder = SPDocumentStoreHelper.MapFolderFromSPFolder(before.ParentFolder);
                  var newFolder = SPDocumentStoreHelper.MapFolderFromSPFolder(after.ParentFolder);
                  var context = new BaristaContext(site, web);
                  EntityMoved(context, entity, oldFolder, newFolder);
                }
                
              }
              break;
          }
        }
      }
      else if (properties.ListItem.ContentTypeId.IsChildOf(documentStoreEntityPartContentTypeId))
      {
        if (
          String.Compare(properties.ListItem.File.Name, Constants.DocumentStoreDefaultEntityPartFileName,
                         StringComparison.InvariantCulture) == 0)
        {
          switch (properties.EventType)
          {
            case SPEventReceiverType.ItemAdded:
              {
                var documentSet = DocumentSet.GetDocumentSet(properties.ListItem.Folder);
                var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, properties.ListItem.File,
                                                                           null);
                using (var site = properties.OpenSite())
                {
                  using (var web = properties.OpenWeb())
                  {
                    var context = new BaristaContext(site, web);
                    EntityAdded(context, entity);
                  }
                }
              }
              break;
            case SPEventReceiverType.ItemUpdated:
              {
                var documentSet = DocumentSet.GetDocumentSet(properties.ListItem.Folder);
                var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, properties.ListItem.File,
                                                                           null);
                using (var site = properties.OpenSite())
                {
                  using (var web = properties.OpenWeb())
                  {
                    var context = new BaristaContext(site, web);
                    EntityUpdated(context, entity);
                  }
                }
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
              using (var site = properties.OpenSite())
              {
                using (var web = properties.OpenWeb())
                {
                  var context = new BaristaContext(site, web);
                  EntityPartAdded(context, entityPart);
                }
              }
              break;
            case SPEventReceiverType.ItemUpdated:
              using (var site = properties.OpenSite())
              {
                using (var web = properties.OpenWeb())
                {
                  var context = new BaristaContext(site, web);
                  EntityPartUpdated(context, entityPart);
                }
              }
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
            using (var site = properties.OpenSite())
            {
              using (var web = properties.OpenWeb())
              {
                var context = new BaristaContext(site, web);
                AttachmentAdded(context, attachment);
              }
            }
            break;
          case SPEventReceiverType.ItemUpdated:
            using (var site = properties.OpenSite())
            {
              using (var web = properties.OpenWeb())
              {
                var context = new BaristaContext(site, web);
                AttachmentUpdated(context, attachment);
              }
            }
            break;
        }
      }
      else if (properties.ListItem.File != null &&
               properties.ListItem.ContentTypeId.IsChildOf(SPBuiltInContentTypeId.Document) &&
               String.Compare(properties.ListItem.File.Name, Constants.DocumentStoreDefaultEntityPartFileName,
                              StringComparison.InvariantCultureIgnoreCase) == 0)
      {
        //Apparently the Default documents in a Doc Set are initially added as a "Document"
        var documentSet = DocumentSet.GetDocumentSet(properties.ListItem.File.ParentFolder);
        var entity = SPDocumentStoreHelper.MapEntityFromDocumentSet(documentSet, properties.ListItem.File, null);
        switch (properties.EventType)
        {
          case SPEventReceiverType.ItemAdded:
            using (var site = properties.OpenSite())
            {
              using (var web = properties.OpenWeb())
              {
                var context = new BaristaContext(site, web);
                EntityAdded(context, entity);
              }
            }
            break;
        }
      }
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Occurs when an entity is created.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    protected virtual void EntityAdded(BaristaContext context, Entity entity)
    {
    }

    /// <summary>
    /// Occurs when an entity part is added to an entity.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityPart"></param>
    protected virtual void EntityPartAdded(BaristaContext context, EntityPart entityPart)
    {
    }

    /// <summary>
    /// Occurs when an attachment is added to an entity.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="attachment"></param>
    protected virtual void AttachmentAdded(BaristaContext context, Attachment attachment)
    {
    }

    /// <summary>
    /// Occurs when an entity is updated.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    protected virtual void EntityUpdated(BaristaContext context, Entity entity)
    {
    }

    /// <summary>
    /// Occurs when an entity part is updated.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityPart"></param>
    protected virtual void EntityPartUpdated(BaristaContext context, EntityPart entityPart)
    {
    }

    /// <summary>
    /// Occurs when an attachment is updated.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="attachment"></param>
    protected virtual void AttachmentUpdated(BaristaContext context, Attachment attachment)
    {
    }

    /// <summary>
    /// Occurs when an entity is deleted from a SPDocumentStore.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityId"></param>
    protected virtual void EntityDeleted(BaristaContext context, Guid entityId)
    {
    }

    /// <summary>
    /// Occurs when an entity part is deleted from an entity.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    protected virtual void EntityPartDeleted(BaristaContext context, Guid entityId, string partName)
    {
    }

    /// <summary>
    /// Occurs when an attachment is deleted from an entity.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityId"></param>
    /// <param name="fileName"></param>
    protected virtual void AttachmentDeleted(BaristaContext context, Guid entityId, string fileName)
    {
    }

    /// <summary>
    /// Occurs when an entity is moved to a different folder.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <param name="oldFolder"></param>
    /// <param name="newFolder"></param>
    protected virtual void EntityMoved(BaristaContext context, Entity entity, Folder oldFolder, Folder newFolder)
    {
    }

    #endregion

  }
}