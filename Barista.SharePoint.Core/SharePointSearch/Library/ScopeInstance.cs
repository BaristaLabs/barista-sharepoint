namespace Barista.SharePoint.SharePointSearch.Library
{
    using System.Linq;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.SharePoint.Library;
    using Microsoft.Office.Server.Search.Administration;

    [Serializable]
    public class ScopeInstance : ObjectInstance
    {
        private readonly Scope m_scope;

        public ScopeInstance(ScriptEngine engine, Scope scope)
            : base(engine)
        {
            m_scope = scope;
            this.PopulateFunctions();
        }

        protected ScopeInstance(ObjectInstance prototype, Scope scope)
            : base(prototype)
        {
            m_scope = scope;
        }

        public Scope Scope
        {
            get
            {
                return m_scope;
            }
        }

        [JSProperty(Name = "alternateResultsPage")]
        public string AlternateResultsPage
        {
            get
            {
                return m_scope.AlternateResultsPage;
            }
            set
            {
                m_scope.AlternateResultsPage = value;
            }
        }

        [JSProperty(Name = "canBeDeleted")]
        public bool CanBeDeleted
        {
            get
            {
                return m_scope.CanBeDeleted;
            }
        }

        [JSProperty(Name = "compilationState")]
        public string CompilationState
        {
            get
            {
                return m_scope.CompilationState.ToString();
            }
        }

        [JSProperty(Name = "compilationType")]
        public string CompilationType
        {
            get
            {
                return m_scope.CompilationType.ToString();
            }
            set
            {
                ScopeCompilationType type;
                if (value.TryParseEnum(true, out type))
                    m_scope.CompilationType = type;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_scope.Count;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_scope.Description;
            }
            set
            {
                m_scope.Description = value;
            }
        }

        [JSProperty(Name = "displayInAdminUI")]
        public bool DisplayInAdminUI
        {
            get
            {
                return m_scope.DisplayInAdminUI;
            }
            set
            {
                m_scope.DisplayInAdminUI = value;
            }
        }

        [JSProperty(Name = "filter")]
        public string Filter
        {
            get
            {
                return m_scope.Filter;
            }
            set
            {
                m_scope.Filter = value;
            }
        }

        [JSProperty(Name = "id")]
        public int Id
        {
            get
            {
                return m_scope.ID;
            }
        }

        [JSProperty(Name = "lastCompilationTime")]
        public DateInstance LastCompilationTime
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_scope.LastCompilationTime);
            }
        }

        [JSProperty(Name = "lastModifiedBy")]
        public string LastModifiedBy
        {
            get
            {
                return m_scope.LastModifiedBy;
            }
        }

        [JSProperty(Name = "lastModifiedTime")]
        public DateInstance LastModifiedTime
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_scope.LastModifiedTime);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_scope.Name;
            }
        }

        [JSProperty(Name = "owningSite")]
        public SPSiteInstance OwningSite
        {
            get
            {
                var site = m_scope.OwningSite;
                return site == null
                    ? null
                    : new SPSiteInstance(this.Engine.Object.InstancePrototype, site);
            }
        }

        //Rules

        [JSFunction(Name = "getContainingDisplayGroups")]
        public ArrayInstance GetContainingDisplayGroups()
        {
            var groups = m_scope.ContainingDisplayGroups;
            if (groups == null)
                return null;

            var result = this.Engine.Array.Construct();
            foreach (var g in groups.OfType<ScopeDisplayGroup>())
                ArrayInstance.Push(result, new ScopeDisplayGroupInstance(this.Engine, g));
            
            return result;
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_scope.Delete();
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_scope.Update();
        }
    }
}
