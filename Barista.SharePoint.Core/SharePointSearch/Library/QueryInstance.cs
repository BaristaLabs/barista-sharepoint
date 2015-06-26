namespace Barista.SharePoint.SharePointSearch.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.Office.Server.Search.Query;
    using System;

    [Serializable]
    public class QueryInstance : ObjectInstance
    {
        private readonly Query m_query;

        public QueryInstance(ScriptEngine engine, Query query)
            : base(engine)
        {
            m_query = query;

            PopulateFunctions();
        }

        protected QueryInstance(ObjectInstance prototype, Query query)
            : base(prototype)
        {
            m_query = query;
        }

        public Query Query
        {
            get { return m_query; }
        }

        [JSProperty(Name = "authenticationType")]
        public string AuthenticationType
        {
            get
            {
                return m_query.AuthenticationType.ToString();
            }
            set
            {
                QueryAuthenticationType authenticationType;
                if (value.TryParseEnum(true, out authenticationType))
                    m_query.AuthenticationType = authenticationType;
            }
        }

        //Culture

        [JSProperty(Name = "directServiceEndpointUri")]
        public UriInstance DirectServiceEndpointUri
        {
            get
            {
                return m_query.DirectServiceEndpointUri == null
                    ? null
                    : new UriInstance(Engine.Object.InstancePrototype, m_query.DirectServiceEndpointUri);
            }
            set
            {
                if (value == null)
                {
                    m_query.DirectServiceEndpointUri = null;
                    return;
                }

                m_query.DirectServiceEndpointUri = value.Uri;
            }
        }

        [JSProperty(Name = "enableNicknames")]
        public bool EnableNicknames
        {
            get
            {
                return m_query.EnableNicknames;
            }
            set
            {
                m_query.EnableNicknames = value;
            }
        }

        [JSProperty(Name = "enablePhonetic")]
        public bool EnablePhonetic
        {
            get
            {
                return m_query.EnablePhonetic;
            }
            set
            {
                m_query.EnablePhonetic = value;
            }
        }

        [JSProperty(Name = "enableStemming")]
        public bool EnableStemming
        {
            get
            {
                return m_query.EnableStemming;
            }
            set
            {
                m_query.EnableStemming = value;
            }
        }

        [JSProperty(Name = "highlightedSentenceCount")]
        public int HighlightedSentenceCount
        {
            get
            {
                return m_query.HighlightedSentenceCount;
            }
            set
            {
                m_query.HighlightedSentenceCount = value;
            }
        }

        //QueryHint
        //HitHighlightedProperties

        [JSProperty(Name = "ignoreAllNoiseQuery")]
        public bool IgnoreAllNoiseQuery
        {
            get
            {
                return m_query.IgnoreAllNoiseQuery;
            }
            set
            {
                m_query.IgnoreAllNoiseQuery = value;
            }
        }

        //KeywordInclusion
        //PagingCookie

        [JSProperty(Name = "partitionId")]
        public GuidInstance PartitionId
        {
            get
            {
                return m_query.PartitionId == default(Guid)
                    ? null
                    : new GuidInstance(Engine.Object.InstancePrototype, m_query.PartitionId);
            }
            set
            {
                m_query.PartitionId = value == null
                    ? default(Guid)
                    : value.Value;
            }
        }

        //PersonalizationData
        //QueryInfo

        [JSProperty(Name = "queryText")]
        public string QueryText
        {
            get
            {
                return m_query.QueryText;
            }
            set
            {
                m_query.QueryText = value;
            }
        }

        [JSProperty(Name = "rankingModelId")]
        public string RankingModelId
        {
            get
            {
                return m_query.RankingModelId;
            }
            set
            {
                m_query.RankingModelId = value;
            }
        }

        //ResultsProvider

        [JSProperty(Name = "resultTypes")]
        public string ResultTypes
        {
            get
            {
                return m_query.ResultTypes.ToString();
            }
            set
            {
                ResultType resultTypes;
                if (value.TryParseEnum(true, out resultTypes))
                    m_query.ResultTypes = resultTypes;
            }
        }

        [JSProperty(Name = "rowLimit")]
        public int RowLimit
        {
            get
            {
                return m_query.RowLimit;
            }
            set
            {
                m_query.RowLimit = value;
            }
        }

        [JSProperty(Name = "rowsPerPage")]
        public int RowsPerPage
        {
            get
            {
                return m_query.RowsPerPage;
            }
            set
            {
                m_query.RowsPerPage = value;
            }
        }

        //SearchApplication

        [JSProperty(Name = "site")]
        public SPSiteInstance Site
        {
            get
            {
                return m_query.Site == null
                    ? null
                    : new SPSiteInstance(Engine.Object.InstancePrototype, m_query.Site);
            }
        }

        [JSProperty(Name = "siteContext")]
        public UriInstance SiteContext
        {
            get
            {
                return m_query.SiteContext == null
                    ? null
                    : new UriInstance(Engine.Object.InstancePrototype, m_query.SiteContext);
            }
            set
            {
                if (value == null)
                {
                    m_query.SiteContext = null;
                    return;
                }

                m_query.SiteContext = value.Uri;
            }
        }

        [JSProperty(Name = "startRow")]
        public int StartRow
        {
            get
            {
                return m_query.StartRow;
            }
            set
            {
                m_query.StartRow = value;
            }
        }

        [JSProperty(Name = "summaryLength")]
        public int SummaryLength
        {
            get
            {
                return m_query.SummaryLength;
            }
            set
            {
                m_query.SummaryLength = value;
            }
        }

        [JSProperty(Name = "timeout")]
        public int Timeout
        {
            get
            {
                return m_query.Timeout;
            }
            set
            {
                m_query.Timeout = value;
            }
        }

        [JSProperty(Name = "totalRowsExactMinimum")]
        public int TotalRowsExactMinimum
        {
            get
            {
                return m_query.TotalRowsExactMinimum;
            }
            set
            {
                m_query.TotalRowsExactMinimum = value;
            }
        }

        [JSProperty(Name = "trimDuplicates")]
        public bool TrimDuplicates
        {
            get
            {
                return m_query.TrimDuplicates;
            }
            set
            {
                m_query.TrimDuplicates = value;
            }
        }

        [JSProperty(Name = "urlZone")]
        public string UrlZone
        {
            get
            {
                return m_query.UrlZone.ToString();
            }
        }

        [JSFunction(Name = "execute")]
        public ResultTableCollectionInstance Execute()
        {
            var resultTableCollection = m_query.Execute();
            return resultTableCollection == null
                ? null
                : new ResultTableCollectionInstance(Engine.Object.InstancePrototype, resultTableCollection);
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_query.Dispose();
        }

        //GetProperties
        //GetQuerySuggestions

        [JSFunction(Name = "getScopes")]
        [JSDoc("ternReturnType", "[+ScopeInformation]")]
        public ArrayInstance GetScopes()
        {
            var result = Engine.Array.Construct();
            var scopes = m_query.GetScopes();
            foreach (var scope in scopes)
                ArrayInstance.Push(result, new ScopeInformationInstance(Engine, scope));

            return result;
        }

        [JSFunction(Name = "highlightStringValue")]
        public ObjectInstance HighlightStringValue(string strValue, bool fLastTermByPrefix, bool fQuerySuggestions)
        {
            var result = Engine.Object.Construct();
            bool hasHighlight;
            var value = m_query.HighlightStringValue(strValue, fLastTermByPrefix, fQuerySuggestions, out hasHighlight);

            result.SetPropertyValue("value", value, false);
            result.SetPropertyValue("hasHighlight", hasHighlight, false);
            return result;
        }
    }
}
