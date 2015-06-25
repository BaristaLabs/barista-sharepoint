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

    public Entity Entity
    {
      get;
      set;
    }

    public EntityPart EntityPart
    {
      get;
      set;
    }

    public string EntityPartName
    {
      get;
      set;
    }

    public Attachment Attachment
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
