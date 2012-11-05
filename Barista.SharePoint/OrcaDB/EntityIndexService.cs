namespace Barista.SharePoint.OrcaDB
{
  using System;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using Microsoft.SharePoint.Client.Services;
  using Barista.OrcaDB;
  using Barista.Framework;
  using Microsoft.SharePoint;

  [BasicHttpBindingServiceMetadataExchangeEndpoint]
  [ServiceContract(Namespace = Constants.ServiceNamespace)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class EntityIndexService
  {
    [OperationContract(IsOneWay = true)]
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [WebGet(BodyStyle= WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json)]
    [DynamicResponseType]
    public void UpdateEntityIndex(string factoryTypeName, Guid siteId, Guid webId, Guid entityId, string entityNamespace, string path, bool rebuildIndexIfNotExists, bool persistIndexChanges)
    {
      Type factoryType = Type.GetType(factoryTypeName, true, true);
      if (factoryType.GetInterfaces().Any(i => i == typeof(IRepositoryFactory)) == false)
        throw new InvalidOperationException("The factory type name was specified, but the corresponding type does not implement IRepositoryFactory");

      var factory = Activator.CreateInstance(factoryType) as IRepositoryFactory;

      using (SPSite site = new SPSite(siteId))
      {
        using (SPWeb web = site.OpenWeb(webId))
        {
          SPDocumentStore documentStore = new SPDocumentStore(web);
          using(var repository = Repository.GetRepository(factory, documentStore))
          {
            repository.UpdateEntityIndexes(entityId, entityNamespace, path, rebuildIndexIfNotExists, updateAsync: false);
          }
        }
      }
    }
  }
}
