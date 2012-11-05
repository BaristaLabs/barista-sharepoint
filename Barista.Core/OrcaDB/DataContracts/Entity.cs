namespace OFS.OrcaDB.Core
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Entity : DSEditableObject
  {
    [DataMember]
    public Guid Id
    {
      get;
      set;
    }

    [DataMember]
    public string Title
    {
      get;
      set;
    }

    [DataMember]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    public string Namespace
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
    public string ContentsETag
    {
      get;
      set;
    }

    [DataMember]
    public DateTime ContentsModified
    {
      get;
      set;
    }

    [DataMember]
    public string Path
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

  public class Entity<T> : Entity
  {
    public Entity()
    {
    }

    public Entity(Entity entity)
    {
      if (entity == null)
        throw new ArgumentNullException("entity", "When creating a clone of an entity, the entity parameter must not be null.");

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

      if (entity is Entity<T>)
      {
        var e = entity as Entity<T>;
        this.Value = e.Value;
      }
      else
      {
        this.Data = entity.Data;
      }
    }

    public override string Data
    {
      get
      {
        return DocumentStoreHelper.SerializeObjectToJson<T>(this.Value);
      }
      set
      {
        this.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(value);
      }
    }

    public virtual T Value
    {
      get;
      set;
    }
  }
}
