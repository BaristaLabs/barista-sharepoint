namespace Barista.DocumentStore
{
  using System;

  /// <summary>
  /// Represents a typed entity object.
  /// </summary>
  /// <remarks>
  /// Acts as a wrapper around an existing Entity and exposes methods to get the data as a strongly typed object.
  /// </remarks>
  /// <typeparam name="T"></typeparam>
  public sealed class Entity<T> : IEntity
  {
    private readonly object m_syncRoot = new object();
    private readonly IEntity m_entity;

    private bool m_hasStronglyTypedValue;
    private T m_stronglyTypedValue;

    /// <summary>
    /// Creates a strongly typed entity wrapper.
    /// </summary>
    /// <param name="entity"></param>
    public Entity(IEntity entity)
    {
      if (entity == null)
        throw new ArgumentNullException("entity",
          @"When creating a clone of an entity, the entity parameter must not be null.");

      m_entity = entity;
    }

    public IEntity WrappedEntity
    {
      get { return m_entity; }
    }

    /// <summary>
    /// Get or sets the string representation of the entity.
    /// </summary>
    public string Data
    {
      get
      {
        lock (m_syncRoot)
        {
          return m_entity.Data;
        }
      }
      set
      {
        lock (m_syncRoot)
        {
          m_hasStronglyTypedValue = false;
          m_stronglyTypedValue = default(T);
          m_entity.Data = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the strongly typed representation of the entity.
    /// </summary>
    public T Value
    {
      get
      {
        lock (m_syncRoot)
        {
          if (m_hasStronglyTypedValue == false)
          {
            m_stronglyTypedValue = DocumentStoreHelper.DeserializeObjectFromJson<T>(m_entity.Data);
            m_hasStronglyTypedValue = true;
          }

          return m_stronglyTypedValue;
        }
      }
      set
      {
        lock (m_syncRoot)
        {
          m_entity.Data = DocumentStoreHelper.SerializeObjectToJson(this.Value);
          m_hasStronglyTypedValue = true;
          m_stronglyTypedValue = value;
        }
      }
    }

    public Guid Id
    {
      get { return m_entity.Id; }
      set { m_entity.Id = value; }
    }

    public string Title
    {
      get { return m_entity.Title; }
      set { m_entity.Title = value; }
    }

    public string Description
    {
      get { return m_entity.Description; }
      set { m_entity.Description = value; }
    }

    public string Namespace
    {
      get { return m_entity.Namespace; }
      set { m_entity.Namespace = value; }
    }

    public string ETag
    {
      get { return m_entity.ETag; }
      set { m_entity.ETag = value; }
    }

    public string ContentsETag
    {
      get { return m_entity.ContentsETag; }
      set { m_entity.ContentsETag = value; }
    }

    public DateTime ContentsModified
    {
      get { return m_entity.ContentsModified; }
      set { m_entity.ContentsModified = value; }
    }

    public string Path
    {
      get { return m_entity.Path; }
      set { m_entity.Path = value; }
    }

    public DateTime Modified
    {
      get { return m_entity.Modified; }
      set { m_entity.Modified = value; }
    }

    public IUser ModifiedBy
    {
      get { return m_entity.ModifiedBy; }
      set { m_entity.ModifiedBy = value; }
    }

    public DateTime Created
    {
      get { return m_entity.Created; }
      set { m_entity.Created = value; }
    }

    public IUser CreatedBy
    {
      get { return m_entity.CreatedBy; }
      set { m_entity.CreatedBy = value; }
    }
  }
}
