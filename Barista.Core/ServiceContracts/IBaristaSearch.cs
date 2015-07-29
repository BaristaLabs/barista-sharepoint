namespace Barista
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using Barista.Search;

    [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
    public interface IBaristaSearch
    {
        [OperationContract]
        void DeleteDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<string> documentIds);

        [OperationContract]
        void DeleteAllDocuments(BaristaIndexDefinition indexDefinition);

        [OperationContract]
        bool DoesIndexExist(BaristaIndexDefinition indexDefinition);

        [OperationContract]
        Explanation Explain(BaristaIndexDefinition indexDefinition, Barista.Search.Query query, int documentId);

        [OperationContract]
        ICollection<string> GetFieldNames(BaristaIndexDefinition indexDefinition);

        [OperationContract]
        string Highlight(BaristaIndexDefinition indexDefinition, Barista.Search.Query query, int documentId, string fieldName, int fragCharSize);

        [OperationContract]
        void IndexDocument(BaristaIndexDefinition indexDefinition, string documentId, DocumentDto document);

        [OperationContract]
        void IndexJsonDocument(BaristaIndexDefinition indexDefinition, JsonDocumentDto document);

        [OperationContract]
        void IndexJsonDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<JsonDocumentDto> documents);

        [OperationContract]
        JsonDocumentDto Retrieve(BaristaIndexDefinition indexDefinition, string documentId);

        [OperationContract]
        IList<SearchResult> Search(BaristaIndexDefinition indexDefinition, SearchArguments arguments);

        [OperationContract]
        int SearchResultCount(BaristaIndexDefinition indexDefinition, SearchArguments arguments);

        [OperationContract]
        IList<FacetedSearchResult> FacetedSearch(BaristaIndexDefinition indexDefinition, SearchArguments arguments);

        [OperationContract]
        void SetFieldOptions(BaristaIndexDefinition indexDefinition, IEnumerable<FieldOptions> fieldOptions);

        [OperationContract]
        void Shutdown(BaristaIndexDefinition indexDefinition);
    }
}
