namespace OFS.OrcaDB.Core
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class EntityPartVersion
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
    public EntityPart EntityPart
    {
      get;
      set;
    }
  }
}
