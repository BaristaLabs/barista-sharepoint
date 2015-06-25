namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Container : DSEditableObject
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
    public string Url
    {
      get;
      set;
    }

    [DataMember]
    public int EntityCount
    {
      get;
      set;
    }
  }
}
