namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;

  public partial class FSDocumentStore
  {
    public EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string category, string data)
    {
      return CreateEntityPart(containerTitle, null, entityId, partName, category, data);
    }

    public bool DeleteEntityPart(string containerTitle, Guid entityId, string partName)
    {
      return DeleteEntityPart(containerTitle, null, entityId, partName);
    }

    public EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName)
    {
      return GetEntityPart(containerTitle, null, entityId, partName);
    }

    public IList<EntityPart> ListEntityParts(string containerTitle, Guid entityId)
    {
      return ListEntityParts(containerTitle, null, entityId);
    }

    public bool RenameEntityPart(string containerTitle, Guid entityId, string partName, string newPartName)
    {
      return RenameEntityPart(containerTitle, null, entityId, partName, newPartName);
    }

    public EntityPart UpdateEntityPart(string containerTitle, Guid entityId, string partName, string category)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.UpdateEntityPart(partName, category);
      }
    }

    public EntityPart UpdateEntityPartData(string containerTitle, Guid entityId, string partName, string eTag, string data)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.UpdateEntityPartData(partName, eTag, data);
      }
    }
  }
}
