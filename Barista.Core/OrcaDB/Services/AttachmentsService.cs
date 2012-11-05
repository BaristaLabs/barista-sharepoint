namespace OFS.OrcaDB.Core
{
  using System;
  using System.IO;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using OFS.OrcaDB.Core.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreAttachmentsService :
    DocumentStoreServiceBase,
    IDocumentStoreAttachmentsService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetAttachment(string containerTitle, string guid, string fileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      var result = documentStore.GetAttachment(containerTitle, id, fileName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream UploadAttachmentFromSourceUrl(string containerTitle, string guid, string sourceUrl)
    {
      Uri sourceUri = null;
      if (Uri.TryCreate(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["sourceUrl"], UriKind.Absolute, out sourceUri) == false)
      {
        OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
        response.StatusCode = HttpStatusCode.BadRequest;
        response.StatusDescription = "Invalid SourceUrl Parameter.";
      }

      string fileName = sourceUri.Segments[sourceUri.Segments.Length - 1];
      string fileNameOverride = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["fileName"];
      if (String.IsNullOrEmpty(fileNameOverride) == false)
        fileName = fileNameOverride;

      string category = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["category"];
      string path = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["path"];

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      var result = documentStore.UploadAttachmentFromSourceUrl(containerTitle, id, fileName, sourceUrl, category, path);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream UploadAttachment(string containerTitle, string guid, Stream data)
    {
      string fileName;
      byte[] fileContents = null;
      string category = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["category"];
      string path = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["path"];

      if (WebOperationContext.Current.IncomingRequest.ContentType == null)
      {
        string fileNameParameter = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["fileName"];
        if (string.IsNullOrEmpty(fileNameParameter))
        {
          OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
          response.StatusCode = HttpStatusCode.BadRequest;
          response.StatusDescription = "If the data is not multipart/form-data, a filename must be specified as a query string parameter.";
          return null;
        }

        fileName = fileNameParameter;
        fileContents = data.ReadToEnd();
      }
      else
      {
        if (WebOperationContext.Current.IncomingRequest.ContentType.StartsWith("multipart/form-data"))
        {
          MultipartParser mpParser = new MultipartParser(data);
          fileContents = mpParser.FileContents;
          fileName = mpParser.Filename;
        }
        else
        {
           OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
           response.StatusCode = HttpStatusCode.BadRequest;
           response.StatusDescription = "No multi-part, single part or sourceUrl was specified. At least one must be specified as the contents of the attachment.";
           return null;
        }
      }

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      var result = documentStore.UploadAttachment(containerTitle, id, fileName, fileContents, category, path);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void RenameAttachment(string containerTitle, string guid, string fileName, string newFileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      documentStore.RenameAttachment(containerTitle, id, fileName, newFileName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void DeleteAttachment(string containerTitle, string guid, string fileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      documentStore.DeleteAttachment(containerTitle, id, fileName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListAttachments(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      var result = documentStore.ListAttachments(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream DownloadAttachmentDirect(string containerTitle, string guid, string fileName)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IAttachmentCapableDocumentStore documentStore = GetDocumentStore<IAttachmentCapableDocumentStore>();
      Attachment attachment = documentStore.GetAttachment(containerTitle, id, fileName);
      Stream attachmentStream = documentStore.DownloadAttachment(containerTitle, id, fileName);

      WebOperationContext.Current.OutgoingResponse.LastModified = attachment.TimeLastModified;
      WebOperationContext.Current.OutgoingResponse.ContentType = DocumentStoreHelper.GetMimeTypeFromFileName(attachment.FileName);
      WebOperationContext.Current.OutgoingResponse.ContentLength = attachmentStream.Length;
      WebOperationContext.Current.OutgoingResponse.ETag = attachment.ETag;

      return attachmentStream;
    }

  }
}
