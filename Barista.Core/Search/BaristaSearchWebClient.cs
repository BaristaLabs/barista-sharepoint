namespace Barista.Search
{
  using System.Net;
  using Barista.Framework;
  using Barista.Newtonsoft.Json;
  using RestSharp;
  using RestSharp.Serializers;
  using System;
  using System.Collections.Generic;

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
          DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };

        var s = new JsonNetSerializer(js);
        return s;
      }
    }

    public void DeleteDocuments(string indexName, IEnumerable<string> documentIds)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("DeleteDocuments", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new {
        indexName = indexName,
        documentIds = documentIds
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;
    }

    public void DeleteAllDocuments(string indexName)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("DeleteAllDocuments", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;
    }

    public bool DoesIndexExist(string indexName)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("DoesIndexExist", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddParameter("indexName", indexName);

      // execute the request
      var response = client.Execute<bool>(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response.Data;
    }

    public Explanation Explain(string indexName, Query query, int documentId)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("Explain", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName,
        query = query,
        documentId = documentId
      });

      // execute the request
      var response = client.Execute<Explanation>(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response.Data;
    }

    public string Highlight(string indexName, Query query, int documentId, string fieldName, int fragCharSize)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("Highlight", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName,
        query = query,
        documentId = documentId,
        fieldName = fieldName,
        fragCharSize = fragCharSize
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response.Content;
    }

    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("IndexDocument", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName,
        document = document
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;
    }

    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("IndexJsonDocument", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new {
        indexName = indexName,
        document = document
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;
    }

    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("IndexJsonDocuments", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName,
        documents = documents
      });

      // execute the request
      var response = client.Execute(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;
    }

    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("Retrieve", Method.GET)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddParameter("indexName", indexName);
      request.AddParameter("documentId", documentId);

      // execute the request
      var response = client.Execute<JsonDocumentDto>(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response.Data;
    }

    public IList<SearchResult> Search(string indexName, SearchArguments arguments)
    {
      var client = new RestClient(this.Url);

      var request = new RestRequest("Search", Method.POST)
      {
        JsonSerializer = this.JsonNetSerializer,
        RequestFormat = DataFormat.Json
      };

      request.AddBody(new
      {
        indexName = indexName,
        arguments = arguments
      });

      // execute the request
      var response = client.Execute<List<SearchResult>>(request);

      if (response.StatusCode != HttpStatusCode.OK)
        throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

      if (response.ErrorException != null)
        throw response.ErrorException;

      return response.Data;
    }

    public int SearchResultCount(string indexName, SearchArguments arguments)
    {
      throw new System.NotImplementedException();
    }

    public IList<FacetedSearchResult> FacetedSearch(string indexName, SearchArguments arguments)
    {
      throw new System.NotImplementedException();
    }

    public void SetFieldOptions(string indexName, IEnumerable<FieldOptions> fieldOptions)
    {
      throw new System.NotImplementedException();
    }
  }
}
