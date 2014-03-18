namespace Barista.DocumentStore
{
  using System;

  public class TriggerProperties
  {
    public TriggerType TriggerType
    {
      get;
      set;
    }

    public Guid EntityId
    {
      get;
      set;
    }

    public IEntity Entity
    {
      get;
      set;
    }

    public IEntityPart EntityPart
    {
      get;
      set;
    }

    public string EntityPartName
    {
      get;
      set;
    }

    public IAttachment Attachment
    {
      get;
      set;
    }

    public string AttachmentFileName
    {
      get;
      set;
    }
  }
}
