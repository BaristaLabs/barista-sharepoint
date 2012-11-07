namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  [KnownType(typeof(User))]
  [KnownType(typeof(Group))]
  public class Principal
  {
    [DataMember]
    public string Name
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class User : Principal
  {
    [DataMember]
    public string LoginName
    {
      get;
      set;
    }

    [DataMember]
    public string Email
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Group : Principal
  {
    [DataMember]
    public string LoginName
    {
      get;
      set;
    }

    [DataMember]
    public string DistributionGroupEmail
    {
      get;
      set;
    }
  }

}
