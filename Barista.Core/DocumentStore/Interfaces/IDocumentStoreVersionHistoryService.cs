namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.DocumentStore;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreVersionHistoryService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Versions/{versionId}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityVersion(string containerTitle, string guid, string versionId);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/Versions", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntityVersions(string containerTitle, string guid);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/Versions/RevertTo?version={versionId}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream RevertEntityToVersion(string containerTitle, string guid, string versionId);
  }
}
