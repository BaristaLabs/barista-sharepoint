namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents an untyped entity part.
  /// </summary>
  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class EntityPart : DSEditableObject
  {
    [DataMember]
    public Guid EntityId
    {
      get;
      set;
    }

    [DataMember]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    public string Category
    {
      get;
      set;
    }

    [DataMember]
    public string ETag
    {
      get;
      set;
    }

    [DataMember]
    public virtual string Data
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a strongly-typed entity part.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class EntityPart<T> : EntityPart
  {
    /// <summary>
    /// Creates a new entity part.
    /// </summary>
    public EntityPart()
    {
    }

    /// <summary>
    /// Creates and returns a clone of the specified entity part.
    /// </summary>
    /// <remarks>
    /// All properties of the passed entity part are retained -- such as created, created by and so forth.
    /// </remarks>
    /// <param name="entityPart"></param>
    public EntityPart(EntityPart entityPart)
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart", @"When a clone of an entity part is created, the entity part must not be null.");

      this.Category = entityPart.Category;
      this.Created = entityPart.Created;
      this.CreatedBy = entityPart.CreatedBy;
      this.Data = entityPart.Data;
      this.EntityId = entityPart.EntityId;
      this.ETag = entityPart.ETag;
      this.Modified = entityPart.Modified;
      this.ModifiedBy = entityPart.ModifiedBy;
      this.Name = entityPart.Name;

      this.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(entityPart.Data);
    }

    /// <summary>
    /// Gets or sets the strongly-typed value of the entity part.
    /// </summary>
    public virtual T Value
    {
      get;
      set;
    }
  }
}
