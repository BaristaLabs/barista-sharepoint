namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.Office.Server.Search.Query;

    [Serializable]
    public class ScopeInformationInstance : ObjectInstance
    {
        private readonly ScopeInformation m_scopeInformation;

        public ScopeInformationInstance(ScriptEngine engine, ScopeInformation scopeInformation)
            : base(engine)
        {
            if (scopeInformation == null)
                throw new ArgumentNullException("scopeInformation");

            m_scopeInformation = scopeInformation;

            this.PopulateFunctions();
        }

        public ScopeInformation ScopeInformation
        {
            get
            {
                return m_scopeInformation;
            }
        }

        [JSProperty(Name = "alternateResultsPage")]
        public string AlternateResultsPage
        {
            get
            {
                return m_scopeInformation.AlternateResultsPage;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_scopeInformation.Description;
            }
        }

        [JSProperty(Name = "filter")]
        public string Filter
        {
            get
            {
                return m_scopeInformation.Filter;
            }
        }

        [JSProperty(Name = "isReady")]
        public bool IsReady
        {
            get
            {
                return m_scopeInformation.IsReady;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_scopeInformation.Name;
            }
        }
    }
}
