namespace Barista.SharePoint.SharePointSearch.Library
{
    using System.Linq;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class RemoteScopesConstructor : ClrFunction
    {
        public RemoteScopesConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "RemoteScopes", new RemoteScopesInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public RemoteScopesInstance Construct()
        {
            var context = SPServiceContext.GetContext(SPBaristaContext.Current.Site);
            return new RemoteScopesInstance(Engine, new RemoteScopes(context));
        }
    }

    [Serializable]
    public class RemoteScopesInstance : ObjectInstance
    {
        private readonly RemoteScopes m_remoteScopes;

        public RemoteScopesInstance(ScriptEngine engine, RemoteScopes remoteScopes)
            : base(engine)
        {
            m_remoteScopes = remoteScopes;

            PopulateFunctions();
        }

        protected RemoteScopesInstance(ObjectInstance prototype, RemoteScopes remoteScopes)
            : base(prototype)
        {
            m_remoteScopes = remoteScopes;
        }

        public RemoteScopes RemoteScopes
        {
            get
            {
                return m_remoteScopes;
            }
        }

        [JSProperty(Name = "allDisplayGroups")]
        public ScopeDisplayGroupCollectionInstance AllDisplayGroups
        {
            get
            {
                return m_remoteScopes.AllDisplayGroups == null
                    ? null
                    : new ScopeDisplayGroupCollectionInstance(Engine, m_remoteScopes.AllDisplayGroups);
            }
        }

        [JSProperty(Name = "allScopes")]
        public ScopeCollectionInstance AllScopes
        {
            get
            {
                return m_remoteScopes.AllScopes == null
                    ? null
                    : new ScopeCollectionInstance(Engine, m_remoteScopes.AllScopes);
            }
        }

        [JSProperty(Name = "compilationType")]
        public string CompilationType
        {
            get
            {
                return m_remoteScopes.CompilationScheduleType.ToString();
            }
            set
            {
                ScopesCompilationScheduleType type;
                if (value.TryParseEnum(true, out type))
                    m_remoteScopes.CompilationScheduleType = type;
            }
        }


        [JSFunction(Name = "getDisplayGroup")]
        public ScopeDisplayGroupInstance GetDisplayGroup(UriInstance siteUrl, string name)
        {
            if (siteUrl == null)
                throw new JavaScriptException(Engine, "Error", "SiteUrl argument must be specified and an instance of UriInstance");

            var result = m_remoteScopes.GetDisplayGroup(siteUrl.Uri, name);

            return result == null
                ? null
                : new ScopeDisplayGroupInstance(Engine, result);
        }

        [JSFunction(Name = "getDisplayGroupsForSite")]
        [JSDoc("ternReturnType", "[+ScopeDisplayGroup]")]
        public ArrayInstance GetDisplayGroupsForSite(UriInstance siteUrl)
        {
            if (siteUrl == null)
                throw new JavaScriptException(Engine, "Error", "SiteUrl argument must be specified and an instance of UriInstance");

            var groups = m_remoteScopes.GetDisplayGroupsForSite(siteUrl.Uri);

            if (groups == null)
                return null;

            var result = Engine.Array.Construct();
            foreach (var dg in groups.OfType<ScopeDisplayGroup>())
                ArrayInstance.Push(result, new ScopeDisplayGroupInstance(Engine, dg));

            return result;
        }

        [JSFunction(Name = "getScope")]
        public ScopeInstance GetScope(UriInstance siteUrl, string name)
        {
            if (siteUrl == null)
                throw new JavaScriptException(Engine, "Error", "SiteUrl argument must be specified and an instance of UriInstance");

            var result = m_remoteScopes.GetScope(siteUrl.Uri, name);

            return result == null
                ? null
                : new ScopeInstance(Engine, result);
        }

        [JSFunction(Name = "getScopesForSite")]
        [JSDoc("ternReturnType", "[+Scope]")]
        public ArrayInstance GetScopesForSite(UriInstance siteUrl)
        {
            if (siteUrl == null)
                throw new JavaScriptException(Engine, "Error", "SiteUrl argument must be specified and an instance of UriInstance");

            var scopes = m_remoteScopes.GetScopesForSite(siteUrl.Uri);

            if (scopes == null)
                return null;

            var result = Engine.Array.Construct();
            foreach (var scope in scopes.OfType<Scope>())
                ArrayInstance.Push(result, new ScopeInstance(Engine, scope));

            return result;
        }

        [JSFunction(Name = "getSharedScope")]
        public ScopeInstance GetSharedScope(string name)
        {
            var result = m_remoteScopes.GetSharedScope(name);

            return result == null
                ? null
                : new ScopeInstance(Engine, result);
        }

        [JSFunction(Name = "getSharedScopes")]
        [JSDoc("ternReturnType", "[+Scope]")]
        public ArrayInstance GetSharedScopes()
        {
            var scopes = m_remoteScopes.GetSharedScopes();

            if (scopes == null)
                return null;

            var result = Engine.Array.Construct();
            foreach (var scope in scopes.OfType<Scope>())
                ArrayInstance.Push(result, new ScopeInstance(Engine, scope));

            return result;
        }

        [JSFunction(Name = "getUnusedScopesForSite")]
        [JSDoc("ternReturnType", "[+Scope]")]
        public ArrayInstance GetUnusedScopesForSite(UriInstance siteUrl)
        {
            if (siteUrl == null)
                throw new JavaScriptException(Engine, "Error", "SiteUrl argument must be specified and an instance of UriInstance");

            var scopes = m_remoteScopes.GetUnusedScopesForSite(siteUrl.Uri);

            if (scopes == null)
                return null;

            var result = Engine.Array.Construct();
            foreach (var scope in scopes.OfType<Scope>())
                ArrayInstance.Push(result, new ScopeInstance(Engine, scope));

            return result;
        }

        [JSFunction(Name = "startCompilation")]
        public void StartCompilation()
        {
            m_remoteScopes.StartCompilation();
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_remoteScopes.Update();
        }
    }
}
