namespace Barista.Services
{
  using System.Collections.Generic;
  using System.ServiceModel;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaSearch
  {
    [OperationContract]
    void IndexDocument(DirectoryDefinition definition, Document document);

    [OperationContract]
    void IndexJsonDocument(DirectoryDefinition definition, JsonDocument document);

    [OperationContract]
    void IndexJsonDocuments(DirectoryDefinition definition, IEnumerable<JsonDocument> documents);

    [OperationContract]
    void DeleteDocuments(DirectoryDefinition definition, IEnumerable<string> documentIds);

    [OperationContract]
    void DeleteAllDocuments(DirectoryDefinition definition);

    [OperationContract]
    JsonDocument Retrieve(DirectoryDefinition definition, string documentId);
   
    [OperationContract]
    IList<SearchResult> Search(DirectoryDefinition definition, string defaultField, string query, int maxResults);

    [OperationContract]
    IList<SearchResult> SearchOData(DirectoryDefinition definition, string defaultField, IDictionary<string, string> filterParameters);
  }
}
