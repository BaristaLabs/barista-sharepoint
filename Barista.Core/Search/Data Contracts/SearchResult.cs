namespace Barista.Search
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
    public JsonDocumentDto Document
    {
      get;
      set;
    }
  }
}
