namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.IO;
  using Barista.Newtonsoft.Json;

  /// <summary>
  /// Represents a physical representation of a container that uses the file system as an underlying store.
  /// </summary>
  public sealed class EntityContainer
  {
    private readonly DirectoryInfo m_rootDirectory;

    public EntityContainer(string path)
    {
      m_rootDirectory = new DirectoryInfo(path);
      if (m_rootDirectory.Exists == false)
        m_rootDirectory.Create();
    }

    #region Properties
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
    #endregion
  }
}
