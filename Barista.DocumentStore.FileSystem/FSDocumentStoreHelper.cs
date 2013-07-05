namespace Barista.DocumentStore.FileSystem
{
  using System.IO;
  using System.Linq;
  using System;
  using Microsoft.WindowsAPICodePack.Shell;

  public static class FSDocumentStoreHelper
  {
    public static Container MapContainerFromFolder(string path)
    {
      var containerInfo = new DirectoryInfo(path);
      var containerObj = ShellObject.FromParsingName(containerInfo.FullName);
      var properties = containerObj.Properties;

      return new Container()
        {
          Created = containerInfo.CreationTime,
          CreatedBy = new User
            {
              LoginName = properties.GetProperty<string>("Author").Value
            },
          Description = properties.GetProperty<string>("Description").Value,
          EntityCount = containerInfo.EnumerateFiles("*.dse", SearchOption.AllDirectories).Count(),
          Id = Guid.Parse(properties.GetProperty<string>("Document ID").Value),
          Modified = containerInfo.LastWriteTime,
          ModifiedBy = null, //Can I get this???,
          Title = properties.GetProperty<string>("Title").Value,
          Url = "", //What should this be??
        };
    }
  }
}
