namespace Barista.DocumentStore
{
  using System.Runtime.Serialization;

  public interface IGroup
  {
    [DataMember]
    string LoginName
    {
      get;
      set;
    }

    [DataMember]
    string DistributionGroupEmail
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
  }
}