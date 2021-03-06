﻿namespace Barista.SharePoint.Library
{
    using System.Reflection;
    using Barista.Library;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using System.Web;
    using System.Linq;
    using System.Web.Caching;

    [Serializable]
    public class BaristaSharePointGlobal : BaristaGlobal
    {
        public BaristaSharePointGlobal(ScriptEngine engine)
            : base(new BaristaGlobal(engine))
        {
            PopulateFunctions(GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        [JSFunction(Name = "include")]
        public override object Include(string scriptUrl)
        {
            var source = new SPFileScriptSource(Engine, scriptUrl);

            return Engine.Evaluate(source);
        }

        public override object Include(string scriptUrl, Jurassic.Compiler.Scope scope, object thisObject, bool strictMode)
        {
            var source = new SPFileScriptSource(Engine, scriptUrl);

            var sourceReader = source.GetReader();
            var code = sourceReader.ReadToEnd();

            return Engine.Eval(code, scope, thisObject, strictMode);
        }

        [JSFunction(Name = "isCurrentInstanceSaved")]
        public bool IsCurrentInstanceStored()
        {
            var result = false;

            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            if (instanceSettings.InstanceMode == BaristaInstanceMode.PerCall)
                return false;

            var cached = HttpRuntime.Cache.Get("Barista_ScriptEngineInstance_" + instanceSettings.InstanceName) as ScriptEngine;
            if (cached != null)
                result = true;

            return result;
        }

        [JSFunction(Name = "clearCurrentInstance")]
        public bool ClearCurrentInstance()
        {
            var result = true;
            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            switch (instanceSettings.InstanceMode)
            {
                case BaristaInstanceMode.Single:
                    result = RemoveScriptEngineInstanceFromRuntimeCache(instanceSettings.InstanceName);
                    break;
                case BaristaInstanceMode.PerSession:
                    result = RemoveScriptEngineInstanceFromRuntimeCache(instanceSettings.InstanceName);
                    break;
            }

            return result;
        }

        [JSFunction(Name = "listPackages")]
        public object ListPackages()
        {
            var objBundles = BaristaHelper.ListPackages();

            var bundlesString = objBundles.ToString();

            return JSONObject.Parse(Engine, bundlesString, null);
        }

        [JSFunction(Name = "getPackageInfo")]
        public object GetPackageInfo(string packageId)
        {
            var objBundles = BaristaHelper.GetPackageInfo(packageId);

            if (objBundles == null)
                return Undefined.Value;

            var bundlesString = objBundles.ToString();

            return JSONObject.Parse(Engine, bundlesString, null);
        }

        #region Internal Functions

        /// <summary>
        /// Returns the stored instance of the script engine, if it exists, from the runtime cache. If it does not exist, a new instance is created.
        /// </summary>
        /// <returns></returns>
        public static ScriptEngine GetOrCreateScriptEngineInstanceFromRuntimeCache(string instanceName, out bool isNewScriptEngineInstance)
        {
            if (String.IsNullOrEmpty(instanceName))
                throw new InvalidOperationException("When using Single instance mode, an instance name must be specified.");

            ScriptEngine engine = null;
            var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;
            string instanceInitializationCodeETag = null;
            isNewScriptEngineInstance = false;

            //If a instance initialization code path is defined, retrieve the etag.
            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            if (String.IsNullOrEmpty(instanceSettings.InstanceInitializationCodePath) == false)
            {
                SPHelper.TryGetSPFileETag(instanceSettings.InstanceInitializationCodePath, out instanceInitializationCodeETag);
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

                if (instanceSettings.InstanceAbsoluteExpiration.HasValue)
                    absoluteExpiration = instanceSettings.InstanceAbsoluteExpiration.Value;

                if (instanceSettings.InstanceSlidingExpiration.HasValue)
                    slidingExpiration = instanceSettings.InstanceSlidingExpiration.Value;

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

        public static bool RemoveScriptEngineInstanceFromRuntimeCache(string instanceName)
        {
            var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;

            var scriptEngine = HttpRuntime.Cache.Remove(cacheKey);
            HttpRuntime.Cache.Remove(cacheKey + "_eTag");

            return (scriptEngine != null);
        }

        public static ScriptEngine GetOrCreateScriptEngineInstanceFromSession(string instanceName, out bool isNewScriptEngineInstance)
        {
            if (String.IsNullOrEmpty(instanceName))
                throw new InvalidOperationException("When using PerSession instance mode, an instance name must be specified.");

            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                throw new InvalidOperationException("When using PerSession instance mode, a valid Http Context and Session must be available.");

            ScriptEngine engine = null;
            var cacheKey = "Barista_ScriptEngineInstance_" + instanceName;
            string instanceInitializationCodeETag = null;
            isNewScriptEngineInstance = false;

            var instanceSettings = SPBaristaContext.Current.Request.ParseInstanceSettings();

            //If a instance initialization code path is defined, retrieve the etag.
            if (String.IsNullOrEmpty(instanceSettings.InstanceInitializationCodePath) == false)
            {
                SPHelper.TryGetSPFileETag(instanceSettings.InstanceInitializationCodePath, out instanceInitializationCodeETag);
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

        public static bool RemoveScriptEngineInstanceFromSession(string instanceName)
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