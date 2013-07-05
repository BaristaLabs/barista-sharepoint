namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Extensions;
  using Microsoft.WindowsAPICodePack.Shell;

  public partial class FSDocumentStore
  {
    public Container CreateContainer(string containerTitle, string description)
    {
      if (containerTitle.IsValidPath() == false)
        throw new ArgumentException(@"The specified container has invalid characters: " + containerTitle, "containerTitle");


      if (m_root.GetDirectories().Any(di => di.Name == containerTitle))
        throw new InvalidOperationException("A container with the specified title already exists.");

      var containerInfo = m_root.CreateSubdirectory(containerTitle);

      var container = ShellObject.FromParsingName(containerInfo.FullName);
      var properties = container.Properties;
      var descriptionProperty = properties.GetProperty<string>("Description");
      descriptionProperty.Value = description;

      var idProperty = properties.GetProperty<string>("Document ID");
      idProperty.Value = Guid.NewGuid().ToString();

      return FSDocumentStoreHelper.MapContainerFromFolder(containerInfo.FullName);
    }

    public void DeleteContainer(string containerTitle)
    {
      var containerInfo = m_root.EnumerateDirectories(containerTitle).SingleOrDefault();

      if (containerInfo == null)
        return;

      containerInfo.Delete(true);
    }

    public Container GetContainer(string containerTitle)
    {
      var containerInfo = m_root.EnumerateDirectories(containerTitle).SingleOrDefault();

      if (containerInfo == null)
        return null;

      return FSDocumentStoreHelper.MapContainerFromFolder(containerInfo.FullName);
    }

    public IList<Container> ListContainers()
    {
      return m_root.EnumerateDirectories()
                   .Select(di => FSDocumentStoreHelper.MapContainerFromFolder(di.FullName))
                   .ToList();
    }

    public bool UpdateContainer(Container container)
    {
      var containerInfo = m_root.EnumerateDirectories(container.Title).SingleOrDefault();

      if (containerInfo == null)
        return false;

      var containerObj = ShellObject.FromParsingName(containerInfo.FullName);
      var properties = containerObj.Properties;
      var descriptionProperty = properties.GetProperty<string>("Description");
      descriptionProperty.Value = container.Description;

      return true;
    }
  }
}
