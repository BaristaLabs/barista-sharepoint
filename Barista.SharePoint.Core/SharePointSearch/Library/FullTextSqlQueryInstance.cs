namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.SharePoint.Library;
    using Microsoft.Office.Server;
    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.Office.Server.Search.Query;
    using Microsoft.SharePoint;

    [Serializable]
    public class FullTextSqlQueryConstructor : ClrFunction
    {
        public FullTextSqlQueryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "FullTextSqlQuery", new FullTextSqlQueryInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public FullTextSqlQueryInstance Construct(object siteOrProxy)
        {
            var instance = siteOrProxy as SPSiteInstance;
            if (instance != null)
                return new FullTextSqlQueryInstance(this.Engine, new FullTextSqlQuery(instance.Site));

            var proxy = siteOrProxy as SearchServiceApplicationProxyInstance;
            if (proxy != null)
                return new FullTextSqlQueryInstance(this.Engine, new FullTextSqlQuery(proxy.SearchServiceApplicationProxy));

            var context = SPServiceContext.GetContext(SPBaristaContext.Current.Site);
            var currentProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy));
            if (currentProxy == null)
                throw new JavaScriptException(this.Engine, "Error", "Could not locate a SearchServiceApplicationProxy for the current context. Ensure that A Search Service Application has been created.");

            var searchAppProxy = currentProxy as SearchServiceApplicationProxy;
            return new FullTextSqlQueryInstance(this.Engine, new FullTextSqlQuery(searchAppProxy));
        }
    }

    [Serializable]
    public class FullTextSqlQueryInstance : QueryInstance
    {
        private readonly FullTextSqlQuery m_fullTextSqlQuery;

        public FullTextSqlQueryInstance(ScriptEngine engine, FullTextSqlQuery fullTextSqlQuery)
            : base(new QueryInstance(engine, fullTextSqlQuery), fullTextSqlQuery)
        {
            m_fullTextSqlQuery = fullTextSqlQuery;

            this.PopulateFunctions();
        }

        public FullTextSqlQueryInstance(ObjectInstance prototype, FullTextSqlQuery fullTextSqlQuery)
            : base(prototype, fullTextSqlQuery)
        {
            m_fullTextSqlQuery = fullTextSqlQuery;
        }

        public FullTextSqlQuery FullTextSqlQuery
        {
            get
            {
                return m_fullTextSqlQuery;
            }
        }

        //Everything is inherited from QueryInstance!
    }
}
