namespace Barista.SharePoint
{
    using Barista.Bundles;
    using Barista.Engine;
    using Barista.V8.Net;
    using System;

    public class SPBaristaV8ScriptEngineFactory : ScriptEngineFactory
    {
        /// <summary>
        /// Returns a new instance of a V8 Script Engine Object with all runtime objects available.
        /// </summary>
        /// <returns></returns>
        public override IScriptEngine GetScriptEngine(WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
        {
            isNewScriptEngineInstance = true;
            errorInInitialization = false;

            AppDomain.CurrentDomain.AssemblyResolve += V8Engine.Resolver;

            var engine = new V8Engine();

            
            return engine;
        }
    }
}
