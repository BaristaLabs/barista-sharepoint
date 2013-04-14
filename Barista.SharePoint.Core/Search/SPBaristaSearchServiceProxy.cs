namespace Barista.SharePoint.Search
{
  using System.Security.Principal;
  using Barista.Extensions;
  using Barista.SharePoint.SPBaristaSearchService;
  using Microsoft.SharePoint.Administration;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.ServiceModel;
  using Document = Barista.SharePoint.SPBaristaSearchService.Document;
  using IBaristaSearch = Barista.SharePoint.SPBaristaSearchService.IBaristaSearch;
  using JsonDocument = Barista.SharePoint.SPBaristaSearchService.JsonDocument;
  using SearchResult = Barista.SharePoint.SPBaristaSearchService.SearchResult;

  public class SPBaristaSearchServiceProxy : IBaristaSearch
  {
    private readonly Uri m_serviceAddress;
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
          MaxBytesPerRead = 2048,
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
        new EndpointAddress(m_serviceAddress.ToString()));

      client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
      client.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;

      return client;
    }

    public void DeleteDocuments(string indexName, List<string> documentIds)
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

    public void IndexDocument(string indexName, string documentId, Document document)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexDocument(indexName, documentId, document);
      }
    }

    public void IndexJsonDocument(string indexName, JsonDocument document)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexJsonDocument(indexName, document);
      }
    }

    public void IndexJsonDocuments(string indexName, List<JsonDocument> documents)
    {
      using (var searchClient = GetSearchClient())
      {
        searchClient.IndexJsonDocuments(indexName, documents);
      }
    }

    public JsonDocument Retrieve(string indexName, string documentId)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Retrieve(indexName, documentId);
      }
    }

    public List<SearchResult> Search(string indexName, string defaultField, string query, int maxResults)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.Search(indexName, defaultField, query, maxResults);
      }
    }

    public List<SearchResult> SearchOData(string indexName, string defaultField, string queryString)
    {
      using (var searchClient = GetSearchClient())
      {
        return searchClient.SearchOData(indexName, defaultField, queryString);
      }
    }
  }
}
