﻿namespace Barista.SharePoint.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
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

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entity.Created); }
      set { m_entity.Created = DateTime.Parse(value.ToISOString()); }
    }

    [JSProperty(Name = "createdBy")]
    public object CreatedBy
    {
      get
      {
        if (m_entity.CreatedBy == null)
          return Null.Value;

        return m_entity.CreatedBy.LoginName;
      }
    }

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entity.Modified); }
      set { m_entity.Modified = DateTime.Parse(value.ToISOString()); }
    }

    [JSProperty(Name = "modifiedBy")]
    public object ModifiedBy
    {
      get
      {
        if (m_entity.ModifiedBy == null)
          return Null.Value;

        return m_entity.ModifiedBy.LoginName;
      }
    }
  }
}
