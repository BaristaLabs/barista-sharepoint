namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Barista.Library;
  using Barista.Newtonsoft.Json;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class EntityInstance : ObjectInstance, IEntity
  {
    private readonly IEntity m_entity;

    public EntityInstance(ScriptEngine engine, IEntity entity)
      : base(engine)
    {
      if (entity == null)
        throw new ArgumentNullException("entity");

      m_entity = entity;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IEntity Entity
    {
      get { return m_entity; }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_entity.Id); }
      set
      {
        if (value != null)
          m_entity.Id = value.Value;
      }
    }

    Guid IEntity.Id
    {
      get { return m_entity.Id; }
      set { m_entity.Id = value; }
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
    public object Namespace
    {
      get
      {
        Uri uri;
        if (Uri.TryCreate(m_entity.Namespace, UriKind.Absolute, out uri))
          return new UriInstance(this.Engine.Object.Prototype, uri);
        return m_entity.Namespace;
      }
      set { m_entity.Namespace = value.ToString(); }
    }

    string IEntity.Namespace
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
      set { m_entity.ContentsModified = DateTime.Parse(value.ToIsoString());  }
    }

    DateTime IEntity.ContentsModified
    {
      get { return m_entity.ContentsModified; }
      set { m_entity.ContentsModified = value; }
    }

    [JSProperty(Name = "path")]
    public string Path
    {
      get { return m_entity.Path; }
      set { m_entity.Path = value; }
    }

    [JSProperty(Name = "data")]
    public object Data
    {
      get
      {
        try
        {
// ReSharper disable RedundantArgumentDefaultValue
          var obj = JsonConvert.DeserializeObject(m_entity.Data);
          var value2 = JsonConvert.SerializeObject(obj, Formatting.None);
          var result = JSONObject.Parse(this.Engine, value2, null);
// ReSharper restore RedundantArgumentDefaultValue
          return result;
        }
        catch (Exception)
        {
          //If there was an issue converting to a JSON object, just return the string value.
          return m_entity.Data;
        }
      }
      set
      {

        if (value == Null.Value || value == Undefined.Value || value == null)
          m_entity.Data = String.Empty;
        else if (value is string || value is StringInstance || value is ConcatenatedString)
          m_entity.Data = value.ToString();
        else if (value is ObjectInstance)
          // ReSharper disable RedundantArgumentDefaultValue
          m_entity.Data = JSONObject.Stringify(this.Engine, value, null, null);
          // ReSharper restore RedundantArgumentDefaultValue
        else
          m_entity.Data = value.ToString();
      }
    }

    string IEntity.Data
    {
      get { return m_entity.Data; }
      set { m_entity.Data = value; }
    }

    [JSProperty(Name = "created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entity.Created); }
      set { m_entity.Created = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IEntity.Created
    {
      get { return m_entity.Created; }
      set { m_entity.Created = value; }
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

    IUser IEntity.CreatedBy
    {
      get { return m_entity.CreatedBy; }
      set { m_entity.CreatedBy = value; }
    }

    [JSProperty(Name = "modified")]
    public DateInstance Modified
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entity.Modified); }
      set { m_entity.Modified = DateTime.Parse(value.ToIsoString()); }
    }

    DateTime IEntity.Modified
    {
      get { return m_entity.Modified; }
      set { m_entity.Modified = value; }
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

    IUser IEntity.ModifiedBy
    {
      get { return m_entity.ModifiedBy; }
      set { m_entity.ModifiedBy = value; }
    }
  }
}
