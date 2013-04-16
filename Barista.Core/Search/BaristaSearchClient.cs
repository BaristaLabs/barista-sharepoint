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

    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      Channel.IndexDocument(indexName, documentId, document);
    }

    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      Channel.IndexJsonDocument(indexName, document);
    }

    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      Channel.IndexJsonDocuments(indexName, documents);
    }

    public void DeleteDocuments(string indexName, IEnumerable<string> documentIds)
    {
      Channel.DeleteDocuments(indexName, documentIds);
    }

    public void DeleteAllDocuments(string indexName)
    {
      Channel.DeleteAllDocuments(indexName);
    }

    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      return Channel.Retrieve(indexName, documentId);
    }

    public IList<SearchResult> SearchWithQuery(string indexName, Query query, int maxResults)
    {
      return Channel.SearchWithQuery(indexName, query, maxResults);
    }

    public IList<SearchResult> SearchWithQueryParser(string indexName, string defaultField, string query, int maxResults)
    {
      return Channel.SearchWithQueryParser(indexName, defaultField, query, maxResults);
    }

    public IList<SearchResult> SearchWithOData(string indexName, string defaultField, string queryString)
    {
      return Channel.SearchWithOData(indexName, defaultField, queryString);
    }
  }
}
