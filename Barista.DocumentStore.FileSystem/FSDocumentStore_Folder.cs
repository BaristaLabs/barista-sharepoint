namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.IO;

  public partial class FSDocumentStore
  {
    public Folder CreateFolder(string containerTitle, string path)
    {
      throw new NotImplementedException();
    }

    public void DeleteFolder(string containerTitle, string path)
    {
      throw new NotImplementedException();
    }

    public Folder GetFolder(string containerTitle, string path)
    {
      throw new NotImplementedException();
    }

    public System.Collections.Generic.IList<Folder> ListAllFolders(string containerTitle, string path)
    {
      throw new NotImplementedException();
    }

    public System.Collections.Generic.IList<Folder> ListFolders(string containerTitle, string path)
    {
      throw new NotImplementedException();
    }

    public Folder RenameFolder(string containerTitle, string path, string newFolderName)
    {
      throw new NotImplementedException();
    }

    public Entity CreateEntity(string containerTitle, string path, string title, string @namespace, string data)
    {
      var newId = Guid.NewGuid();

      var packagePath = GetEntityPackagePath(containerTitle, path, newId);

      if (File.Exists(packagePath))
        throw new InvalidOperationException("An entity with the specified id already exists.");

      using (var package = EntityPackage.Create(packagePath, newId, @namespace, title, data))
      {
        return package.MapEntityFromPackage(true);
      }
    }

    public Entity GetEntity(string containerTitle, Guid entityId, string path)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var package = EntityPackage.Open(packagePath))
      {
        return package == null
          ? null
          : package.MapEntityFromPackage(true);
      }
    }

    public Entity GetEntityLight(string containerTitle, Guid entityId, string path)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      using (var package = EntityPackage.Open(packagePath))
      {
        return package == null
          ? null
          : package.MapEntityFromPackage(false);
      }
    }

    public Entity ImportEntity(string containerTitle, string path, Guid entityId, string @namespace, byte[] archiveData)
    {
      throw new NotImplementedException();
    }

    public System.Collections.Generic.IList<Entity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      throw new NotImplementedException();
    }

    public int CountEntities(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      throw new NotImplementedException();
    }

    public bool MoveEntity(string containerTitle, Guid entityId, string destinationPath)
    {
      throw new NotImplementedException();
    }
  }
}
