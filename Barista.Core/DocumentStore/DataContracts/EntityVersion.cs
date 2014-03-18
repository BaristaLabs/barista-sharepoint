namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class EntityVersion
  {
    [DataMember]
    public int VersionId
    {
      get;
      set;
    }

    [DataMember]
    public string VersionLabel
    {
      get;
      set;
    }

    [DataMember]
    public bool IsCurrentVersion
    {
      get;
      set;
    }

    [DataMember]
    public string Comment
    {
      get;
      set;
    }

    [DataMember]
    public DateTime Created
    {
      get;
      set;
    }

    [DataMember]
    public string CreatedByLoginName
    {
      get;
      set;
    }

    [DataMember]
    public Entity Entity
    {
      get;
      set;
    }
  }
}
