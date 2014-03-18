namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  public interface IEntityPart : IDSObject, IDSComments, IDSMetadata, IDSVersions, IDSPermissions
  {
    [DataMember]
    Guid EntityId
    {
      get;
      set;
    }

    [DataMember]
    string Name
    {
      get;
      set;
    }

    [DataMember]
    string Category
    {
      get;
      set;
    }

    [DataMember]
    string ETag
    {
      get;
      set;
    }

    [DataMember]
    string Data
    {
      get;
      set;
    }
  }
}