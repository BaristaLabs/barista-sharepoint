namespace Barista.SharePoint.Bundles
{
    using Barista.SharePoint.SharePointSearch.Library;
    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SharePointSearchBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "SharePoint Search"; }
        }

        public string BundleDescription
        {
            get { return "SharePoint Search Bundle. Provides top-level objects to interact with SharePoint Search"; }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            //engine.SetGlobalValue("SearchServiceApplication", new SearchServiceApplicationConstructor(engine));
            engine.SetGlobalValue("SearchServiceApplicationProxy", new SearchServiceApplicationProxyConstructor(engine));
            engine.SetGlobalValue("KeywordQuery", new KeywordQueryConstructor(engine));
            engine.SetGlobalValue("FullTextSqlQuery", new FullTextSqlQueryConstructor(engine));
            engine.SetGlobalValue("RemoteScopes", new RemoteScopesConstructor(engine));

            var context = SPServiceContext.GetContext(SPBaristaContext.Current.Site);
            var proxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy));
            var searchAppProxy = proxy as SearchServiceApplicationProxy;
            return searchAppProxy == null
                ? null
                : new SearchServiceApplicationProxyInstance(engine, searchAppProxy);
        }
    }
}
