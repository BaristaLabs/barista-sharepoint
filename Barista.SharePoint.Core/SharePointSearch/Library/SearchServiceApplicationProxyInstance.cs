namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.SharePoint;

    [Serializable]
    public class SearchServiceApplicationProxyConstructor : ClrFunction
    {
        public SearchServiceApplicationProxyConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SearchServiceApplicationProxy", new SearchServiceApplicationProxyInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SearchServiceApplicationProxyInstance Construct()
        {
            var context = SPServiceContext.GetContext(SPBaristaContext.Current.Site);
            var proxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy));
            if (proxy == null)
                throw new JavaScriptException(this.Engine, "Error", "Could not locate a SearchServiceApplicationProxy for the current context. Ensure that A Search Service Application has been created.");

            var searchAppProxy = proxy as SearchServiceApplicationProxy;
            return new SearchServiceApplicationProxyInstance(this.Engine, searchAppProxy);
        }
    }

    [Serializable]
    public class SearchServiceApplicationProxyInstance : ObjectInstance
    {
        private readonly SearchServiceApplicationProxy m_searchServiceApplicationProxy;

        public SearchServiceApplicationProxyInstance(ScriptEngine engine, SearchServiceApplicationProxy searchServiceApplicationProxy)
            : base(engine)
        {
            m_searchServiceApplicationProxy = searchServiceApplicationProxy;
            this.PopulateFunctions();
        }

        protected SearchServiceApplicationProxyInstance(ObjectInstance prototype, SearchServiceApplicationProxy searchServiceApplicationProxy)
            : base(prototype)
        {
            m_searchServiceApplicationProxy = searchServiceApplicationProxy;
        }

        public SearchServiceApplicationProxy SearchServiceApplicationProxy
        {
            get
            {
                return m_searchServiceApplicationProxy;
            }
        }
    }
}
