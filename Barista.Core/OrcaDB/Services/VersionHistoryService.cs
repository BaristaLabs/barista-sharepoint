namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using Newtonsoft.Json;
  using Barista.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreVersionHistoryService :
    DocumentStoreServiceBase,
    IDocumentStoreVersionHistoryService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListEntityVersions(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IVersionHistoryCapableDocumentStore documentStore = GetDocumentStore<IVersionHistoryCapableDocumentStore>();
      var result = documentStore.ListEntityVersions(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetEntityVersion(string containerTitle, string guid, string versionId)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IVersionHistoryCapableDocumentStore documentStore = GetDocumentStore<IVersionHistoryCapableDocumentStore>();
      var result = documentStore.GetEntityVersion(containerTitle, id, Int32.Parse(versionId));
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream RevertEntityToVersion(string containerTitle, string guid, string versionId)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IVersionHistoryCapableDocumentStore documentStore = GetDocumentStore<IVersionHistoryCapableDocumentStore>();
      var result = documentStore.RevertEntityToVersion(containerTitle, id, Int32.Parse(versionId));
      return GetJsonStream(result);
    }

    public void GetVersionHistoryForEntityPart(string containerName, string entityId, string entityPart)
    {
      throw new NotImplementedException();
    }
  }
}
