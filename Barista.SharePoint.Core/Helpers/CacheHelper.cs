namespace Barista.SharePoint
{
  using System;
  using System.Web;
  using System.Web.Caching;

  public class CacheHelper
  {
    public static void AddToCache(int defaultCacheTime, string propertyBagKey, string cacheKey, object itemToCache)
    {
      int cacheTime = defaultCacheTime;
      string propertyBagValue = CommonPropertyUtil.Load(propertyBagKey);
      if (String.IsNullOrEmpty(propertyBagValue) == false)
        int.TryParse(propertyBagValue, out cacheTime);

      HttpRuntime.Cache.Add(cacheKey, itemToCache, null, DateTime.Now.AddMinutes(cacheTime), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
     
    }

    public static T GetFromCacheOrInitialize<T>(int defaultCacheTime, string propertyBagKey, string cacheKey, Func<T> initialize)
      where T : class
    {
      T cachedItem = GetFromCache<T>(cacheKey);
      if (cachedItem != null)
        return cachedItem;

      T itemToCache = initialize();
      AddToCache(defaultCacheTime, propertyBagKey, cacheKey, itemToCache);
      return itemToCache;
    }

    public static T GetFromCache<T>(string cacheKey)
      where T : class
    {
      T cachedItem = HttpRuntime.Cache.Get(cacheKey) as T;
      if (cachedItem != null)
        return cachedItem;
      return null;
    }
  }
}
