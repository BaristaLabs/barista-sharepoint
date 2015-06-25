namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.SharePoint.Library;
    using Microsoft.Office.Server;
    using Microsoft.Office.Server.Search.Administration;
    using Microsoft.Office.Server.Search.Query;
    using Microsoft.SharePoint;

    [Serializable]
    public class KeywordQueryConstructor : ClrFunction
    {
        public KeywordQueryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "KeywordQuery", new KeywordQueryInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public KeywordQueryInstance Construct(object siteOrProxy)
        {
            var instance = siteOrProxy as SPSiteInstance;
            if (instance != null)
                return new KeywordQueryInstance(this.Engine, new KeywordQuery(instance.Site));

            var proxy = siteOrProxy as SearchServiceApplicationProxyInstance;
            if (proxy != null)
                return new KeywordQueryInstance(this.Engine, new KeywordQuery(proxy.SearchServiceApplicationProxy));

            var context = SPServiceContext.GetContext(SPBaristaContext.Current.Site);
            var currentProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy));
            if (currentProxy == null)
                throw new JavaScriptException(this.Engine, "Error", "Could not locate a SearchServiceApplicationProxy for the current context. Ensure that A Search Service Application has been created.");

            var searchAppProxy = currentProxy as SearchServiceApplicationProxy;
            return new KeywordQueryInstance(this.Engine, new KeywordQuery(searchAppProxy));
        }
    }

    [Serializable]
    public class KeywordQueryInstance : QueryInstance
    {
        private readonly KeywordQuery m_keywordQuery;

        public KeywordQueryInstance(ScriptEngine engine, KeywordQuery keywordQuery)
            : base(new QueryInstance(engine, keywordQuery), keywordQuery)
        {
            this.m_keywordQuery = keywordQuery;

            this.PopulateFunctions();
        }

        protected KeywordQueryInstance(ObjectInstance prototype, KeywordQuery keywordQuery)
            : base(prototype, keywordQuery)
        {
            m_keywordQuery = keywordQuery;
        }

        public KeywordQuery KeywordQuery
        {
            get
            {
                return m_keywordQuery;
            }
        }

        //CustomRefinementIntervals

        [JSProperty(Name = "enableFql")]
        public bool EnableFql
        {
            get
            {
                return m_keywordQuery.EnableFQL;
            }
            set
            {
                m_keywordQuery.EnableFQL = value;
            }
        }

        [JSProperty(Name = "enableSpellcheck")]
        public string EnableSpellcheck
        {
            get
            {
                return m_keywordQuery.EnableSpellcheck.ToString();
            }
            set
            {
                SpellcheckMode mode;
                if (value.TryParseEnum(true, out mode))
                    m_keywordQuery.EnableSpellcheck = mode;
            }
        }

        [JSProperty(Name = "enableUrlSmashing")]
        public bool EnableUrlSmashing
        {
            get
            {
                return m_keywordQuery.EnableUrlSmashing;
            }
            set
            {
                m_keywordQuery.EnableUrlSmashing = value;
            }
        }

        [JSProperty(Name = "hiddenConstraints")]
        public string HiddenConstraints
        {
            get
            {
                return m_keywordQuery.HiddenConstraints;
            }
            set
            {
                m_keywordQuery.HiddenConstraints = value;
            }
        }

        [JSProperty(Name = "maxShallowRefinementHits")]
        public int MaxShallowRefinementHits
        {
            get
            {
                return m_keywordQuery.MaxShallowRefinementHits;
            }
            set
            {
                m_keywordQuery.MaxShallowRefinementHits = value;
            }
        }

        [JSProperty(Name = "maxSummaryLength")]
        public int MaxSummaryLength
        {
            get
            {
                return m_keywordQuery.MaxSummaryLength;
            }
            set
            {
                m_keywordQuery.MaxSummaryLength = value;
            }
        }

        [JSProperty(Name = "maxUrlLength")]
        public int MaxUrlLength
        {
            get
            {
                return m_keywordQuery.MaxUrlLength;
            }
            set
            {
                m_keywordQuery.MaxUrlLength = value;
            }
        }

        //RefinementFilters

        [JSProperty(Name = "refiners")]
        public string Refiners
        {
            get
            {
                return m_keywordQuery.Refiners;
            }
            set
            {
                m_keywordQuery.Refiners = value;
            }
        }

        [JSProperty(Name = "resubmitFlags")]
        public string ResubmitFlags
        {
            get
            {
                return m_keywordQuery.ResubmitFlags.ToString();
            }
            set
            {
                ResubmitFlag flag;
                if (value.TryParseEnum(true, out flag))
                    m_keywordQuery.ResubmitFlags = flag;
            }
        }

        [JSProperty(Name = "searchTerms")]
        public string SearchTerms
        {
            get
            {
                return m_keywordQuery.SearchTerms;
            }
        }

        //SelectProperties

        [JSProperty(Name = "similarTo")]
        public string SimilarTo
        {
            get
            {
                return m_keywordQuery.SimilarTo;
            }
            set
            {
                m_keywordQuery.SimilarTo = value;
            }
        }

        [JSProperty(Name = "similarType")]
        public string SimilarType
        {
            get
            {
                return m_keywordQuery.SimilarType.ToString();
            }
            set
            {
                SimilarType type;
                if (value.TryParseEnum(true, out type))
                    m_keywordQuery.SimilarType = type;
            }
        }

        //SortList

        [JSProperty(Name = "sortSimilar")]
        public bool SortSimilar
        {
            get
            {
                return m_keywordQuery.SortSimilar;
            }
            set
            {
                m_keywordQuery.SortSimilar = value;
            }
        }

        //TimeZone

        [JSProperty(Name = "trimDuplicatesIncludeId")]
        public double TrimDuplicatesIncludeId
        {
            get
            {
                return m_keywordQuery.TrimDuplicatesIncludeId;
            }
            set
            {
                m_keywordQuery.TrimDuplicatesIncludeId = (long)value;
            }
        }

        [JSProperty(Name = "trimDuplicatesKeepCount")]
        public int TrimDuplicatesKeepCount
        {
            get
            {
                return m_keywordQuery.TrimDuplicatesKeepCount;
            }
            set
            {
                m_keywordQuery.TrimDuplicatesKeepCount = value;
            }
        }

        [JSProperty(Name = "trimDuplicatesOnProperty")]
        public string TrimDuplicatesOnProperty
        {
            get
            {
                return m_keywordQuery.TrimDuplicatesOnProperty;
            }
            set
            {
                m_keywordQuery.TrimDuplicatesOnProperty = value;
            }
        }

        [JSProperty(Name = "userContextData")]
        public string UserContextData
        {
            get
            {
                return m_keywordQuery.UserContextData;
            }
            set
            {
                m_keywordQuery.UserContextData = value;
            }
        }

        [JSProperty(Name = "userContextGroupId")]
        public string UserContextGroupId
        {
            get
            {
                return m_keywordQuery.UserContextGroupID;
            }
            set
            {
                m_keywordQuery.UserContextGroupID = value;
            }
        }
    }
}
