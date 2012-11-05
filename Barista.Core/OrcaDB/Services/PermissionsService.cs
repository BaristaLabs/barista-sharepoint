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
  public class DocumentStorePermissionsService :
    DocumentStoreServiceBase,
    IDocumentStorePermissionsService
  {
    #region Container Permissions

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream AddPrincipalRoleToContainer(string containerTitle, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.AddPrincipalRoleToContainer(containerTitle, principalName, principalType, roleName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public bool RemovePrincipalRoleFromContainer(string containerTitle, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      return documentStore.RemovePrincipalRoleFromContainer(containerTitle, principalName, principalType, roleName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetContainerPermissions(string containerTitle)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetContainerPermissions(containerTitle);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetContainerPermissionsForPrincipal(string containerTitle, string principalName, string principalType)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetContainerPermissionsForPrincipal(containerTitle, principalName, principalType);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ResetContainerPermissions(string containerTitle)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.ResetContainerPermissions(containerTitle);
      return GetJsonStream(result);
    }
    #endregion

    #region Entity Permissions

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream AddPrincipalRoleToEntity(string containerTitle, string guid, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      //bool doSystemUpdate = false;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$doSystemUpdate"))
      //{
      //  doSystemUpdate = true;
      //}

      //bool eventFiringEnabled = true;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$disableEventFiring"))
      //{
      //  eventFiringEnabled = false;
      //}

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.AddPrincipalRoleToEntity(containerTitle, id, principalName, principalType, roleName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public bool RemovePrincipalRoleFromEntity(string containerTitle, string guid, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return false;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      return documentStore.RemovePrincipalRoleFromEntity(containerTitle, id, principalName, principalType, roleName);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetEntityPermissions(string containerTitle, string guid)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetEntityPermissions(containerTitle, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetEntityPermissionsForPrincipal(string containerTitle, string guid, string principalName, string principalType)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetEntityPermissionsForPrincipal(containerTitle, id, principalName, principalType);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ResetEntityPermissions(string containerTitle, string guid)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.ResetEntityPermissions(containerTitle, id);
      return GetJsonStream(result);
    }
    #endregion

    #region Entity Part Permissions

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream AddPrincipalRoleToEntityPart(string containerTitle, string guid, string partName, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      //bool doSystemUpdate = false;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$doSystemUpdate"))
      //{
      //  doSystemUpdate = true;
      //}

      //bool eventFiringEnabled = true;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$disableEventFiring"))
      //{
      //  eventFiringEnabled = false;
      //}

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.AddPrincipalRoleToEntityPart(containerTitle, id, partName, principalName, principalType, roleName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public bool RemovePrincipalRoleFromEntityPart(string containerTitle, string guid, string partName, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return false;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      return documentStore.RemovePrincipalRoleFromEntityPart(containerTitle, id, partName, principalName, principalType, roleName);
    }


    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetEntityPartPermissions(string containerTitle, string guid, string partName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetEntityPartPermissions(containerTitle, id, partName);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetEntityPartPermissionsForPrincipal(string containerTitle, string guid, string partName, string principalName, string principalType)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetEntityPartPermissionsForPrincipal(containerTitle, id, partName, principalName, principalType);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ResetEntityPartPermissions(string containerTitle, string guid, string partName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.ResetEntityPartPermissions(containerTitle, id, partName);
      return GetJsonStream(result);
    }
    #endregion

    #region Attachment Permissions

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream AddPrincipalRoleToAttachment(string containerTitle, string guid, string fileName, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      //bool doSystemUpdate = false;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$doSystemUpdate"))
      //{
      //  doSystemUpdate = true;
      //}

      //bool eventFiringEnabled = true;
      //if (WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters.AllKeys.Any(k => k == "$disableEventFiring"))
      //{
      //  eventFiringEnabled = false;
      //}

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.AddPrincipalRoleToAttachment(containerTitle, id, fileName, principalName, principalType, roleName);
      return GetJsonStream(result);
    }


    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public bool RemovePrincipalRoleFromAttachment(string containerTitle, string guid, string fileName, string principalName, string principalType, string roleName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      if (String.IsNullOrEmpty(roleName))
        throw new InvalidOperationException("roleName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return false;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      return documentStore.RemovePrincipalRoleFromAttachment(containerTitle, id, fileName, principalName, principalType, roleName);
    }


    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetAttachmentPermissions(string containerTitle, string guid, string fileName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetAttachmentPermissions(containerTitle, id, fileName);
      return GetJsonStream(result);
    }


    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetAttachmentPermissionsForPrincipal(string containerTitle, string guid, string fileName, string principalName, string principalType)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      if (String.IsNullOrEmpty(principalName))
        throw new ArgumentNullException("principalName");

      if (String.IsNullOrEmpty(principalType))
        throw new ArgumentNullException("principalType");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      principalName = principalName.Replace('/', '\\');

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.GetAttachmentPermissionsForPrincipal(containerTitle, id, fileName, principalName, principalType);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ResetAttachmentPermissions(string containerTitle, string guid, string fileName)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      if (String.IsNullOrEmpty(fileName))
        throw new ArgumentNullException("fileName");

      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return null;

      IPermissionsCapableDocumentStore documentStore = GetDocumentStore<IPermissionsCapableDocumentStore>();
      var result = documentStore.ResetAttachmentPermissions(containerTitle, id, fileName);
      return GetJsonStream(result);
    }
    #endregion
  }
}
