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

    public bool UpdateEntityPart(string containerTitle, string path, Guid entityId, EntityPart entityPart)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        entityPackage.UpdateEntityPart(entityPart);
        return true;
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
