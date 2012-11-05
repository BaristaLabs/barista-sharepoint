namespace OFS.OrcaDB.Core
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Web;
  using OFS.OrcaDB.Core.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreEntitiesService :
    DocumentStoreServiceBase,
    IDocumentStoreEntitiesService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetEntityByGuid(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IDocumentStore documentStore = GetDocumentStore();
      var result = documentStore.GetEntity(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetEntityByGuidInPath(string containerTitle, string guid, string path)
    {
      if (!Uri.IsWellFormedUriString(HttpUtility.UrlEncode(path), UriKind.Relative))
        throw new ArgumentException("Path Argument is Invalid. Must be a relative path.", "path");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
     
      var result = documentStore.GetEntity(containerTitle, id, path);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual string GetEntityETagByGuid(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IDocumentStore documentStore = GetDocumentStore();
      return documentStore.GetEntityContentsETag(containerTitle, id);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ExportEntityByGuid(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IDocumentStore documentStore = GetDocumentStore();
      Entity entity = documentStore.GetEntity(containerTitle, id);
      Stream stream = documentStore.ExportEntity(containerTitle, id);

      WebOperationContext.Current.OutgoingResponse.LastModified = entity.Modified;
      WebOperationContext.Current.OutgoingResponse.ContentType = "application/zip";
      WebOperationContext.Current.OutgoingResponse.ContentLength = stream.Length;

      return stream;
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ImportEntity(string containerTitle, string guid, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var @namespace = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["namespace"];

      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      byte[] fileContents = null;
      if (WebOperationContext.Current.IncomingRequest.ContentType == null)
      {
        fileContents = data.ReadToEnd();
      }
      else
      {
        if (WebOperationContext.Current.IncomingRequest.ContentType.StartsWith("multipart/form-data"))
        {
          MultipartParser mpParser = new MultipartParser(data);
          fileContents = mpParser.FileContents;
        }
        else
        {
          OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
          response.StatusCode = HttpStatusCode.BadRequest;
          response.StatusDescription = "No multi-part, single part or sourceUrl was specified. At least one must be specified as the contents of the attachment.";
          return null;
        }
      }

      IDocumentStore documentStore = GetDocumentStore();
      var result = documentStore.ImportEntity(containerTitle, id, @namespace, fileContents);
      return GetJsonStream(result);
    }
    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ImportEntityIntoPath(string containerTitle, string path, string guid, Stream data)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      var @namespace = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["namespace"];

      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      byte[] fileContents = null;
      if (WebOperationContext.Current.IncomingRequest.ContentType == null)
      {
        fileContents = data.ReadToEnd();
      }
      else
      {
        if (WebOperationContext.Current.IncomingRequest.ContentType.StartsWith("multipart/form-data"))
        {
          MultipartParser mpParser = new MultipartParser(data);
          fileContents = mpParser.FileContents;
        }
        else
        {
          OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
          response.StatusCode = HttpStatusCode.BadRequest;
          response.StatusDescription = "No multi-part, single part or sourceUrl was specified. At least one must be specified as the contents of the attachment.";
          return null;
        }
      }

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.ImportEntity(containerTitle, path, id, @namespace, fileContents);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream CreateEntity(string containerTitle, Stream data)
    {
      return CreateEntityInFolder(containerTitle, String.Empty, data);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream CreateEntityInFolder(string containerTitle, string path, Stream data)
    {
      if (!Uri.IsWellFormedUriString(HttpUtility.UrlEncode(path), UriKind.Relative))
        throw new ArgumentException("Path Argument is Invalid. Must be a relative path.", "path");

      var @namespace = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["namespace"];

      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      var entityData = GetObjectFromJsonStream<string>(data);

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.CreateEntity(containerTitle, path, @namespace, entityData);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void UpdateEntity(string containerTitle, Stream data)
    {
      var entity = GetObjectFromJsonStream<Entity>(data);
      IDocumentStore documentStore = GetDocumentStore();
      documentStore.UpdateEntity(containerTitle, entity);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void DeleteEntity(string containerTitle, string guid)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IDocumentStore documentStore = GetDocumentStore();
      documentStore.DeleteEntity(containerTitle, id);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListEntities(string containerTitle)
    {
      return ListEntitiesInFolder(containerTitle, string.Empty);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListEntitiesInFolder(string containerTitle, string path)
    {
      if (!Uri.IsWellFormedUriString(HttpUtility.UrlEncode(path), UriKind.Relative))
        throw new ArgumentException("Path Argument is Invalid. Must be a relative path.", path);

      EntityFilterCriteria criteria = new EntityFilterCriteria();

      if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$namespace"))
      {
        criteria.Namespace = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["$namespace"];
      }

      if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$top"))
      {
        uint top;
        if (uint.TryParse(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["$top"], out top))
          criteria.Top = top;
      }

      if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$skip"))
      {
        uint skip;
        if (uint.TryParse(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["$skip"], out skip))
          criteria.Skip = skip;
      }

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.ListEntities(containerTitle, path, criteria);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void MoveEntityToFolder(string containerTitle, string guid, string destinationPath)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      documentStore.MoveEntity(containerTitle, id, destinationPath);
    }
  }
}
