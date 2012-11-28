namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Web;
  using System.Linq;
  using System.Web.Caching;

  [Serializable]
  public class BaristaSharePointGlobal : ObjectInstance
  {
    public BaristaSharePointGlobal(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "include")]
    public void Include(string scriptUrl)
    {
      var source = new SPFileScriptSource(this.Engine, scriptUrl);

      this.Engine.Execute(source);
    }

    [JSFunction(Name = "isCurrentInstanceSaved")]
    public static bool IsCurrentInstanceStored()
    {
      bool result = false;

      if (BaristaContext.Current.Request.InstanceMode == BaristaInstanceMode.PerCall)
        return false;

      var cached = HttpRuntime.Cache.Get("Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceName) as ScriptEngine;
      if (cached != null)
        result = true;

      return result;
    }

    [JSFunction(Name = "clearCurrentInstance")]
    public static bool ClearCurrentInstance()
    {
      bool result = true;
      switch (BaristaContext.Current.Request.InstanceMode)
      {
        case BaristaInstanceMode.Single:
          result = BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
          break;
        case BaristaInstanceMode.PerSession:
          result = BaristaSharePointGlobal.RemoveScriptEngineInstanceFromRuntimeCache(BaristaContext.Current.Request.InstanceName);
          break;
      }

      return result;
    }

    #region Internal Functions

    /// <summary>
    /// Returns the stored instance of the script engine, if it exists, from the runtime cache. If it does not exist, a new instance is created.
    /// </summary>
    /// <returns></returns>
    internal static ScriptEngine GetOrCreateScriptEngineInstanceFromRuntimeCache(string instanceName, out bool isNewScriptEngineInstance)
    {
      if (String.IsNullOrEmpty(instanceName))
        throw new InvalidOperationException("When using Single instance mode, an instance name must be specified.");

      ScriptEngine engine = null;
      string cacheKey = "Barista_ScriptEngineInstance_" + instanceName;
      string instanceInitializationCodeETag = null;
      isNewScriptEngineInstance = false;

      //If a instance initialization code path is defined, retrieve the etag.
      if (String.IsNullOrEmpty(BaristaContext.Current.Request.InstanceInitializationCodePath) == false)
      {
        SPHelper.TryGetSPFileETag(BaristaContext.Current.Request.InstanceInitializationCodePath, out instanceInitializationCodeETag);
      }

      //If the eTag of the initialization script differs, recreate the instance.
      var eTagWhenCreated = HttpRuntime.Cache.Get(cacheKey + "_eTag") as string;

      if (String.IsNullOrEmpty(eTagWhenCreated) == false && eTagWhenCreated != instanceInitializationCodeETag)
      {
        HttpRuntime.Cache.Remove(cacheKey);
        HttpRuntime.Cache.Remove(cacheKey + "_eTag");
      }
      else
      {
        engine = HttpRuntime.Cache.Get(cacheKey) as ScriptEngine;
      }

      if (engine == null)
      {
        var absoluteExpiration = Cache.NoAbsoluteExpiration;
        var slidingExpiration = Cache.NoSlidingExpiration;

        if (BaristaContext.Current.Request.InstanceAbsoluteExpiration.HasValue)
          absoluteExpiration = BaristaContext.Current.Request.InstanceAbsoluteExpiration.Value;

        if (BaristaContext.Current.Request.InstanceSlidingExpiration.HasValue)
          slidingExpiration = BaristaContext.Current.Request.InstanceSlidingExpiration.Value;

        engine = new ScriptEngine();
        isNewScriptEngineInstance = true;
        HttpRuntime.Cache.Add(cacheKey, engine, null, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
        if (instanceInitializationCodeETag != null)
        {
          HttpRuntime.Cache.Add(cacheKey + "_eTag", instanceInitializationCodeETag, null, absoluteExpiration,
                                slidingExpiration, CacheItemPriority.Normal, null);
        }
      }

      return engine;
    }

    internal static bool RemoveScriptEngineInstanceFromRuntimeCache(string instanceName)
    {
      var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;

      var scriptEngine = HttpRuntime.Cache.Remove(cacheKey);
      HttpRuntime.Cache.Remove(cacheKey + "_eTag");

      return (scriptEngine != null);
    }

    internal static ScriptEngine GetOrCreateScriptEngineInstanceFromSession(string instanceName, out bool isNewScriptEngineInstance)
    {
      if (String.IsNullOrEmpty(instanceName))
        throw new InvalidOperationException("When using PerSession instance mode, an instance name must be specified.");

      if (HttpContext.Current == null || HttpContext.Current.Session == null)
        throw new InvalidOperationException("When using PerSession instance mode, a valid Http Context and Session must be available.");

      ScriptEngine engine = null;
      var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;
      string instanceInitializationCodeETag = null;
      isNewScriptEngineInstance = false;

      //If a instance initialization code path is defined, retrieve the etag.
      if (String.IsNullOrEmpty(BaristaContext.Current.Request.InstanceInitializationCodePath) == false)
      {
        SPHelper.TryGetSPFileETag(BaristaContext.Current.Request.InstanceInitializationCodePath, out instanceInitializationCodeETag);
      }

      //If the eTag of the initialization script differs, recreate the instance.
      string eTagWhenCreated = null;
      if (HttpContext.Current.Session.Keys.OfType<string>().Any(k => k == (cacheKey + "_eTag")))
      {
        eTagWhenCreated = HttpContext.Current.Session[cacheKey + "_eTag"] as string;
      }

      if (String.IsNullOrEmpty(eTagWhenCreated) == false && eTagWhenCreated != instanceInitializationCodeETag)
      {
        HttpContext.Current.Session.Remove(cacheKey);
        HttpContext.Current.Session.Remove(cacheKey + "_eTag");
      }
      else
      {
        if (HttpContext.Current.Session.Keys.OfType<string>().Any(k => k == cacheKey))
        {
          engine = HttpContext.Current.Session[cacheKey] as ScriptEngine;
        }
      }

      if (engine == null)
      {
        engine = new ScriptEngine();
        isNewScriptEngineInstance = true;
        HttpContext.Current.Session.Add(cacheKey, engine);
        HttpContext.Current.Session.Add(cacheKey + "_eTag", instanceInitializationCodeETag);
      }

      return engine;
    }

    internal static bool RemoveScriptEngineInstanceFromSession(string instanceName)
    {
      if (HttpContext.Current == null || HttpContext.Current.Session == null)
        throw new InvalidOperationException("When using PerSession instance mode, a valid Http Context and Session must be available.");

      var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;

      var result = HttpContext.Current.Session.Keys.OfType<string>().Any(k => k == cacheKey);

      HttpContext.Current.Session.Remove(cacheKey);
      HttpContext.Current.Session.Remove(cacheKey + "_eTag");
      return result;
    }
    #endregion
  }
}
