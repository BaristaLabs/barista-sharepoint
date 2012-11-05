namespace Barista.SharePoint.OrcaDB
{
  using Microsoft.SharePoint.Client.Services;
  using Barista.OrcaDB;
  
  [BasicHttpBindingServiceMetadataExchangeEndpointAttribute]
  public class SPDocumentStoreVersionHistoryService :
    DocumentStoreVersionHistoryService, 
    IDocumentStoreVersionHistoryService
  {
  }
}
