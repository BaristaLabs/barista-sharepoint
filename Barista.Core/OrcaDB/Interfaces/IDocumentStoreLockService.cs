namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.OrcaDB;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreLockService
  {
    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/EntityLockStatus/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    Stream GetEntityLockStatusInPath(string containerTitle, string guid, string path);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/LockEntity({timeoutMs})/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void LockEntityInPath(string containerTitle, string guid, string path, string timeoutMs);

    [OperationContract]
    [WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/Entity({guid})/UnlockEntity/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void UnlockEntityInPath(string containerTitle, string guid, string path);

    [OperationContract]
    [WebGet(UriTemplate = "Container({containerTitle})/Entity({guid})/WaitForEntityLockRelease({timeoutMs})/{*path}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    void WaitForEntityLockReleaseInPath(string containerTitle, string guid, string path, string timeoutMs);
  }
}
