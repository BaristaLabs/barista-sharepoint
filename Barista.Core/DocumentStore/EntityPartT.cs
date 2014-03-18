namespace Barista.DocumentStore
{
  using System;

  /// <summary>
  /// Represents a strongly-typed entity part.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class EntityPart<T> : IEntityPart
  {
    private readonly IEntityPart m_entityPart;
    private readonly object m_syncRoot = new object();

    private bool m_hasStronglyTypedValue;
    private T m_stronglyTypedValue;

    /// <summary>
    /// Creates and returns a clone of the specified entity part.
    /// </summary>
    /// <remarks>
    /// All properties of the passed entity part are retained -- such as created, created by and so forth.
    /// </remarks>
    /// <param name="entityPart"></param>
    public EntityPart(IEntityPart entityPart)
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart",
          @"When a clone of an entity part is created, the entity part must not be null.");

      m_entityPart = entityPart;
    }

    public IEntityPart WrappedEntityPart
    {
      get { return m_entityPart; }
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
            m_stronglyTypedValue = DocumentStoreHelper.DeserializeObjectFromJson<T>(m_entityPart.Data);
            m_hasStronglyTypedValue = true;
          }

          return m_stronglyTypedValue;
        }
      }
      set
      {
        lock (m_syncRoot)
        {
          m_entityPart.Data = DocumentStoreHelper.SerializeObjectToJson(this.Value);
          m_hasStronglyTypedValue = true;
          m_stronglyTypedValue = value;
        }
      }
    }

    /// <summary>
    /// Get or sets the string representation of the entity part;
    /// </summary>
    public string Data
    {
      get
      {
        lock (m_syncRoot)
        {
          return m_entityPart.Data;
        }
      }
      set
      {
        lock (m_syncRoot)
        {
          m_hasStronglyTypedValue = false;
          m_stronglyTypedValue = default(T);
          m_entityPart.Data = value;
        }
      }
    }

    public Guid EntityId
    {
      get { return m_entityPart.EntityId; }
      set { m_entityPart.EntityId = value; }
    }

    public string Name
    {
      get { return m_entityPart.Name; }
      set { m_entityPart.Name = value; }
    }

    public string Category
    {
      get { return m_entityPart.Category; }
      set { m_entityPart.Category = value; }
    }

    public string ETag
    {
      get { return m_entityPart.ETag; }
      set { m_entityPart.ETag = value; }
    }

    public DateTime Modified
    {
      get { return m_entityPart.Modified; }
      set { m_entityPart.Modified = value; }
    }

    public IUser ModifiedBy
    {
      get { return m_entityPart.ModifiedBy; }
      set { m_entityPart.ModifiedBy = value; }
    }

    public DateTime Created
    {
      get { return m_entityPart.Created; }
      set { m_entityPart.Created = value; }
    }

    public IUser CreatedBy
    {
      get { return m_entityPart.CreatedBy; }
      set { m_entityPart.CreatedBy = value; }
    }
  }
}