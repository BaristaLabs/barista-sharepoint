namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

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

  public class EntityPart<T> : EntityPart
  {
    public EntityPart()
    {
    }

    public EntityPart(EntityPart entityPart)
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart");

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

    public virtual T Value
    {
      get;
      set;
    }
  }
}
