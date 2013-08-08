namespace Barista.DocumentStore.FileSystem
{
  using Barista.Newtonsoft.Json;
  using System;

  public sealed class ContainerInfo
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

    [JsonProperty("description")]
    public string Description
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

    [JsonProperty("modifiedBy")]
    public User ModifiedBy
    {
      get;
      set;
    }

    [JsonProperty("url")]
    public string Url
    {
      get;
      set;
    }
  }
}
