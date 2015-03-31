namespace Barista.SharePoint
{
    using System;
    //using V8.Net;
    public class SPBaristaV8ScriptEngineFactory : ScriptEngineFactory
    {
        public override IScriptEngine GetScriptEngine(Barista.Bundles.WebBundleBase webBundle, out bool isNewScriptEngineInstance, out bool errorInInitialization)
        {
            //var v8Engine = new V8Engine();
            throw new NotImplementedException();
        }
    }
}
