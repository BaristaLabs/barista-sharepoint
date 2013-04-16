namespace Barista
{
  using System.Collections.Generic;
  using System.ServiceModel;
  using Barista.Search;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaSearch
  {
    [OperationContract]
    void IndexDocument(string indexName, string documentId, DocumentDto document);

    [OperationContract]
    void IndexJsonDocument(string indexName, JsonDocumentDto document);

    [OperationContract]
    void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents);

    [OperationContract]
    void DeleteDocuments(string indexName, IEnumerable<string> documentIds);

    [OperationContract]
    void DeleteAllDocuments(string indexName);

    [OperationContract]
    JsonDocumentDto Retrieve(string indexName, string documentId);

    [OperationContract]
    IList<SearchResult> SearchWithQuery(string indexName, Query query, int maxResults);

    [OperationContract]
    IList<SearchResult> SearchWithQueryParser(string indexName, string defaultField, string query, int maxResults);

    [OperationContract]
    IList<SearchResult> SearchWithOData(string indexName, string defaultField, string queryString);
  }
}
