namespace Barista.SharePoint.SharePointSearch.Library
{
    using System.Linq;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.Office.Server.Search.Administration;

    [Serializable]
    public class ScopeCollectionInstance : ObjectInstance
    {
        private readonly ScopeCollection m_scopeCollection;

        public ScopeCollectionInstance(ScriptEngine engine, ScopeCollection scopeCollection)
            : base(engine)
        {
            m_scopeCollection = scopeCollection;

            PopulateFunctions();
        }

        protected ScopeCollectionInstance(ObjectInstance prototype, ScopeCollection scopeCollection)
            : base(prototype)
        {
            if (scopeCollection == null)
                throw new ArgumentNullException("scopeCollection");

            m_scopeCollection = scopeCollection;
        }

        public ScopeCollection ScopeCollection
        {
            get
            {
                return m_scopeCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_scopeCollection.Count;
            }
        }

        [JSFunction(Name = "getScopeByIndex")]
        public ScopeInstance GetScopeByIndex(int index)
        {
            var result = m_scopeCollection[index];
            return result == null
                ? null
                : new ScopeInstance(Engine, result);
        }

        [JSFunction(Name = "create")]
        public ScopeInstance Create(string name, string description, UriInstance owningSiteUrl, bool displayInAdminUI, string scopeCompilationType, string alternateResultsPage, string filter)
        {
            ScopeCompilationType type;
            scopeCompilationType.TryParseEnum(true, ScopeCompilationType.AlwaysCompile, out type);

            var result = m_scopeCollection.Create(name, description, owningSiteUrl.Uri, displayInAdminUI, alternateResultsPage, type, filter);
            return result == null
                ? null
                : new ScopeInstance(Engine, result);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+Scope]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var scope in m_scopeCollection.OfType<Scope>())
                ArrayInstance.Push(result, new ScopeInstance(Engine, scope));

            return result;
        }
    }
}
