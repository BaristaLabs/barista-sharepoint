namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  public class EntityPartInstance : ObjectInstance
  {
    EntityPart m_entityPart;

    public EntityPartInstance(ScriptEngine engine, EntityPart entityPart)
      : base(engine)
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart");

      m_entityPart = entityPart;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public EntityPart EntityPart
    {
      get { return m_entityPart; }
    }

    [JSProperty(Name = "entityId")]
    public string EntityId
    {
      get { return m_entityPart.EntityId.ToString(); }
      set { m_entityPart.EntityId = new Guid(value); }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_entityPart.Name; }
      set { m_entityPart.Name = value; }
    }

    [JSProperty(Name = "category")]
    public string Category
    {
      get { return m_entityPart.Category; }
      set { m_entityPart.Category = value; }
    }

    [JSProperty(Name = "eTag")]
    public string ETag
    {
      get { return m_entityPart.ETag; }
      set { m_entityPart.ETag = value; }
    }

    [JSProperty(Name = "data")]
    public virtual string Data
    {
      get { return m_entityPart.Data; }
      set { m_entityPart.Data = value; }
    }
  }
}
