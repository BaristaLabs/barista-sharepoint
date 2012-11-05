namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using OFS.OrcaDB.Core;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreCommentsService
  {
    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddAttachmentComment(string containerTitle, string guid, string fileName, Stream comment);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddEntityComment(string containerTitle, string guid, Stream comment);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream AddEntityPartComment(string containerTitle, string guid, string partName, Stream comment);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Attachment({fileName})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListAttachmentComments(string containerTitle, string guid, string fileName);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityComments(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityPart({partName})/Comments", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityPartComments(string containerTitle, string guid, string partName);
  }
}
