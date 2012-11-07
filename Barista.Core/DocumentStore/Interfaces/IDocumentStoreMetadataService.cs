namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.DocumentStore;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreMetadataService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetAttachmentMetadata(string containerTitle, string guid, string fileName, string key);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetContainerMetadata(string containerTitle, string key);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetEntityMetadata(string containerTitle, string guid, string key);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetEntityPartMetadata(string containerTitle, string guid, string partName, string key);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Metadata", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListAttachmentMetadata(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Metadata", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListContainerMetadata(string containerTitle);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Metadata", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityMetadata(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Metadata", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityPartMetadata(string containerTitle, string guid, string partName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void SetAttachmentMetadata(string containerTitle, string guid, string fileName, string key, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void SetContainerMetadata(string containerTitle, string key, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void SetEntityMetadata(string containerTitle, string guid, string key, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Metadata({key})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void SetEntityPartMetadata(string containerTitle, string guid, string partName, string key, Stream data);
  }
}
