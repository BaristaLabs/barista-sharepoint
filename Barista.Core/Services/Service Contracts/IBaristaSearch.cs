namespace Barista.Services
{
  using System.Collections.Generic;
  using System.ServiceModel;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaSearch
  {
    [OperationContract]
    void IndexDocument(string indexName, string documentId, Document document);

    [OperationContract]
    void IndexJsonDocument(string indexName, JsonDocument document);

    [OperationContract]
    void IndexJsonDocuments(string indexName, IEnumerable<JsonDocument> documents);

    [OperationContract]
    void DeleteDocuments(string indexName, IEnumerable<string> documentIds);

    [OperationContract]
    void DeleteAllDocuments(string indexName);

    [OperationContract]
    JsonDocument Retrieve(string indexName, string documentId);
   
    [OperationContract]
    IList<SearchResult> Search(string indexName, string defaultField, string query, int maxResults);

    [OperationContract]
    IList<SearchResult> SearchOData(string indexName, string defaultField, string queryString);
  }
}
