namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  public class EntityInstance : ObjectInstance
  {
    Entity m_entity;

    public EntityInstance(ScriptEngine engine, Entity entity)
      : base(engine)
    {
      if (entity == null)
        throw new ArgumentNullException("entity");

      m_entity = entity;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public Entity Entity
    {
      get { return m_entity; }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get { return m_entity.Id.ToString(); }
      set { m_entity.Id = new Guid(value); }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get { return m_entity.Title; }
      set { m_entity.Title = value; }
    }

    [JSProperty(Name = "description")]
    public string Description
    {
      get { return m_entity.Description; }
      set { m_entity.Description = value; }
    }

    [JSProperty(Name = "namespace")]
    public string Namespace
    {
      get { return m_entity.Namespace; }
      set { m_entity.Namespace = value; }
    }

    [JSProperty(Name = "eTag")]
    public string ETag
    {
      get { return m_entity.ETag; }
      set { m_entity.ETag = value; }
    }

    [JSProperty(Name = "contentsETag")]
    public string ContentsETag
    {
      get { return m_entity.ContentsETag; }
      set { m_entity.ContentsETag = value; }
    }

    [JSProperty(Name = "contentsModified")]
    public DateInstance ContentsModified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entity.ContentsModified); }
      set { m_entity.ContentsModified = DateTime.Parse(value.ToISOString());  }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get { return m_entity.Path; }
      set { m_entity.Path = value; }
    }

    [JSProperty(Name = "data")]
    public string Data
    {
      get { return m_entity.Data; }
      set { m_entity.Data = value; }
    }
  }
}
