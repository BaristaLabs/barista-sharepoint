namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.OrcaDB;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreFoldersService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Folder/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetFolderByPath(string containerTitle, string path);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/FolderETag/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    string GetFolderETagByPath(string containerTitle, string path);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Folders/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream CreateContainerFolder(string containerTitle, string path);

    [OperationContract]
    [WebInvoke(Method = "PUT", UriTemplate = "Container({containerTitle})/Folders/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream RenameContainerFolder(string containerTitle, string path);

    [OperationContract]
    [WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/Folders/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void DeleteContainerFolder(string containerTitle, string path);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Folders", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListRootContainerFolders(string containerTitle);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Folders/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream ListContainerFolders(string containerTitle, string path);
  }
}
