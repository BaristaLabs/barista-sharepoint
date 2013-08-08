namespace Barista.DocumentStore.FileSystem.Data
{
  using System;
  using System.Collections.Generic;
  using Barista.Newtonsoft.Json;

  public class AttachmentMetadata
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

    [JsonProperty("fileName")]
    public string FileName
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

    [JsonProperty("path")]
    public string Path
    {
      get;
      set;
    }

    [JsonProperty("size")]
    public long Size
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

    [JsonProperty("timeLastModified")]
    public DateTime TimeLastModified
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
