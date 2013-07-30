namespace Barista.Configuration
{
  using Barista.Newtonsoft.Json;

  public interface IIndexDefinitionConfig
  {
    [JsonProperty("name")]
    string IndexName
    {
      get;
    }

    [JsonProperty("description")]
    string Description
    {
      get;
    }

    [JsonProperty("typeName")]
    string TypeName
    {
      get;
    }

    [JsonProperty("indexStoragePath")]
    string IndexStoragePath
    {
      get;
    }
  }
}
