namespace Barista.DocumentStore.FileSystem
{
  using System.IO;
  using System.IO.Packaging;
  using System.Linq;
  using System;
  using Microsoft.WindowsAPICodePack.Shell;
  using System.Text;
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
        EntityCount = di.EnumerateFiles("*.dse", SearchOption.AllDirectories).Count(),
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
      return Guid.Parse(idString);
    }

    public static Entity MapEntityFromPackage(string packagePath)
    {
      var fi = new FileInfo(packagePath);

      if (fi.Exists == false)
        throw new InvalidOperationException("An entity package at the specified path does not exist.");

      var result = new Entity();

      using (var package =
        Package.Open(packagePath, FileMode.Open))
      {
        var metadataPart =
          package.GetPart(new Uri(Barista.DocumentStore.Constants.MetadataV1Namespace + "entity.json", UriKind.Relative));

        using (var fs = metadataPart.GetStream())
        {
          var bytes = fs.ReadToEnd();
          var metadataJson = Encoding.UTF8.GetString(bytes);
          var metadata = JsonConvert.DeserializeObject<EntityMetadata>(metadataJson);
          result.Id = metadata.Id;
          result.Title = metadata.Title;
          result.Namespace = metadata.Namespace;
        }

        var defaultEntityPart =
          package.GetPart(new Uri(Barista.DocumentStore.Constants.EntityPartV1Namespace + "default.dsep", UriKind.Relative));


        using (var fs = defaultEntityPart.GetStream())
        {
          var bytes = fs.ReadToEnd();
          result.Data = Encoding.UTF8.GetString(bytes);
        }
      }

      return result;
    }
  }
}
