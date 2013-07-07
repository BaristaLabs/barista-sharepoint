namespace Barista.DocumentStore.FileSystem
{
  using System;
  using Barista.Newtonsoft.Json;

  public class EntityMetadata
  {
    [JsonProperty("id")]
    public Guid Id
    {
      get;
      set;
    }

    [JsonProperty("title")]
    public string Title
    {
      get;
      set;
    }

    [JsonProperty("namespace")]
    public string Namespace
    {
      get;
      set;
    }

  }
}
