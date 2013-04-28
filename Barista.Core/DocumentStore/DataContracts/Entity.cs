namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents an untyped entity object.
  /// </summary>
  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Entity : DSEditableObject
  {
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    [DataMember]
    public Guid Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the title of the entity.
    /// </summary>
    [DataMember]
    public string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a textual description of the entity.
    /// </summary>
    [DataMember]
    public string Description
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the entity namespace -- which can be used to denote type.
    /// </summary>
    [DataMember]
    public string Namespace
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the eTag of the entity which can be used to indicate if an entity has been modified.
    /// </summary>
    [DataMember]
    public string ETag
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the eTag of the entity data.
    /// </summary>
    [DataMember]
    public string ContentsETag
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value that indicates when the entity data was last modified.
    /// </summary>
    [DataMember]
    public DateTime ContentsModified
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value that indicates the path of the location of the entity (For IFolderCapable Document STores.)
    /// </summary>
    [DataMember]
    public string Path
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the data of the entity
    /// </summary>
    [DataMember]
    public virtual string Data
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a typed entity object.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class Entity<T> : Entity
  {
    /// <summary>
    /// Creates a new instance of an entity.
    /// </summary>
    public Entity()
    {
    }

    /// <summary>
    /// Creates a strongly typed clone of an entity.
    /// </summary>
    /// <param name="entity"></param>
    public Entity(Entity entity)
    {
      if (entity == null)
        throw new ArgumentNullException("entity", @"When creating a clone of an entity, the entity parameter must not be null.");

      this.ContentsETag = entity.ContentsETag;
      this.ContentsModified = entity.ContentsModified;
      this.Created = entity.Created;
      this.CreatedBy = entity.CreatedBy;
      this.Data = entity.Data;
      this.Description = entity.Description;
      this.ETag = entity.ETag;
      this.Id = entity.Id;
      this.Modified = entity.Modified;
      this.ModifiedBy = entity.ModifiedBy;
      this.Namespace = entity.Namespace;
      this.Title = entity.Title;
      this.Path = entity.Path;

      //By setting the Data property, we're implicitly setting the Value property as well.
      //If the passed object is a strongly-typed entity, don't copy the value type.
      this.Data = entity.Data;
    }

    /// <summary>
    /// Get or sets the string representation of the entity.
    /// </summary>
    public override string Data
    {
      get
      {
        return DocumentStoreHelper.SerializeObjectToJson(this.Value);
      }
      set
      {
        this.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(value);
      }
    }

    public T Value
    {
      get;
      set;
    }
  }
}
