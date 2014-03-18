namespace Barista.DocumentStore
{
  using System;
  using Barista.Newtonsoft.Json;

  public class EntityMetadata
  {
    [JsonProperty("entityId")]
    public Guid EntityId
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

    [JsonProperty("path")]
    public string Path
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

    [JsonProperty("eTag")]
    public string ETag
    {
      get;
      set;
    }

    [JsonProperty("contentsETag")]
    public string ContentsETag
    {
      get;
      set;
    }

    [JsonProperty("contentsModified")]
    public DateTime ContentsModified
    {
      get;
      set;
    }

    public static EntityMetadata CreateMetadata(IEntity entity)
    {
      if (entity == null)
        return null;

      var result = new EntityMetadata
      {
        EntityId = entity.Id,
        ContentsETag = entity.ContentsETag,
        ContentsModified = entity.ContentsModified,
        ETag = entity.ETag,
        Namespace = entity.Namespace,
        Path = entity.Path,
        Title = entity.Title,
        Description = entity.Description,
      };

      return result;
    }
  }
}
