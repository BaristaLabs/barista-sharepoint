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

    [JsonProperty("createdBy")]
    public User CreatedBy
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

    [JsonProperty("modifiedBy")]
    public User ModifiedBy
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

  }
}
