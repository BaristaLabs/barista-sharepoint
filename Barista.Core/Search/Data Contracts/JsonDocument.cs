namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class JsonDocumentDto
  {
    [DataMember]
    public string DocumentId
    {
      get;
      set;
    }

    [DataMember]
    public string MetadataAsJson
    {
      get;
      set;
    }

    [DataMember]
    public string DataAsJson
    {
      get;
      set;
    }
  }
}
