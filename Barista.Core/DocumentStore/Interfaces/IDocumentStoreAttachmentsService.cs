namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.DocumentStore;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreAttachmentsService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetAttachment(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream UploadAttachment(string containerTitle, string guid, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachments?sourceUrl={sourceUrl}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream UploadAttachmentFromSourceUrl(string containerTitle, string guid, string sourceUrl);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})?newFileName={newFileName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void RenameAttachment(string containerTitle, string guid, string fileName, string newFileName);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void DeleteAttachment(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListAttachments(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachments/{fileName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream DownloadAttachmentDirect(string containerTitle, string guid, string fileName);
  }
}
