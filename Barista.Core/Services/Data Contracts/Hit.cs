namespace Barista.Services
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class Hit
  {
    [DataMember]
    public float Score
    {
      get;
      set;
    }

    [DataMember]
    public int DocumentId
    {
      get;
      set;
    }

    [DataMember]
    public Document Document
    {
      get;
      set;
    }
  }
}
