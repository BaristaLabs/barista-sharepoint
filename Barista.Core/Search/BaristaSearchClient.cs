namespace Barista.Search
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;

    [DebuggerStepThrough]
    public class BaristaSearchClient : ClientBase<IBaristaSearch>, IBaristaSearch
    {

        public BaristaSearchClient()
        {
        }

        public BaristaSearchClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public BaristaSearchClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public BaristaSearchClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public BaristaSearchClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        public ICollection<string> GetFieldNames(BaristaIndexDefinition indexDefinition)
        {
            return Channel.GetFieldNames(indexDefinition);
        }

        public string Highlight(BaristaIndexDefinition indexDefinition, Query query, int documentId, string fieldName, int fragCharSize)
        {
            return Channel.Highlight(indexDefinition, query, documentId, fieldName, fragCharSize);
        }

        public void IndexDocument(BaristaIndexDefinition indexDefinition, string documentId, DocumentDto document)
        {
            Channel.IndexDocument(indexDefinition, documentId, document);
        }

        public void IndexJsonDocument(BaristaIndexDefinition indexDefinition, JsonDocumentDto document)
        {
            Channel.IndexJsonDocument(indexDefinition, document);
        }

        public void IndexJsonDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<JsonDocumentDto> documents)
        {
            Channel.IndexJsonDocuments(indexDefinition, documents);
        }

        public void DeleteDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<string> documentIds)
        {
            Channel.DeleteDocuments(indexDefinition, documentIds);
        }

        public void DeleteAllDocuments(BaristaIndexDefinition indexDefinition)
        {
            Channel.DeleteAllDocuments(indexDefinition);
        }

        public bool DoesIndexExist(BaristaIndexDefinition indexDefinition)
        {
            return Channel.DoesIndexExist(indexDefinition);
        }

        public Explanation Explain(BaristaIndexDefinition indexDefinition, Query query, int documentId)
        {
            return Channel.Explain(indexDefinition, query, documentId);
        }

        public JsonDocumentDto Retrieve(BaristaIndexDefinition indexDefinition, string documentId)
        {
            return Channel.Retrieve(indexDefinition, documentId);
        }

        public IList<SearchResult> Search(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            return Channel.Search(indexDefinition, arguments);
        }

        public int SearchResultCount(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            return Channel.SearchResultCount(indexDefinition, arguments);
        }

        public IList<FacetedSearchResult> FacetedSearch(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            return Channel.FacetedSearch(indexDefinition, arguments);
        }

        public void SetFieldOptions(BaristaIndexDefinition indexDefinition, IEnumerable<FieldOptions> fieldOptions)
        {
            Channel.SetFieldOptions(indexDefinition, fieldOptions);
        }

        public void Shutdown(BaristaIndexDefinition indexDefinition)
        {
            Channel.Shutdown(indexDefinition);
        }
    }
}
