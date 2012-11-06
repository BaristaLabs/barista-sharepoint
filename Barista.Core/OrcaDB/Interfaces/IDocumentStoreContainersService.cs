namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.OrcaDB;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreContainersService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetContainer(string containerTitle);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Containers/?title={containerTitle}&description={description}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream CreateContainer(string containerTitle, string description);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Containers/", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void UpdateContainer(Stream data);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void DeleteContainer(string containerTitle);

    [OperationContract]
    [WebGet(UriTemplate = "Containers", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListContainers();
  }
}
