namespace Barista.SharePoint.SharePointSearch.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.SharePoint.Library;
    using Microsoft.Office.Server.Search.Administration;
    
    [Serializable]
    public class ScopeDisplayGroupInstance : ObjectInstance
    {
        private readonly ScopeDisplayGroup m_scopeDisplayGroup;

        public ScopeDisplayGroupInstance(ScriptEngine engine, ScopeDisplayGroup scopeDisplayGroup)
            : base(engine)
        {
            m_scopeDisplayGroup = scopeDisplayGroup;
            PopulateFunctions();
        }

        protected ScopeDisplayGroupInstance(ObjectInstance prototype, ScopeDisplayGroup scopeDisplayGroup)
            : base(prototype)
        {
            m_scopeDisplayGroup = scopeDisplayGroup;
        }

        public ScopeDisplayGroup ScopeDisplayGroup
        {
            get
            {
                return m_scopeDisplayGroup;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_scopeDisplayGroup.Count;
            }
        }

        //Default

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_scopeDisplayGroup.Description;
            }
            set
            {
                m_scopeDisplayGroup.Description = value;
            }
        }

        [JSProperty(Name = "displayInAdminUI")]
        public bool DisplayInAdminUI
        {
            get
            {
                return m_scopeDisplayGroup.DisplayInAdminUI;
            }
            set
            {
                m_scopeDisplayGroup.DisplayInAdminUI = value;
            }
        }

        [JSProperty(Name = "id")]
        public int Id
        {
            get
            {
                return m_scopeDisplayGroup.ID;
            }
        }

        [JSProperty(Name = "lastModifiedBy")]
        public string LastModifiedBy
        {
            get
            {
                return m_scopeDisplayGroup.LastModifiedBy;
            }
        }

        [JSProperty(Name = "lastModifiedTime")]
        public DateInstance LastModifiedTime
        {
            get
            {
                return JurassicHelper.ToDateInstance(Engine, m_scopeDisplayGroup.LastModifiedTime);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_scopeDisplayGroup.Name;
            }
            set
            {
                m_scopeDisplayGroup.Name = value;
            }
        }

        [JSProperty(Name = "owningSite")]
        public SPSiteInstance OwningSite
        {
            get
            {
                var site = m_scopeDisplayGroup.OwningSite;
                return site == null
                    ? null
                    : new SPSiteInstance(Engine.Object.InstancePrototype, site);
            }
        }

        [JSFunction(Name = "add")]
        public void Add(ScopeInstance scope)
        {
            if (scope == null)
                throw new JavaScriptException(Engine, "Error", "Scope must be specified.");

            m_scopeDisplayGroup.Add(scope.Scope);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_scopeDisplayGroup.Clear();
        }

        [JSFunction(Name = "contains")]
        public bool Contains(ScopeInstance scope)
        {
            if (scope == null)
                throw new JavaScriptException(Engine, "Error", "Scope must be specified.");

            return m_scopeDisplayGroup.Contains(scope.Scope);
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_scopeDisplayGroup.Delete();
        }

        [JSFunction(Name = "indexOf")]
        public int IndexOf(ScopeInstance scope)
        {
            if (scope == null)
                throw new JavaScriptException(Engine, "Error", "Scope must be specified.");

            return m_scopeDisplayGroup.IndexOf(scope.Scope);
        }

        [JSFunction(Name = "insert")]
        public void Insert(int index, ScopeInstance scope)
        {
            if (scope == null)
                throw new JavaScriptException(Engine, "Error", "Scope must be specified.");

            m_scopeDisplayGroup.Insert(index, scope.Scope);
        }

        [JSFunction(Name = "remove")]
        public void Remove(ScopeInstance scope)
        {
            if (scope == null)
                throw new JavaScriptException(Engine, "Error", "Scope must be specified.");

            m_scopeDisplayGroup.Remove(scope.Scope);
        }

        [JSFunction(Name = "removeAt")]
        public void RemoveAt(int index)
        {
            m_scopeDisplayGroup.RemoveAt(index);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_scopeDisplayGroup.Update();
        }

        [JSFunction(Name = "getDefaultScope")]
        public ScopeInstance GetDefaultScope()
        {
            var scope = m_scopeDisplayGroup.Default;
            return scope == null
                ? null
                : new ScopeInstance(Engine, scope);
        }

        [JSFunction(Name = "getScopeByIndex")]
        public ScopeInstance GetScopeByIndex(int index)
        {
            var scope = m_scopeDisplayGroup[index];
            return scope == null
                ? null
                : new ScopeInstance(Engine, scope);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+Scope]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var scope in m_scopeDisplayGroup.OfType<Scope>())
                ArrayInstance.Push(result, new ScopeInstance(Engine, scope));
            return result;
        }
    }
}
