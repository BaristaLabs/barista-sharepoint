namespace Barista.Services
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class SearchResult
  {
    [DataMember]
    public float Score
    {
      get;
      set;
    }

    [DataMember]
    public JsonDocument Document
    {
      get;
      set;
    }
  }
}
