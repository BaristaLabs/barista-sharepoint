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
  public class DocumentStoreEntityPartsService :
    DocumentStoreServiceBase,
    IDocumentStoreEntityPartsService
  {
    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetEntityPart(string containerTitle, string guid, string partName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      var result = documentStore.GetEntityPart(containerTitle, id, partName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual string GetEntityPartETag(string containerTitle, string guid, string partName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      return documentStore.GetEntityPartETag(containerTitle, id, partName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream CreateEntityPart(string containerTitle, string guid, string partName, Stream data)
    {
      string category = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["category"];

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var entityPartData = GetObjectFromJsonStream<string>(data);
      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      var result = documentStore.CreateEntityPart(containerTitle, id, partName, category, entityPartData);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void RenameEntityPart(string containerTitle, string guid, string partName, string newPartName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      documentStore.RenameEntityPart(containerTitle, id, partName, newPartName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void UpdateEntityPart(string containerTitle, string guid, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      string newCategory = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["category"];

      var entityPart = GetObjectFromJsonStream<EntityPart>(data);
      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      documentStore.UpdateEntityPart(containerTitle, id, entityPart);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void DeleteEntityPart(string containerTitle, string guid, string partName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      documentStore.DeleteEntityPart(containerTitle, id, partName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListEntityParts(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IEntityPartCapableDocumentStore documentStore = GetDocumentStore<IEntityPartCapableDocumentStore>();
      var result = documentStore.ListEntityParts(containerTitle, id);
      return GetJsonStream(result);
    }
  }
}
