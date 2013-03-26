namespace Barista.SharePoint
{
  using Microsoft.SharePoint;
  using System;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public sealed class BaristaItemEventProperties
  {
    public string AfterUrl
    {
      get;
      set;
    }

    public string BeforeUrl
    {
      get;
      set;
    }

    public int CurrentUserId
    {
      get;
      set;
    }

    public SPEventReceiverType EventType
    {
      get;
      set;
    }

    public string FileSystemObjectTypePropertyName
    {
      get;
      set;
    }

    public Guid ListId
    {
      get;
      set;
    }

    public int ListItemId
    {
      get;
      set;
    }

    public string ListTitle
    {
      get;
      set;
    }

    public byte[] OriginatingUserToken
    {
      get;
      set;
    }

    public string ReceiverData
    {
      get;
      set;
    }

    public string RedirectUrl
    {
      get;
      set;
    }

    public string RelativeWebUrl
    {
      get;
      set;
    }

    public Guid SiteId
    {
      get;
      set;
    }

    public SPEventReceiverStatus Status
    {
      get;
      set;
    }

    public string UserDisplayName
    {
      get;
      set;
    }

    public string UserLoginName
    {
      get;
      set;
    }

    public bool Versionless
    {
      get;
      set;
    }

    public string WebUrl
    {
      get;
      set;
    }

    public SPUrlZone Zone
    {
      get;
      set;
    }

    public static BaristaItemEventProperties CreateItemEventProperties(SPItemEventProperties properties)
    {
      var result = new BaristaItemEventProperties
        {
          AfterUrl = properties.AfterUrl,
          BeforeUrl = properties.BeforeUrl,
          CurrentUserId = properties.CurrentUserId,
          EventType = properties.EventType,
          FileSystemObjectTypePropertyName = properties.FileSystemObjectTypePropertyName,
          ListId = properties.ListId,
          ListItemId = properties.ListItemId,
          ListTitle = properties.ListTitle,
          OriginatingUserToken = properties.OriginatingUserToken.BinaryToken,
          ReceiverData = properties.ReceiverData,
          RedirectUrl = properties.RedirectUrl,
          RelativeWebUrl = properties.RelativeWebUrl,
          SiteId = properties.SiteId,
          Status = properties.Status,
          UserDisplayName = properties.UserDisplayName,
          UserLoginName = properties.UserLoginName,
          Versionless = properties.Versionless,
          WebUrl = properties.WebUrl,
          Zone = properties.Zone,
        };
      return result;
    }
  }
}
