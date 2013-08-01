namespace Barista.Search
{
  using Barista.Framework;
  using Barista.Newtonsoft.Json;
  using RestSharp;
  using RestSharp.Serializers;
  using System;
  using System.Collections.Generic;
  using System.Net;

  public class BaristaSearchWebClient : IBaristaSearch
  {
    
    public BaristaSearchWebClient(string serviceUrl)
    {
      if (String.IsNullOrEmpty(serviceUrl))
        throw new ArgumentNullException("serviceUrl", @"The url of the Barista Search Index must be specified.");
      this.Url = serviceUrl;
    }

    public string Url
    {
      get;
      set;
    }

    protected ISerializer JsonNetSerializer
    {
      get
      {
        var js = new Barista.Newtonsoft.Json.JsonSerializer
        {
          //DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
          TypeNameHandling = TypeNameHandling.Auto,
          //TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
        };

        var s = new JsonNetSerializer(js);
        return s;
      }
    }

    protected IRestResponse ExecuteRestRequest(string operation, Method method, Action<RestRequest> configRequest)
    {
      var restRequest = new RestRequest(operation, method)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      configRequest(restRequest);

      var client = new RestClient(this.Url);
      client.AddHandler("application/json", new JsonNetDeserializer());

      var response = client.Execute(restRequest);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response;
    }

    protected IRestResponse<T> ExecuteRestRequest<T>(string operation, Method method, Action<RestRequest> configRequest)
      where T : new()
    {
      var restRequest =  new RestRequest(operation, method)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      configRequest(restRequest);

      var client = new RestClient(this.Url);
      client.AddHandler("application/json", new JsonNetDeserializer());

      var response = client.Execute<T>(restRequest);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response;
    }

    public void DeleteDocuments(string indexName, IEnumerable<string> documentIds)
    {
      ExecuteRestRequest("DeleteDocuments", Method.POST, request => request.AddBody(new
      {
        indexName,
        documentIds
      }));
    }

    public void DeleteAllDocuments(string indexName)
    {
      ExecuteRestRequest("DeleteAllDocuments", Method.POST, request => request.AddParameter("indexName", indexName));
    }

    public bool DoesIndexExist(string indexName)
    {
      var response = ExecuteRestRequest<bool>("DoesIndexExist", Method.POST,
        request => request.AddParameter("indexName", indexName));

      return response.Data;
    }

    public Explanation Explain(string indexName, Query query, int documentId)
    {
      var response = ExecuteRestRequest<Explanation>("Explain", Method.POST, request => request.AddBody(new
      {
        indexName,
        query,
        documentId
      }));

      return response.Data;
    }

    public string Highlight(string indexName, Query query, int documentId, string fieldName, int fragCharSize)
    {
      var response = ExecuteRestRequest("Highlight", Method.POST, request => request.AddBody(new
      {
        indexName,
        query,
        documentId,
        fieldName,
        fragCharSize
      }));

      return response.Content;
    }

    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      ExecuteRestRequest("IndexDocument", Method.POST, request => request.AddBody(new
      {
        indexName,
        document
      }));
    }

    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      ExecuteRestRequest("IndexJsonDocument", Method.POST, request => request.AddBody(new
      {
        indexName,
        document
      }));
    }

    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      ExecuteRestRequest("IndexJsonDocuments", Method.POST, request => request.AddBody(new
      {
        indexName,
        documents
      }));
    }

    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      var response = ExecuteRestRequest <JsonDocumentDto>("Retrieve", Method.GET, request =>
      {
        request.AddParameter("indexName", indexName);
        request.AddParameter("documentId", documentId);
      });

      return response.Data;
    }

    public IList<SearchResult> Search(string indexName, SearchArguments arguments)
    {
      var response = ExecuteRestRequest<List<SearchResult>>("Search", Method.POST, request => request.AddBody(new
      {
        indexName,
        arguments
      }));

      return response.Data;
    }

    public int SearchResultCount(string indexName, SearchArguments arguments)
    {
      var response = ExecuteRestRequest<int>("SearchResultCount", Method.POST, request => request.AddBody(new
      {
        indexName,
        arguments
      }));

      return response.Data;
    }

    public IList<FacetedSearchResult> FacetedSearch(string indexName, SearchArguments arguments)
    {
      var response = ExecuteRestRequest<List<FacetedSearchResult>>("FacetedSearch", Method.POST, request => request.AddBody(new
      {
        indexName,
        arguments
      }));

      return response.Data;
    }

    public void SetFieldOptions(string indexName, IEnumerable<FieldOptions> fieldOptions)
    {
      ExecuteRestRequest("SetFieldOptions", Method.POST, request => request.AddBody(new
      {
        indexName,
        fieldOptions
      }));
    }
  }
}
