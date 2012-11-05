namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using OFS.OrcaDB.Core;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreEntitiesService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityByGuid(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityByGuidInPath(string containerTitle, string guid, string path);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/EntityContentsETag({guid})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetEntityETagByGuid(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/ExportEntity", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ExportEntityByGuid(string containerTitle, string guid);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/ImportEntity", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ImportEntity(string containerTitle, string guid, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/ImportEntity/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ImportEntityIntoPath(string containerTitle, string path, string guid, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entities/", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream CreateEntity(string containerTitle, Stream data);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entities/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream CreateEntityInFolder(string containerTitle, string path, Stream data);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Container({containerTitle})/Entities/", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void UpdateEntity(string containerTitle, Stream data);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Entity({guid})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void DeleteEntity(string containerTitle, string guid);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entities/", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntities(string containerTitle);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entities/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListEntitiesInFolder(string containerTitle, string path);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/MoveEntity?destination={destinationPath}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void MoveEntityToFolder(string containerTitle, string guid, string destinationPath);
  }
}
