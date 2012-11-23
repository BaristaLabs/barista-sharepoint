namespace Barista.SharePoint.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Web;
  using System.Web.Caching;
  using Microsoft.SharePoint;
  using Newtonsoft.Json;
  using Barista.DocumentStore;

  /// <summary>
  /// Represents a SPDocumentStore that uses Asp.Net based caching to speed data access.
  /// </summary>
  public class SPMemoryCachedDocumentStore : SPDocumentStore
  {
    private static TimeSpan s_cacheSlidingExpiration = TimeSpan.FromMinutes(15);

    public SPMemoryCachedDocumentStore()
      : base()
    {
    }

    public override Entity GetEntity(string containerTitle, Guid entityId, string path)
    {
      var etag = base.GetEntityContentsETag(containerTitle, entityId);
      if (String.IsNullOrEmpty(etag))
        return null;

      string blobPath = String.Empty;
      string blobFileName = String.Empty;

      Entity entity = null;

      var cachedValue = HttpRuntime.Cache["ODB_Entity_" + etag];
      if (cachedValue != null && cachedValue is Entity)
        entity = cachedValue as Entity;

      if (entity == null)
      {
        entity = base.GetEntity(containerTitle, entityId, path);

        if (entity != null)
        {
          HttpRuntime.Cache.Add("ODB_Entity_" + etag,
            entity,
            null,
            Cache.NoAbsoluteExpiration,
            s_cacheSlidingExpiration,
            CacheItemPriority.Normal,
            null);
        }
        else
        {
          HttpRuntime.Cache.Remove("ODB_Entity_" + etag);
        }
      }

      return entity;
    }

    public override EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName)
    {
      var etag = base.GetEntityPartETag(containerTitle, entityId, partName);
      if (string.IsNullOrEmpty(etag))
        return null;

      string blobPath = String.Empty;
      string blobFileName = String.Empty;

      EntityPart entityPart = null;

      var cachedValue = HttpRuntime.Cache["ODB_EntityPart_" + etag];

      if (cachedValue != null && cachedValue is EntityPart)
        entityPart = cachedValue as EntityPart;

      if (entityPart == null)
      {
        entityPart = base.GetEntityPart(containerTitle, entityId, partName);

        if (entityPart != null)
        {
          HttpRuntime.Cache.Add("ODB_EntityPart_" + etag,
            entityPart,
            null,
            Cache.NoAbsoluteExpiration,
            s_cacheSlidingExpiration,
            CacheItemPriority.Normal,
            null);
        }
        else
        {
          HttpRuntime.Cache.Remove("ODB_EntityPart_" + etag);
        }
      }

      return entityPart;
    }

    public override IList<Entity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      var etag = base.GetFolderETag(containerTitle, path, criteria);
      if (String.IsNullOrEmpty(etag))
        return new List<Entity>();

      string blobPath = String.Empty;
      string blobFileName = String.Empty;

      IList<Entity> entityList = null;

      var cachedValue = HttpRuntime.Cache["ODB_List_" + etag];
      if (cachedValue != null && cachedValue is List<Entity>)
        entityList = cachedValue as List<Entity>;

      if (entityList == null)
      {
        entityList = base.ListEntities(containerTitle, path, criteria);

        if (entityList != null && entityList.Count > 0)
        {
          HttpRuntime.Cache.Add("ODB_List_" + etag,
            entityList,
            null,
            Cache.NoAbsoluteExpiration,
            s_cacheSlidingExpiration,
            CacheItemPriority.Normal,
            null);
        }
        else
        {
          HttpRuntime.Cache.Remove("ODB_List_" + etag);
        }
      }

      return entityList;
    }
  }
}
