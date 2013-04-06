namespace Barista.Services
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class JsonDocument
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
