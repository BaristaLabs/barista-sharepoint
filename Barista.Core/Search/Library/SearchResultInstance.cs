namespace Barista.Search.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Search;
    using System;

    [Serializable]
    public sealed class SearchResultInstance : ObjectInstance
    {
        private readonly SearchResult m_searchResult;

        public SearchResultInstance(ScriptEngine engine, SearchResult searchResult)
            : base(engine)
        {
            if (searchResult == null)
                throw new ArgumentNullException("searchResult");

            m_searchResult = searchResult;

            this.PopulateFunctions();
        }

        public SearchResult SearchResult
        {
            get { return m_searchResult; }
        }

        [JSProperty(Name = "score")]
        public double Score
        {
            get { return m_searchResult.Score; }
            set { m_searchResult.Score = Convert.ToSingle(value); }
        }

        [JSProperty(Name = "luceneDocId")]
        public int LuceneDocId
        {
            get { return m_searchResult.LuceneDocId; }
            set { m_searchResult.LuceneDocId = value; }
        }

        [JSProperty(Name = "document")]
        public JsonDocumentInstance Document
        {
            get { return new JsonDocumentInstance(this.Engine, m_searchResult.Document); }
            set { m_searchResult.Document = value.JsonDocument; }
        }
    }
}
