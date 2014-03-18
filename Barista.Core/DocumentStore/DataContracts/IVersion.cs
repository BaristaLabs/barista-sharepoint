namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  public interface IVersion
  {
    [DataMember]
    int VersionId
    {
      get;
      set;
    }

    [DataMember]
    string VersionLabel
    {
      get;
      set;
    }

    [DataMember]
    bool IsCurrentVersion
    {
      get;
      set;
    }

    [DataMember]
    string Comment
    {
      get;
      set;
    }

    [DataMember]
    DateTime Created
    {
      get;
      set;
    }

    [DataMember]
    string CreatedByLoginName
    {
      get;
      set;
    }

    [DataMember]
    IDSObject DSObject
    {
      get;
      set;
    }
  }
}