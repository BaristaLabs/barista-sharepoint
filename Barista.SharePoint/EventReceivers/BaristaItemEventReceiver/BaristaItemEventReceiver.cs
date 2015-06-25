namespace Barista.SharePoint.EventReceivers.BaristaItemEventReceiver
{
  using System;
  using Barista.SharePoint.Extensions;
  using Barista.SharePoint.Services;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;

  /// <summary>
  /// List Item Events
  /// </summary>
  public class BaristaItemEventReceiver : SPItemEventReceiver
  {
    ////// <summary>
    ////// An item is being added.
    ////// </summary>
    //public override void ItemAdding(SPItemEventProperties properties)
    //{
    //  base.ItemAdding(properties);
    //}

    ////// <summary>
    ////// An item is being updated.
    ////// </summary>
    //public override void ItemUpdating(SPItemEventProperties properties)
    //{
    //  base.ItemUpdating(properties);
    //}

    ////// <summary>
    ////// An item is being deleted.
    ////// </summary>
    //public override void ItemDeleting(SPItemEventProperties properties)
    //{
    //  base.ItemDeleting(properties);
    //}

    ///// <summary>
    ///// An item is being checked in.
    ///// </summary>
    //public override void ItemCheckingIn(SPItemEventProperties properties)
    //{
    //  base.ItemCheckingIn(properties);
    //}

    ///// <summary>
    ///// An item is being checked out.
    ///// </summary>
    //public override void ItemCheckingOut(SPItemEventProperties properties)
    //{
    //  base.ItemCheckingOut(properties);
    //}

    ///// <summary>
    ///// An item is being unchecked out.
    ///// </summary>
    //public override void ItemUncheckingOut(SPItemEventProperties properties)
    //{
    //  base.ItemUncheckingOut(properties);
    //}

    ///// <summary>
    ///// An attachment is being added to the item.
    ///// </summary>
    //public override void ItemAttachmentAdding(SPItemEventProperties properties)
    //{
    //  base.ItemAttachmentAdding(properties);
    //}

    ///// <summary>
    ///// An attachment is being removed from the item.
    ///// </summary>
    //public override void ItemAttachmentDeleting(SPItemEventProperties properties)
    //{
    //  base.ItemAttachmentDeleting(properties);
    //}

    ///// <summary>
    ///// A file is being moved.
    ///// </summary>
    //public override void ItemFileMoving(SPItemEventProperties properties)
    //{
    //  base.ItemFileMoving(properties);
    //}

    /// <summary>
    /// An item was added.
    /// </summary>
    public override void ItemAdded(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemAdded(properties);
    }

    /// <summary>
    /// An item was updated.
    /// </summary>
    public override void ItemUpdated(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemUpdated(properties);
    }

    /// <summary>
    /// An item was deleted.
    /// </summary>
    public override void ItemDeleted(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemDeleted(properties);
    }

    /// <summary>
    /// An item was checked in.
    /// </summary>
    public override void ItemCheckedIn(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemCheckedIn(properties);
    }

    /// <summary>
    /// An item was checked out.
    /// </summary>
    public override void ItemCheckedOut(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemCheckedOut(properties);
    }

    /// <summary>
    /// An item was unchecked out.
    /// </summary>
    public override void ItemUncheckedOut(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemUncheckedOut(properties);
    }

    /// <summary>
    /// An attachment was added to the item.
    /// </summary>
    public override void ItemAttachmentAdded(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemAttachmentAdded(properties);
    }

    /// <summary>
    /// An attachment was removed from the item.
    /// </summary>
    public override void ItemAttachmentDeleted(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemAttachmentDeleted(properties);
    }

    /// <summary>
    /// A file was moved.
    /// </summary>
    public override void ItemFileMoved(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemFileMoved(properties);
    }

    /// <summary>
    /// A file was converted.
    /// </summary>
    public override void ItemFileConverted(SPItemEventProperties properties)
    {
      ExecuteBaristaScript(properties);
      base.ItemFileConverted(properties);
    }

    ////// <summary>
    ////// The list received a context event.
    ////// </summary>
    //public override void ContextEvent(SPItemEventProperties properties)
    //{
    //  base.ContextEvent(properties);
    //}

    /// <summary>
    /// Executes the Barista Script in response to an item event.
    /// </summary>
    /// <param name="properties">The properties.</param>
    private static void ExecuteBaristaScript(SPItemEventProperties properties)
    {
      BrewRequest request;
      using (var web = properties.OpenWeb())
      {
        var list = web.Lists[properties.ListId];
        var item = list.GetItemById(properties.ListItemId);

        request = new BrewRequest
          {
            ContentType = "application/json", //default to application/json.
            Code = properties.ReceiverData,
            ScriptEngineFactory = "Barista.SharePoint.SPBaristaJurassicScriptEngineFactory, Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52"
          };

        request.SetExtendedPropertiesFromSPItemEventProperties(web, list, item, properties);
      }

      var serviceContext = SPServiceContext.Current ??
                           SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default,
                                                       new SPSiteSubscriptionIdentifier(Guid.Empty));

      var client = new BaristaServiceClient(serviceContext);

      client.Exec(request);
      //TODO: Allow for Syncronous events that expect to return an object with which to update the properties object with.
    }
  }
}