namespace Barista.DocumentStore.FileSystem.Data
{
  using System;
  using System.Collections.Generic;
  using Barista.Newtonsoft.Json;

  public class EntityPartMetadata
  {
    [JsonProperty("category")]
    public string Category
    {
      get;
      set;
    }

    [JsonProperty("created")]
    public DateTime Created
    {
      get;
      set;
    }

    [JsonProperty("createdBy")]
    public string CreatedBy
    {
      get;
      set;
    }

    [JsonProperty("etag")]
    public Etag Etag
    {
      get;
      set;
    }

    [JsonProperty("name")]
    public string Name
    {
      get;
      set;
    }

    [JsonProperty("modified")]
    public DateTime Modified
    {
      get;
      set;
    }

    [JsonProperty("modifiedBy")]
    public string ModifiedBy
    {
      get;
      set;
    }

    [JsonProperty("properties")]
    public Dictionary<string, string> Properties
    {
      get;
      set;
    }
  }
}
