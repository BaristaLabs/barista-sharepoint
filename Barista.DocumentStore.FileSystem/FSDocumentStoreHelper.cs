namespace Barista.DocumentStore.FileSystem
{
  using System.IO;
  using System.IO.Packaging;
  using System;
  using System.Linq;
  using System.Text;
  using System.Web;
  using Barista.DocumentStore.FileSystem.Data;
  using Barista.Newtonsoft.Json;

  public static class FSDocumentStoreHelper
  {
    

    public static Container MapContainerFromContainerInfo(ContainerInfo containerInfo)
    {
      var di = new DirectoryInfo(containerInfo.Url);

      return new Container
      {
        Created = di.CreationTime,
        CreatedBy = containerInfo.CreatedBy,
        Description = containerInfo.Description,
        EntityCount = di.GetFiles("*.dse", SearchOption.AllDirectories).Count(),
        Id = containerInfo.Id,
        Modified = di.LastWriteTime,
        ModifiedBy = containerInfo.ModifiedBy,
        Title = containerInfo.Title,
        Url = containerInfo.Url
      };
    }

    private static Guid GetEntityIdFromPackageFileName(string packageFileName)
    {
      var idString = packageFileName.Substring(0, packageFileName.LastIndexOf('.'));
      return new Guid(idString);
    }
  }
}
