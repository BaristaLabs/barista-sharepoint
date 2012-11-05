namespace OFS.OrcaDB.Core
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
  using OFS.OrcaDB.Core.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreMetadataService :
    DocumentStoreServiceBase,
    IDocumentStoreMetadataService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public string GetContainerMetadata(string containerTitle, string key)
    {
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      return documentStore.GetContainerMetadata(containerTitle, key);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public string GetEntityMetadata(string containerTitle, string guid, string key)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      return documentStore.GetEntityMetadata(containerTitle, id, key);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public string GetEntityPartMetadata(string containerTitle, string guid, string partName, string key)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      return documentStore.GetEntityPartMetadata(containerTitle, id, partName, key);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public string GetAttachmentMetadata(string containerTitle, string guid, string fileName, string key)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      return documentStore.GetAttachmentMetadata(containerTitle, id, fileName, key);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void SetContainerMetadata(string containerTitle, string key, Stream data)
    {
      var value = GetObjectFromJsonStream<string>(data);
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      documentStore.SetContainerMetadata(containerTitle, key, value);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void SetEntityMetadata(string containerTitle, string guid, string key, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      var value = GetObjectFromJsonStream<string>(data);
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      documentStore.SetEntityMetadata(containerTitle, id, key, value);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void SetEntityPartMetadata(string containerTitle, string guid, string partName, string key, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      var value = GetObjectFromJsonStream<string>(data);
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      documentStore.SetEntityPartMetadata(containerTitle, id, partName, key, value);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void SetAttachmentMetadata(string containerTitle, string guid, string fileName, string key, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      var value = GetObjectFromJsonStream<string>(data);
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      documentStore.SetAttachmentMetadata(containerTitle, id, fileName, key, value);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListContainerMetadata(string containerTitle)
    {
      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      var result = documentStore.ListContainerMetadata(containerTitle);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListEntityMetadata(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      var result = documentStore.ListEntityMetadata(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListEntityPartMetadata(string containerTitle, string guid, string partName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      var result = documentStore.ListEntityPartMetadata(containerTitle, id, partName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListAttachmentMetadata(string containerTitle, string guid, string fileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IMetadataCapableDocumentStore documentStore = GetDocumentStore<IMetadataCapableDocumentStore>();
      var result = documentStore.ListAttachmentMetadata(containerTitle, id, fileName);
      return GetJsonStream(result);
    }
  }
}
