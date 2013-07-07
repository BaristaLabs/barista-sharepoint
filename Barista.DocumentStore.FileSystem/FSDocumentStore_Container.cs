namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using Barista.Newtonsoft.Json;

  public partial class FSDocumentStore
  {
    private readonly object m_syncRoot = new object();

    public Container CreateContainer(string containerTitle, string description)
    {
      lock (m_syncRoot)
      {
        var containerInfos = GetContainerInfos().ToList();

        if (containerInfos.Any(ci => ci.Title == containerTitle))
          throw new InvalidOperationException("A container with the specified name already exists in the document store.");

        var containerInfo = new ContainerInfo
          {
            Id = Guid.NewGuid(),
            Title = containerTitle,
            Description = description,
            CreatedBy = User.GetCurrentUser(),
            ModifiedBy = User.GetCurrentUser()
          };
        var containerDirectoryInfo = m_root.CreateSubdirectory(containerInfo.Id.ToString());
        containerInfo.Url = containerDirectoryInfo.FullName;

        containerInfos.Add(containerInfo);
        UpdateContainerInfos(containerInfos);
        return FSDocumentStoreHelper.MapContainerFromContainerInfo(containerInfo);
      }
    }

    public void DeleteContainer(string containerTitle)
    {
      lock (m_syncRoot)
      {
        var containerInfos = GetContainerInfos().ToList();
        var containerInfo = containerInfos.FirstOrDefault(ci => ci.Title == containerTitle);
        if (containerInfo == null)
          return;

        var di = new DirectoryInfo(containerInfo.Url);
        di.Delete(true);

        containerInfos.Remove(containerInfo);
        UpdateContainerInfos(containerInfos);
      }
    }

    public Container GetContainer(string containerTitle)
    {
      var containerInfos = GetContainerInfos().ToList();
      var containerInfo = containerInfos.FirstOrDefault(ci => ci.Title == containerTitle);

      if (containerInfo == null)
        return null;

      return FSDocumentStoreHelper.MapContainerFromContainerInfo(containerInfo);
    }

    public IList<Container> ListContainers()
    {
      var containerInfos = GetContainerInfos();
      return containerInfos
                   .Select(FSDocumentStoreHelper.MapContainerFromContainerInfo)
                   .ToList();
    }

    public bool UpdateContainer(Container container)
    {
      lock (m_syncRoot)
      {
        var containerInfos = GetContainerInfos().ToList();
        var containerInfo = containerInfos.FirstOrDefault(ci => ci.Title == container.Title);
        if (containerInfo == null)
          return false;

        containerInfo.Title = container.Title;
        containerInfo.Description = container.Description;
        containerInfo.ModifiedBy = User.GetCurrentUser();

        UpdateContainerInfos(containerInfos);
        return true;
      }
    }

    protected string GetContainerPath(string containerTitle)
    {
      var containerInfos = GetContainerInfos();
      var containerInfo = containerInfos.SingleOrDefault(ci => ci.Title == containerTitle);
      
      if (containerInfo == null)
        throw new InvalidOperationException("A Container with the specified title does not exist: " + containerTitle);

      return Path.Combine(m_root.FullName, containerInfo.Id.ToString());
    }

    protected IEnumerable<ContainerInfo> GetContainerInfos()
    {
      var containerInfoPath = Path.Combine(m_root.FullName, ".DS_Containers");

      var containers = new List<ContainerInfo>();
      if (File.Exists(containerInfoPath))
      {
        var text = File.ReadAllText(containerInfoPath);
        containers = JsonConvert.DeserializeObject<List<ContainerInfo>>(text);
      }

      return containers;
    }

    protected void UpdateContainerInfos(IEnumerable<ContainerInfo> containerInfos)
    {
      if (containerInfos == null)
        throw new ArgumentNullException("containerInfos", @"containerInfos cannot be null");

      var containerInfoPath = Path.Combine(m_root.FullName, ".DS_Containers");

      var text = JsonConvert.SerializeObject(containerInfos, Formatting.Indented);
      File.WriteAllText(containerInfoPath, text);
    }
  }
}
