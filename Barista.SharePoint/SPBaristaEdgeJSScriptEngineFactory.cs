namespace Barista.SharePoint
{
    using Barista.Bundles;
    using Barista.Engine;
    using System;

    public class SPBaristaEdgeJSScriptEngineFactory : ScriptEngineFactory
    {
        /// <summary>
        /// Returns a new instance of an EdgeJS Script Engine Object with all runtime objects available.
        /// </summary>
        /// <returns></returns>
        public override IScriptEngine GetScriptEngine(WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
        {
            isNewScriptEngineInstance = true;
            errorInInitialization = false;

            var engine = new EdgeJSScriptEngine();

            return engine;
        }
    }
}
