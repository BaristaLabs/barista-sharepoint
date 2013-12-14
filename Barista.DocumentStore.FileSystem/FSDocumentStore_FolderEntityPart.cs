namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public partial class FSDocumentStore
  {
    public EntityPart CreateEntityPart(string containerTitle, string path, Guid entityId, string partName,
      string category, string data)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.CreateEntityPart(partName, category, data);
      }
    }

    public bool DeleteEntityPart(string containerTitle, string path, Guid entityId, string partName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.DeleteEntityPart(partName);
      }
    }

    public EntityPart GetEntityPart(string containerTitle, string path, Guid entityId, string partName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.GetEntityPart(partName, true);
      }
    }

    public bool RenameEntityPart(string containerTitle, string path, Guid entityId, string partName, string newPartName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.RenameEntityPart(partName, newPartName);
      }
    }

    public EntityPart UpdateEntityPart(string containerTitle, string path, Guid entityId, string partName, string category)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.UpdateEntityPart(partName, category);
      }
    }

    public EntityPart UpdateEntityPartData(string containerTitle, string path, Guid entityId, string partName, string eTag, string data)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.UpdateEntityPartData(partName, eTag, data);
      }
    }

    public IList<EntityPart> ListEntityParts(string containerTitle, string path, Guid entityId)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.ListEntityParts().ToList();
      }
    }
  }
}
