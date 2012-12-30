namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using Microsoft.SharePoint;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class BaristaItemEventPropertiesConstructor : ClrFunction
  {
    public BaristaItemEventPropertiesConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "BaristaItemEventProperties", new BaristaItemEventPropertiesInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public BaristaItemEventPropertiesInstance Construct()
    {
      return new BaristaItemEventPropertiesInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class BaristaItemEventPropertiesInstance : ObjectInstance
  {
    private readonly BaristaItemEventProperties m_baristaItemEventProperties;

    public BaristaItemEventPropertiesInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public BaristaItemEventPropertiesInstance(ObjectInstance prototype,
                                              BaristaItemEventProperties baristaItemEventProperties)
      : this(prototype)
    {
      if (baristaItemEventProperties == null)
        throw new ArgumentNullException("baristaItemEventProperties");

      m_baristaItemEventProperties = baristaItemEventProperties;
    }

    public BaristaItemEventProperties BaristaItemEventProperties
    {
      get { return m_baristaItemEventProperties; }
    }

    [JSProperty(Name = "afterUrl")]
    public string AfterUrl
    {
      get { return m_baristaItemEventProperties.AfterUrl; }
      set { m_baristaItemEventProperties.AfterUrl = value; }
    }

    [JSProperty(Name = "beforeUrl")]
    public string BeforeUrl
    {
      get { return m_baristaItemEventProperties.BeforeUrl; }
      set { m_baristaItemEventProperties.BeforeUrl = value; }
    }

    [JSProperty(Name = "currentUserId")]
    public int CurrentUserId
    {
      get { return m_baristaItemEventProperties.CurrentUserId; }
      set { m_baristaItemEventProperties.CurrentUserId = value; }
    }

    [JSProperty(Name = "eventType")]
    public string EventType
    {
      get { return m_baristaItemEventProperties.EventType.ToString(); }
      set { m_baristaItemEventProperties.EventType = (SPEventReceiverType)Enum.Parse(typeof(SPEventReceiverType), value); }
    }

    [JSProperty(Name = "fileSystemObjectTypePropertyName")]
    public string FileSystemObjectTypePropertyName
    {
      get { return m_baristaItemEventProperties.FileSystemObjectTypePropertyName; }
      set { m_baristaItemEventProperties.FileSystemObjectTypePropertyName = value; }
    }

    [JSProperty(Name = "listId")]
    public GuidInstance ListId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_baristaItemEventProperties.ListId); }
      set
      {
        if (value != null)
        {
          m_baristaItemEventProperties.ListId = value.Value;
        }
      }
    }

    [JSProperty(Name = "listItemId")]
    public int ListItemId
    {
      get { return m_baristaItemEventProperties.ListItemId; }
      set { m_baristaItemEventProperties.ListItemId = value; }
    }

    [JSProperty(Name = "listTitle")]
    public string ListTitle
    {
      get { return m_baristaItemEventProperties.ListTitle; }
      set { m_baristaItemEventProperties.ListTitle = value; }
    }

    [JSProperty(Name = "originatingUserToken")]
    public Base64EncodedByteArrayInstance OriginatingUserToken
    {
      get { return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_baristaItemEventProperties.OriginatingUserToken); }
      set
      {
        if (value != null)
        {
          m_baristaItemEventProperties.OriginatingUserToken = value.Data;
        }
      }
    }

    [JSProperty(Name = "receiverData")]
    public string ReceiverData
    {
      get { return m_baristaItemEventProperties.ReceiverData; }
      set { m_baristaItemEventProperties.ReceiverData = value; }
    }

    [JSProperty(Name = "redirectUrl")]
    public string RedirectUrl
    {
      get { return m_baristaItemEventProperties.RedirectUrl; }
      set { m_baristaItemEventProperties.RedirectUrl = value; }
    }

    [JSProperty(Name = "relativeWebUrl")]
    public string RelativeWebUrl
    {
      get { return m_baristaItemEventProperties.RelativeWebUrl; }
      set { m_baristaItemEventProperties.RelativeWebUrl = value; }
    }

    [JSProperty(Name = "siteId")]
    public GuidInstance SiteId
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_baristaItemEventProperties.SiteId); }
      set
      {
        if (value != null)
        {
          m_baristaItemEventProperties.SiteId = value.Value;
        }
      }
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_baristaItemEventProperties.Status.ToString(); }
      set { m_baristaItemEventProperties.Status = (SPEventReceiverStatus)Enum.Parse(typeof(SPEventReceiverStatus), value); }
    }

    [JSProperty(Name = "userDisplayName")]
    public string UserDisplayName
    {
      get { return m_baristaItemEventProperties.UserDisplayName; }
      set { m_baristaItemEventProperties.UserDisplayName = value; }
    }

    [JSProperty(Name = "userLoginName")]
    public string UserLoginName
    {
      get { return m_baristaItemEventProperties.UserLoginName; }
      set { m_baristaItemEventProperties.UserLoginName = value; }
    }

    [JSProperty(Name = "versionless")]
    public bool Versionless
    {
      get { return m_baristaItemEventProperties.Versionless; }
      set { m_baristaItemEventProperties.Versionless = value; }
    }

    [JSProperty(Name = "webUrl")]
    public string WebUrl
    {
      get { return m_baristaItemEventProperties.WebUrl; }
      set { m_baristaItemEventProperties.WebUrl = value; }
    }

    [JSProperty(Name = "zone")]
    public string Zone
    {
      get { return m_baristaItemEventProperties.Zone.ToString(); }
      set { m_baristaItemEventProperties.Zone = (SPUrlZone)Enum.Parse(typeof(SPUrlZone), value); }
    }
  }
}
