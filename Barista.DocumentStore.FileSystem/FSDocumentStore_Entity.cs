namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.IO.Packaging;
  using Barista.Framework;
  using Barista.Newtonsoft.Json;

  public partial class FSDocumentStore
  {
    public Entity CreateEntity(string containerTitle, string title, string @namespace, string data)
    {
      return CreateEntityInternal(null, containerTitle, title, @namespace, data);
    }

    protected Entity CreateEntityInternal(string path, string containerTitle, string title, string @namespace, string data)
    {
      var newId = Guid.NewGuid();

      var packagePath = GetEntityPackagePath(containerTitle, path, newId);

      if (File.Exists(packagePath))
        throw new InvalidOperationException("An entity with the specified id already exists.");

      var metadata = new EntityMetadata
        {
          Id = newId,
          Title = title,
          Namespace = @namespace,
          Created = DateTime.Now,
          CreatedBy = User.GetCurrentUser(),
          Modified = DateTime.Now,
          ModifiedBy = User.GetCurrentUser()
        };

      using (var package =
        Package.Open(packagePath, FileMode.Create))
      {
        // Add the metadata part to the Package.
        package.PackageProperties.ContentType = "application/barista-entity";
        package.PackageProperties.Identifier = newId.ToString();
        package.PackageProperties.Subject = @namespace;

        var metadataEntityPart =
            package.CreatePart(new Uri(Barista.DocumentStore.Constants.MetadataV1Namespace + "entity.json", UriKind.Relative),
                           "application/json");

        if (metadataEntityPart == null)
          throw new InvalidOperationException("The Metadata Part Document could not be created.");

        //Copy the metadata to the package part 
        var metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);
        using (var fileStream = new StringStream(metadataJson))
        {
          fileStream.CopyTo(metadataEntityPart.GetStream());
        }

        // Add the default entity part to the Package.
        var defaultEntityPart =
            package.CreatePart(new Uri(Barista.DocumentStore.Constants.EntityPartV1Namespace + "default.dsep", UriKind.Relative), 
                           "application/json");

        if (defaultEntityPart == null)
          throw new InvalidOperationException("The Default Entity Part Document could not be created.");

        //Copy the data to the default entity part 
        using (var fileStream = new StringStream(data))
        {
          fileStream.CopyTo(defaultEntityPart.GetStream());
        }

        return FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    public bool DeleteEntity(string containerTitle, Guid entityId)
    {
      return DeleteEntityInternal(containerTitle, null, entityId);
    }

    protected bool DeleteEntityInternal(string containerTitle, string path, Guid entityId)
    {
      using(var package = GetEntityPackage(containerTitle, path, entityId))
      {
        if (package == null)
          return false;


      }
    }

    public System.IO.Stream ExportEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public Entity GetEntity(string containerTitle, Guid entityId)
    {
      return GetEntityInternal(containerTitle, null, entityId);
    }

    protected Entity GetEntityInternal(string containerTitle, string path, Guid entityId)
    {
      using (var package = GetEntityPackage(containerTitle, path, entityId))
      {
        return package == null
          ? null
          : FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    public Entity GetEntityLight(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, byte[] archiveData)
    {
      throw new NotImplementedException();
    }

    public IList<Entity> ListEntities(string containerTitle, EntityFilterCriteria criteria)
    {
      throw new NotImplementedException();
    }

    public int CountEntities(string containerTitle, EntityFilterCriteria criteria)
    {
      throw new NotImplementedException();
    }

    public Entity UpdateEntity(string containerTitle, Guid entityId, string title, string description, string @namespace)
    {
      throw new NotImplementedException();
    }

    public Entity UpdateEntityData(string containerTitle, Guid entityId, string eTag, string data)
    {
      throw new NotImplementedException();
    }

    protected string GetEntityPackagePath(string containerTitle, string path, Guid entityId)
    {
      var packagePath = path;

      packagePath = String.IsNullOrWhiteSpace(packagePath)
        ? Path.Combine(GetContainerPath(containerTitle), entityId + ".dse")
        : Path.Combine(GetContainerPath(containerTitle), packagePath, entityId + ".dse");

      return packagePath;
    }

    protected Package GetEntityPackage(string containerTitle, string path, Guid entityId)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);
      if (File.Exists(packagePath))
      {
        return Package.Open(packagePath, FileMode.Open);
      }

      var packages = Directory.GetFiles(GetContainerPath(containerTitle), entityId + ".dse", SearchOption.AllDirectories);

      if (packages.Length <= 0)
        return null;

      if (packages.Length > 1)
        throw new InvalidOperationException(
          String.Format("Multiple Entities with the same Id were found in the container: {0} {1}", containerTitle,
                        entityId));

      return Package.Open(packages[0], FileMode.Open);
    }
  }
}
