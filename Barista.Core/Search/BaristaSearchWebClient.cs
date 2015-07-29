namespace Barista.Search
{
    using System.Collections.ObjectModel;
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
            Url = serviceUrl;
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
                JsonSerializer = JsonNetSerializer,
                RequestFormat = DataFormat.Json
            };

            configRequest(restRequest);

            var client = new RestClient(Url);
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
            var restRequest = new RestRequest(operation, method)
            {
                JsonSerializer = JsonNetSerializer,
                RequestFormat = DataFormat.Json
            };

            configRequest(restRequest);

            var client = new RestClient(Url);
            client.AddHandler("application/json", new JsonNetDeserializer());

            var response = client.Execute<T>(restRequest);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException("Error Calling Service: " + response.StatusDescription);

            if (response.ErrorException != null)
                throw response.ErrorException;

            return response;
        }

        public void DeleteDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<string> documentIds)
        {
            ExecuteRestRequest("DeleteDocuments", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                documentIds
            }));
        }

        public void DeleteAllDocuments(BaristaIndexDefinition indexDefinition)
        {
            ExecuteRestRequest("DeleteAllDocuments", Method.POST, request => request.AddBody(new
            {
                indexDefinition
            }));
        }

        public bool DoesIndexExist(BaristaIndexDefinition indexDefinition)
        {
            var response = ExecuteRestRequest<bool>("DoesIndexExist", Method.POST,
                request => request.AddBody(new
                {
                    indexDefinition
                }));

            return response.Data;
        }

        public Explanation Explain(BaristaIndexDefinition indexDefinition, Query query, int documentId)
        {
            var response = ExecuteRestRequest<Explanation>("Explain", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                query,
                documentId
            }));

            return response.Data;
        }

        public ICollection<string> GetFieldNames(BaristaIndexDefinition indexDefinition)
        {
            var response = ExecuteRestRequest<Collection<string>>("GetFieldNames", Method.POST,
                request => request.AddBody(new
                {
                    indexDefinition
                }));

            return response.Data;
        }

        public string Highlight(BaristaIndexDefinition indexDefinition, Query query, int documentId, string fieldName, int fragCharSize)
        {
            var response = ExecuteRestRequest("Highlight", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                query,
                documentId,
                fieldName,
                fragCharSize
            }));

            return response.Content;
        }

        public void IndexDocument(BaristaIndexDefinition indexDefinition, string documentId, DocumentDto document)
        {
            ExecuteRestRequest("IndexDocument", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                document
            }));
        }

        public void IndexJsonDocument(BaristaIndexDefinition indexDefinition, JsonDocumentDto document)
        {
            ExecuteRestRequest("IndexJsonDocument", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                document
            }));
        }

        public void IndexJsonDocuments(BaristaIndexDefinition indexDefinition, IEnumerable<JsonDocumentDto> documents)
        {
            ExecuteRestRequest("IndexJsonDocuments", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                documents
            }));
        }

        public JsonDocumentDto Retrieve(BaristaIndexDefinition indexDefinition, string documentId)
        {
            var response = ExecuteRestRequest<JsonDocumentDto>("Retrieve", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                documentId
            }));

            return response.Data;
        }

        public IList<SearchResult> Search(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            var response = ExecuteRestRequest<List<SearchResult>>("Search", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                arguments
            }));

            return response.Data;
        }

        public int SearchResultCount(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            var response = ExecuteRestRequest<int>("SearchResultCount", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                arguments
            }));

            return response.Data;
        }

        public IList<FacetedSearchResult> FacetedSearch(BaristaIndexDefinition indexDefinition, SearchArguments arguments)
        {
            var response = ExecuteRestRequest<List<FacetedSearchResult>>("FacetedSearch", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                arguments
            }));

            return response.Data;
        }

        public void SetFieldOptions(BaristaIndexDefinition indexDefinition, IEnumerable<FieldOptions> fieldOptions)
        {
            ExecuteRestRequest("SetFieldOptions", Method.POST, request => request.AddBody(new
            {
                indexDefinition,
                fieldOptions
            }));
        }

        public void Shutdown(BaristaIndexDefinition indexDefinition)
        {
            ExecuteRestRequest("Shutdown", Method.POST, request => request.AddBody(new
            {
                indexDefinition
            }));
        }
    }
}
