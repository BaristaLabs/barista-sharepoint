namespace Barista.Search
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Barista.Newtonsoft.Json;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class JsonDocumentDto
  {
    public JsonDocumentDto()
    {
      this.FieldOptions = new List<FieldOptions>();
    }

    [DataMember]
    [JsonProperty("documentId")]
    public string DocumentId
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("fieldOptions")]
    public IEnumerable<FieldOptions> FieldOptions
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("metadataAsJson")]
    public string MetadataAsJson
    {
      get;
      set;
    }

    [DataMember]
    [JsonProperty("dataAsJson")]
    public string DataAsJson
    {
      get;
      set;
    }
  }
}
