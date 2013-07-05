namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;

  public partial class FSDocumentStore
  {
    public Entity CreateEntity(string containerTitle, string title, string @namespace, string data)
    {
      throw new NotImplementedException();
    }

    public bool DeleteEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public System.IO.Stream ExportEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public Entity GetEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
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
  }
}
