namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.Office.Server.Search.Administration;
    using System;

    [Serializable]
    public class SearchServiceApplicationConstructor : ClrFunction
    {
        public SearchServiceApplicationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SearchServiceApplication", new SearchServiceApplicationInstance(engine, null))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public SearchServiceApplicationInstance Construct()
        {
            return new SearchServiceApplicationInstance(this.Engine, new SearchServiceApplication());
        }
    }

    [Serializable]
    public class SearchServiceApplicationInstance : ObjectInstance
    {
        private readonly SearchServiceApplication m_searchServiceApplication;

        public SearchServiceApplicationInstance(ScriptEngine engine, SearchServiceApplication searchServiceApplication)
            : base(engine)
        {
            m_searchServiceApplication = searchServiceApplication;
            this.PopulateFunctions();
        }

        protected SearchServiceApplicationInstance(ObjectInstance prototype, SearchServiceApplication searchServiceApplication)
            : base(prototype)
        {
            m_searchServiceApplication = searchServiceApplication;
        }

        public SearchServiceApplication SearchServiceApplication
        {
            get { return m_searchServiceApplication; }
        }
    }
}
