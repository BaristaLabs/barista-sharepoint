namespace Barista.SharePoint.Search
{
  using System.Reflection;
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

    public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
    {
      TypeNameHandling = TypeNameHandling.Auto
    };

    public SPBaristaSearchServiceProxy()
    {
      var farm = SPFarm.Local;
      if (farm == null)
        throw new InvalidOperationException("SharePoint farm not found.");

      //Obtain the search (windows) service registered with the farm.
      var searchService =
          farm.Services.GetValue<Barista.SharePoint.ServiceManagement.BaristaSearchService>(
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

      if (searchServiceInstance.Server == null)
        throw new InvalidOperationException("A Barista Search Service Instance was located, however, it is not currently associated with a server.");

      var serverAddress = searchServiceInstance.Server.Address;
      m_serviceAddress = new Uri("http://" + serverAddress + ":8500/Barista/Search", UriKind.Absolute);

      if (searchService.ProcessIdentity != null && searchService.ProcessIdentity.ManagedAccount != null)
      {
        var propertyInfo = typeof (SPManagedAccount).GetProperty("UPNName",
                                                                 BindingFlags.Instance | BindingFlags.NonPublic);
        var upn = (string)propertyInfo.GetValue(searchService.ProcessIdentity.ManagedAccount, null);
        if (upn.IsNullOrWhiteSpace())
          throw new InvalidOperationException("Unable to determine service accout UPN for authentication.");

        m_serviceIdentity = EndpointIdentity.CreateUpnIdentity(upn);
      }
      else
      {
        m_serviceIdentity =
          EndpointIdentity.CreateDnsIdentity(SPServer.Local.Address.ToLowerInvariant() ==
                                             serverAddress.ToLowerInvariant()
                                               ? serverAddress
                                               : "");
      }

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
            },
          BypassProxyOnLocal = true,
          UseDefaultWebProxy = false,
        };

      //binding.ReliableSession.Enabled = true;
      //binding.ReliableSession.Ordered = true;

      //FIXME: This should use message-based security, change it back to SecurityMode.None.
      //binding.Security.Mode = SecurityMode.None;
      //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
      //binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
      //binding.Security.Transport.Realm = "";

      //binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
      //binding.Security.Message.NegotiateServiceCredential = true;

      return binding;
    }

    private BaristaSearchClient GetSearchClient()
    {
      var client = new BaristaSearchClient(m_baristaSearchBinding,
        new EndpointAddress(m_serviceAddress, m_serviceIdentity));

      //FIXME: This should use message-based security, change it back to SecurityMode.None.
      //if (client.ClientCredentials != null)
      //{
      //  client.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
      //  client.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
      //}

      return client;
    }

    public bool DoesIndexExist(string indexName)
    {
      var directory = BaristaHelper.GetDirectoryFromIndexName(indexName);

      return directory != null;
    }

    public void DeleteDocuments(string indexName, IEnumerable<string> documentIds)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.DeleteDocuments(indexName, documentIds);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public void DeleteAllDocuments(string indexName)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.DeleteAllDocuments(indexName);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public Explanation Explain(string indexName, Query query, int documentId)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          return searchClient.Explain(indexName, query, documentId);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public string Highlight(string indexName, Query query, int documentId, string fieldName, int fragCharSize)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          return searchClient.Highlight(indexName, query, documentId, fieldName, fragCharSize);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }

    }

    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.IndexDocument(indexName, documentId, document);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public void IndexJsonDocument(string indexName, string documentId, object docObject, object metadata, IEnumerable<FieldOptions> fieldOptions)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          var document = new JsonDocumentDto
            {
              DocumentId = documentId,
              DataAsJson = JsonConvert.SerializeObject(docObject, Formatting.Indented, SerializerSettings),
            };

          if (metadata != null)
            document.MetadataAsJson = JsonConvert.SerializeObject(metadata, Formatting.Indented, SerializerSettings);

          if (fieldOptions != null)
            document.FieldOptions = fieldOptions;

          searchClient.IndexJsonDocument(indexName, document);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.IndexJsonDocument(indexName, document);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.IndexJsonDocuments(indexName, documents);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public
      JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          return searchClient.Retrieve(indexName, documentId);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public IList<SearchResult> Search(string indexName, SearchArguments arguments)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          return searchClient.Search(indexName, arguments);
         
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public IList<FacetedSearchResult> FacetedSearch(string indexName, SearchArguments arguments)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          return searchClient.FacetedSearch(indexName, arguments);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }

    public void SetFieldOptions(string indexName, IEnumerable<FieldOptions> fieldOptions)
    {
      try
      {
        using (var searchClient = GetSearchClient())
        {
          searchClient.SetFieldOptions(indexName, fieldOptions);
        }
      }
      catch (CommunicationObjectFaultedException ex)
      {
        if (ex.InnerException != null)
          throw ex.InnerException;
        throw;
      }
    }
  }
}
