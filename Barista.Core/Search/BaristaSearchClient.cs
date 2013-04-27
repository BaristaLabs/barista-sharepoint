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

    public string Highlight(string indexName, Query query, int documentId, string fieldName, int fragCharSize)
    {
      return Channel.Highlight(indexName, query, documentId, fieldName, fragCharSize);
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

    public Explanation Explain(string indexName, Query query, int documentId)
    {
      return Channel.Explain(indexName, query, documentId);
    }

    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      return Channel.Retrieve(indexName, documentId);
    }

    public IList<SearchResult> Search(string indexName, SearchArguments arguments)
    {
      return Channel.Search(indexName, arguments);
    }

    public IList<FacetedSearchResult> FacetedSearch(string indexName, SearchArguments arguments)
    {
      return Channel.FacetedSearch(indexName, arguments);
    }

    public void SetFieldOptions(string indexName, IEnumerable<FieldOptions> fieldOptions)
    {
      Channel.SetFieldOptions(indexName, fieldOptions);
    }
  }
}
