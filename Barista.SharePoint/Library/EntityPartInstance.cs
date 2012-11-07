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
    public Guid EntityId
    {
      get;
      set;
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get;
      set;
    }

    [JSProperty(Name = "category")]
    public string Category
    {
      get;
      set;
    }

    [JSProperty(Name = "eTag")]
    public string ETag
    {
      get;
      set;
    }

    [JSProperty(Name = "data")]
    public virtual string Data
    {
      get;
      set;
    }
  }
}
