namespace Barista.SharePoint.HostService
{
  using System.ServiceModel;

  [ServiceContract(Namespace = Barista.Constants.ServiceNamespace)]
  public interface IBaristaSearch
  {
    [OperationContract]
    void SetupIndex(SearchIndexOptions options);

    [OperationContract]
    void RemoveIndex(string indexId);

    [OperationContract]
    void AddDocumentToIndex(string indexId, Document document);

    [OperationContract]
    void UpdateDocumentInIndex(string indexId, Document document);

    [OperationContract]
    void RemoveDocumentFromIndex(string indexId, string key, string value);

    [OperationContract]
    void SearchIndex(string indexUrl, string query);
  }
}
