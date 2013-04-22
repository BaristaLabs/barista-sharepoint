namespace Barista.SharePoint.Search
{
  using System.Security.Principal;
  using Barista.Extensions;
  using Barista.Search;
  using Microsoft.SharePoint.Administration;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.ServiceModel;
  using Barista.Newtonsoft.Json;

  public class SPBaristaSearchServiceProxy : IBaristaSearch
  {
    private readonly Uri m_serviceAddress;
    private readonly EndpointIdentity m_serviceIdentity;
    private readonly WSHttpBinding m_baristaSearchBinding;

    public SPBaristaSearchServiceProxy()
    {
      var farm = SPFarm.Local;
      if (farm == null)
        throw new InvalidOperationException("SharePoint farm not found.");

      //Obtain the search (windows) service registered with the farm.
      var searchService =
          SPFarm.Local.Services.GetValue<Barista.SharePoint.ServiceManagement.BaristaSearchService>(
            Barista.SharePoint.ServiceManagement.BaristaSearchService.NtServiceName);

      if (searchService == null)
        throw new InvalidOperationException("The Barista Search Service is not registered with the farm. Please have your administrator run SetupBaristSearchService.ps1 referenced in the Barista installation documentation.");
      
      //TODO: Make this multi-service application aware.
      var serviceAffinity = Utilities.GetFarmKeyValue("BaristaSearchServiceAffinity");
      SPServiceInstance searchServiceInstance;
      if (serviceAffinity.IsNullOrWhiteSpace())
      {
        searchServiceInstance = searchService.Instances.FirstOrDefault();

        if (searchServiceInstance == null)
          throw new InvalidOperationException("No Barista Search Service Instances are registered with the farm.");
      }
      else
      {
        searchServiceInstance =
          searchService.Instances.FirstOrDefault(ss => ss.Id.ToString() == serviceAffinity);

        if (searchServiceInstance == null)
          throw new InvalidOperationException("The current service application specifies a search service affinity, however, that search service instance cannot be located.");
      }

      if (searchServiceInstance.Status != SPObjectStatus.Online)
        throw new InvalidOperationException("A Barista Search Service Instance was located, however it is currently not online.");

      var serverAddress = searchServiceInstance.Server.Address;
      //serverAddress = System.Net.Dns.GetHostEntry(serverAddress).HostName;

      m_serviceIdentity = EndpointIdentity.CreateDnsIdentity(serverAddress);
      m_serviceAddress = new Uri("http://" + serverAddress + ":8500/Barista/Search", UriKind.Absolute);
      m_baristaSearchBinding = InitServiceBinding();
    }

    public SPBaristaSearchServiceProxy(Uri serviceAddress)
    {
      m_serviceAddress = serviceAddress;

      m_baristaSearchBinding = InitServiceBinding();
    }

    private static WSHttpBinding InitServiceBinding()
    {
      var binding = new WSHttpBinding
      {
        AllowCookies = true,
        ReceiveTimeout = TimeSpan.FromHours(1),
        SendTimeout = TimeSpan.FromHours(1),
        OpenTimeout = TimeSpan.FromHours(1),
        CloseTimeout = TimeSpan.FromHours(1),
        MaxReceivedMessageSize = int.MaxValue,
        ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas
        {
          MaxArrayLength = int.MaxValue,
          MaxBytesPerRead = int.MaxValue,
          MaxDepth = int.MaxValue,
          MaxNameTableCharCount = int.MaxValue,
          MaxStringContentLength = int.MaxValue,
        }
      };

      //binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
      binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
      return binding;
    }

    private BaristaSearchClient GetSearchClient()
    {
      var client = new BaristaSearchClient(m_baristaSearchBinding,
        new EndpointAddress(m_serviceAddress, m_serviceIdentity));

      if (client.ClientCredentials != null)
      {
        client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
        client.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
      }

      return client;
    }

    public void DeleteDocuments(string indexName, IEnumerable<string> documentIds)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.DeleteDocuments(indexName, documentIds);
      }
    }

    public void DeleteAllDocuments(string indexName)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.DeleteAllDocuments(indexName);
      }
    }

    public Explanation Explain(string indexName, Query query, int documentId)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Explain(indexName, query, documentId);
      }
    }

    public string Highlight(string indexName, Query query, int documentId, string fieldName, int fragCharSize)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Highlight(indexName, query, documentId, fieldName, fragCharSize);
      }
    }

    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexDocument(indexName, documentId, document);
      }
    }

    public void IndexJsonDocument(string indexName, string documentId, object docObject, object metadata)
    {
      using (var searchClient = GetSearchClient())
      {
        var document = new JsonDocumentDto
          {
            DocumentId = documentId,
            DataAsJson = JsonConvert.SerializeObject(docObject, Formatting.Indented),
          };

        if (metadata != null)
          document.MetadataAsJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);

        searchClient.IndexJsonDocument(indexName, document);
      }
    }

    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexJsonDocument(indexName, document);
      }
    }

    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexJsonDocuments(indexName, documents);
      }
    }

    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Retrieve(indexName, documentId);
      }
    }

    public IList<SearchResult> Search(string indexName, SearchArguments arguments)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Search(indexName, arguments);
      }
    }

    public IList<FacetedSearchResult> FacetedSearch(string indexName, SearchArguments arguments)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.FacetedSearch(indexName, arguments);
      }
    }
  }
}
