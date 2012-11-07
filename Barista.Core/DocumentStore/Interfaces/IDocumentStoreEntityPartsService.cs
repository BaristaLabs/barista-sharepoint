namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.DocumentStore;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreEntityPartsService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityPart(string containerTitle, string guid, string partName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPartETag({partName})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetEntityPartETag(string containerTitle, string guid, string partName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityParts?partName={partName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream CreateEntityPart(string containerTitle, string guid, string partName, Stream data);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})?newPartName={newPartName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void RenameEntityPart(string containerTitle, string guid, string partName, string newPartName);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityParts/", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void UpdateEntityPart(string containerTitle, string guid, Stream entityPart);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void DeleteEntityPart(string containerTitle, string guid, string partName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityParts", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityParts(string containerTitle, string guid);
  }
}
