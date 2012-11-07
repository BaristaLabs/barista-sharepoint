namespace Barista.SharePoint.DocumentStore
{
  using Microsoft.SharePoint.Client.Services;
  using Barista.DocumentStore;
  
  [BasicHttpBindingServiceMetadataExchangeEndpointAttribute]
  public class SPDocumentStoreEntitiesService :
    DocumentStoreEntitiesService, 
    IDocumentStoreEntitiesService
  {
  }
}
