namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using OFS.OrcaDB.Core;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStorePermissionsService
  {
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddPrincipalRoleToAttachment(string containerTitle, string guid, string fileName, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddPrincipalRoleToContainer(string containerTitle, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddPrincipalRoleToEntity(string containerTitle, string guid, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddPrincipalRoleToEntityPart(string containerTitle, string guid, string partName, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle}/Entity({guid})/Attachment({fileName})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetAttachmentPermissions(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/?name={principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetAttachmentPermissionsForPrincipal(string containerTitle, string guid, string fileName, string principalName, string principalType);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetContainerPermissions(string containerTitle);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Permissions/{principalType}/?name={principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetContainerPermissionsForPrincipal(string containerTitle, string principalName, string principalType);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle}/Entity({guid})/EntityPart({partName})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityPartPermissions(string containerTitle, string guid, string partName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/?name={principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityPartPermissionsForPrincipal(string containerTitle, string guid, string partName, string principalName, string principalType);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle}/Entity({guid})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityPermissions(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Permissions/{principalType}/?name={principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityPermissionsForPrincipal(string containerTitle, string guid, string principalName, string principalType);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    bool RemovePrincipalRoleFromAttachment(string containerTitle, string guid, string fileName, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    bool RemovePrincipalRoleFromContainer(string containerTitle, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    bool RemovePrincipalRoleFromEntity(string containerTitle, string guid, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/{roleName}/{*principalName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    bool RemovePrincipalRoleFromEntityPart(string containerTitle, string guid, string partName, string principalName, string principalType, string roleName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ResetAttachmentPermissions(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ResetContainerPermissions(string containerTitle);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ResetEntityPermissions(string containerTitle, string guid);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ResetEntityPartPermissions(string containerTitle, string guid, string partName);
  }
}
