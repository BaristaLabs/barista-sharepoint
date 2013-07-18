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

    public static Entity MapEntityFromPackage(string packagePath, bool includeData)
    {
      var fi = new FileInfo(packagePath);

      if (fi.Exists == false)
        throw new InvalidOperationException("An entity package at the specified path does not exist.");

      using (var package =
        Package.Open(packagePath, FileMode.Open))
      {
        return MapEntityFromPackage(package, includeData);
      }
    }

    public static Entity MapEntityFromPackage(Package package, bool includeData)
    {
      var result = new Entity();

      result.Id = Guid.Parse(package.PackageProperties.Identifier);
      result.Title = package.PackageProperties.Title;
      result.Namespace = package.PackageProperties.Subject;
      result.Description = package.PackageProperties.Description;

      if (package.PackageProperties.Created.HasValue)
        result.Created = package.PackageProperties.Created.Value;
      result.CreatedBy = User.GetUser(package.PackageProperties.Creator);


      if (package.PackageProperties.Modified.HasValue)
        result.Modified = package.PackageProperties.Modified.Value;
      result.CreatedBy = User.GetUser(package.PackageProperties.Creator);

      if (package.PackageProperties.Modified.HasValue)
        result.Modified = package.PackageProperties.Modified.Value;
      result.ModifiedBy = User.GetUser(package.PackageProperties.LastModifiedBy);

      if (includeData)
      {
        var defaultEntityPart =
          package.GetPart(new Uri(Barista.DocumentStore.Constants.EntityPartV1Namespace + "default.dsep",
                                  UriKind.Relative));

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
