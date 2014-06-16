namespace Barista.SharePoint.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Web;
  using System.Web.Caching;
  using System.Linq;
  using Microsoft.SharePoint;
  using Barista.DocumentStore;
  using System.Collections;

  /// <summary>
  /// Represents a SPDocumentStore that uses Asp.Net based caching to speed data access.
  /// </summary>
  public class SPMemoryCachedDocumentStore : SPDocumentStore
  {
    private static readonly TimeSpan CacheSlidingExpiration = TimeSpan.FromMinutes(15);
    private const string EntityContentsCachePrefix = "BaristaDS_EntityContents_";

    public SPMemoryCachedDocumentStore()
    {
    }

    public SPMemoryCachedDocumentStore(SPWeb web)
      :base(web)
    {
    }

    public override Entity GetEntity(string containerTitle, Guid entityId, string path)
    {
      //Get a new web in case we're executing in elevated permissions.
      using (var site = new SPSite(this.DocumentStoreUrl))
      {
        using (var web = site.OpenWeb())
        {
          var contentsHash = SPDocumentStoreHelper.GetEntityContentsHash(web, containerTitle, entityId);

          //If the contents hash is not set, fall back on just retrieving the entity.
          if (String.IsNullOrEmpty(contentsHash))
            return base.GetEntity(containerTitle, entityId, path);

          //If we found it in the cache, return the entity value.
          var cachedValue = HttpRuntime.Cache[EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash];
          if (cachedValue is EntityContents)
          {
            var cachedEntityContents = cachedValue as EntityContents;
            
            //If it hasn't changed, return a clone of the entity.
            if (cachedEntityContents.Entity.ContentsETag == contentsHash)
              return DocumentStoreHelper.CloneObject(cachedEntityContents.Entity);
          }

          EntityContents entityContents = GetEntityContents(web, containerTitle, entityId);

          HttpRuntime.Cache.Add(EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash,
                                entityContents,
                                null,
                                Cache.NoAbsoluteExpiration,
                                CacheSlidingExpiration,
                                CacheItemPriority.Normal,
                                null);

          return DocumentStoreHelper.CloneObject(entityContents.Entity);
        }
      }
    }

    public override bool DeleteEntity(string containerTitle, Guid entityId)
    {
      foreach (var key in HttpRuntime.Cache
        .OfType<DictionaryEntry>()
        .ToList()
        .Select(cacheItem => cacheItem.Key as string)
        .Where(key => key != null &&
          key.StartsWith(EntityContentsCachePrefix + "_" + entityId)))
      {
        HttpRuntime.Cache.Remove(key);
      }

      return base.DeleteEntity(containerTitle, entityId);
    }

    public override EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName)
    {
      //Get a new web in case we're executing in elevated permissions.
      using (var site = new SPSite(this.DocumentStoreUrl))
      {
        using (var web = site.OpenWeb())
        {
          var contentsHash = SPDocumentStoreHelper.GetEntityContentsHash(web, containerTitle, entityId);

          //If the contents hash is not set, fall back on just retrieving the entity.
          if (String.IsNullOrEmpty(contentsHash))
            return base.GetEntityPart(containerTitle, entityId, partName);

          //If we found it in the cache, return the entity value.
          var cachedValue = HttpRuntime.Cache[EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash];
          if (cachedValue is EntityContents)
          {
            var cachedEntityContents = cachedValue as EntityContents;

            //If it hasn't changed, return a clone of the entity part.
            if (cachedEntityContents.Entity.ContentsETag == contentsHash)
            {
              if (cachedEntityContents.EntityParts.ContainsKey(partName) == false)
                return null;

              return DocumentStoreHelper.CloneObject(cachedEntityContents.EntityParts[partName]);
            }
          }

          var entityContents = GetEntityContents(web, containerTitle, entityId);

          HttpRuntime.Cache.Add(EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash,
                                entityContents,
                                null,
                                Cache.NoAbsoluteExpiration,
                                CacheSlidingExpiration,
                                CacheItemPriority.Normal,
                                null);

          if (entityContents.EntityParts.ContainsKey(partName) == false)
            return null;

          return DocumentStoreHelper.CloneObject(entityContents.EntityParts[partName]);
        }
      }
    }

    public override IList<EntityPart> ListEntityParts(string containerTitle, string path, Guid entityId)
    {
      //Get a new web in case we're executing in elevated permissions.
      using (var site = new SPSite(this.DocumentStoreUrl))
      {
        using (var web = site.OpenWeb())
        {
          var contentsHash = SPDocumentStoreHelper.GetEntityContentsHash(web, containerTitle, entityId);

          //If the contents hash is not set, fall back on just retrieving the entity.
          if (String.IsNullOrEmpty(contentsHash))
            return base.ListEntityParts(containerTitle, path, entityId);

          //If we found it in the cache, return the entity value.
          var cachedValue = HttpRuntime.Cache[EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash];
          if (cachedValue is EntityContents)
          {
            var cachedEntityContents = cachedValue as EntityContents;

            if (cachedEntityContents.Entity.ContentsETag == contentsHash)
              return DocumentStoreHelper.CloneObject(cachedEntityContents.EntityParts.Values.ToList());
          }

          var entityContents = GetEntityContents(web, containerTitle, entityId);

          HttpRuntime.Cache.Add(EntityContentsCachePrefix + "_" + entityId + "_" + contentsHash,
                                entityContents,
                                null,
                                Cache.NoAbsoluteExpiration,
                                CacheSlidingExpiration,
                                CacheItemPriority.Normal,
                                null);

          return DocumentStoreHelper.CloneObject(entityContents.EntityParts.Values.ToList());
        }
      }
    }

    private static EntityContents GetEntityContents(SPWeb web, string containerTitle, Guid entityId)
    {
      SPList list;
      SPFolder folder;
      if (SPDocumentStoreHelper.TryGetFolderFromPath(web, containerTitle, out list, out folder, String.Empty) ==false)
        return null;

      SPFile defaultEntityPart;
      if (SPDocumentStoreHelper.TryGetDocumentStoreDefaultEntityPart(list, folder, entityId, out defaultEntityPart) ==false)
        return null;

      EntityContents entityContents = SPDocumentStoreHelper.GetEntityContentsEntityPart(web, list, defaultEntityPart.ParentFolder);

      if (entityContents == null)
        throw new NotImplementedException(); // TODO: Build it, update the hash yada yada.

      return entityContents;
    }
  }
}
