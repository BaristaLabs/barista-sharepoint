namespace Barista.OrcaDB
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public abstract class DSObject
  {
    [DataMember]
    public DateTime Created
    {
      get;
      set;
    }

    [DataMember]
    public User CreatedBy
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public abstract class DSEditableObject : DSObject
  {
    [DataMember]
    public DateTime Modified
    {
      get;
      set;
    }

    [DataMember]
    public User ModifiedBy
    {
      get;
      set;
    }
  }
}
