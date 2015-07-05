namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.Office.Server.Search.Administration;
    using System;
    using System.Linq;

    [Serializable]
    public class ScopeDisplayGroupCollectionInstance : ObjectInstance
    {
        private readonly ScopeDisplayGroupCollection m_scopeDisplayGroupCollection;

        public ScopeDisplayGroupCollectionInstance(ScriptEngine engine, ScopeDisplayGroupCollection scopeDisplayGroupCollection)
            : base(engine)
        {
            m_scopeDisplayGroupCollection = scopeDisplayGroupCollection;

            PopulateFunctions();
        }

        protected ScopeDisplayGroupCollectionInstance(ObjectInstance prototype, ScopeDisplayGroupCollection scopeDisplayGroupCollection)
            : base(prototype)
        {
            m_scopeDisplayGroupCollection = scopeDisplayGroupCollection;
        }

        public ScopeDisplayGroupCollection ScopeDisplayGroupCollection
        {
            get
            {
                return m_scopeDisplayGroupCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_scopeDisplayGroupCollection.Count;
            }
        }

        [JSFunction(Name = "getScopeDisplayGroupByIndex")]
        public ScopeDisplayGroupInstance GetScopeDisplayGroupByIndex(int index)
        {
            var result = m_scopeDisplayGroupCollection[index];
            return result == null
                ? null
                : new ScopeDisplayGroupInstance(Engine, result);
        }

        [JSFunction(Name = "create")]
        public ScopeDisplayGroupInstance Create(string name, string description, UriInstance owningSiteUrl, bool displayInAdminUI)
        {
            var result = m_scopeDisplayGroupCollection.Create(name, description, owningSiteUrl.Uri, displayInAdminUI);
            return result == null
                ? null
                : new ScopeDisplayGroupInstance(Engine, result);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+ScopeDisplayGroup]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var scope in m_scopeDisplayGroupCollection.OfType<ScopeDisplayGroup>())
                ArrayInstance.Push(result, new ScopeDisplayGroupInstance(Engine, scope));

            return result;
        }
    }
}
