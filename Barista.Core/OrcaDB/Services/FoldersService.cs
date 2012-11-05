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
  public class DocumentStoreFoldersService :
    DocumentStoreServiceBase,
    IDocumentStoreFoldersService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetFolderByPath(string containerTitle, string path)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.GetFolder(containerTitle, path);

      return GetJsonStream(result);
    }
    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual string GetFolderETagByPath(string containerTitle, string path)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

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
      return documentStore.GetFolderETag(containerTitle, path, criteria);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream CreateContainerFolder(string containerTitle, string path)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (DocumentStoreHelper.IsValidPath(path) == false)
        throw new ArgumentOutOfRangeException("path");

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result =  documentStore.CreateFolder(containerTitle, path);

      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream RenameContainerFolder(string containerTitle, string path)
    {
      var newFolderName = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters["newFolderName"];

      if (String.IsNullOrEmpty(newFolderName))
        throw new ArgumentNullException("newFolderName");

      if (DocumentStoreHelper.IsValidPath(path) == false)
        throw new ArgumentOutOfRangeException("path");

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.RenameFolder(containerTitle, path, newFolderName);

      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void DeleteContainerFolder(string containerTitle, string path)
    {
      if (DocumentStoreHelper.IsValidPath(path) == false)
        throw new ArgumentOutOfRangeException("path");

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();
      documentStore.DeleteFolder(containerTitle, path);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListRootContainerFolders(string containerTitle)
    {
      return ListContainerFolders(containerTitle, String.Empty);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream ListContainerFolders(string containerTitle, string path)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      bool listAllFolders = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(qp => qp.ToLowerInvariant() == "allfolders");

      IFolderCapableDocumentStore documentStore = GetDocumentStore<IFolderCapableDocumentStore>();

      IList<Folder> result = null;
      if (listAllFolders)
      {
        result = documentStore.ListAllFolders(containerTitle, path);
      }
      else
      {
        result = documentStore.ListFolders(containerTitle, path);
      }

      return GetJsonStream(result);
    }
  }
}
