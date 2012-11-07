namespace Barista.DocumentStore
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
  public class DocumentStoreCommentsService :
    DocumentStoreServiceBase,
    IDocumentStoreCommentsService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream AddEntityComment(string containerTitle, string guid, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var comment = GetObjectFromJsonStream<string>(data);
      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.AddEntityComment(containerTitle, id, comment);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListEntityComments(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.ListEntityComments(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream AddEntityPartComment(string containerTitle, string guid, string partName, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var comment = GetObjectFromJsonStream<string>(data);
      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.AddEntityPartComment(containerTitle, id, partName, comment);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListEntityPartComments(string containerTitle, string guid, string partName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.ListEntityPartComments(containerTitle, id, partName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream AddAttachmentComment(string containerTitle, string guid, string fileName, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var comment = GetObjectFromJsonStream<string>(data);
      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.AddAttachmentComment(containerTitle, id, fileName, comment);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListAttachmentComments(string containerTitle, string guid, string fileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      ICommentCapableDocumentStore documentStore = GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.ListAttachmentComments(containerTitle, id, fileName);
      return GetJsonStream(result);
    }
  }
}
