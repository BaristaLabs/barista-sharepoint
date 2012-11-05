namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.ServiceModel;
  using Barista.OrcaDB;
  using System.ServiceModel.Web;

  [ServiceContract(Namespace = Constants.ServiceV1Namespace)]
  public interface IDocumentStoreManagedPropertiesService
  {
    #region Managed Properties
    //[OperationContract]
    //[WebInvoke(Method = "POST", UriTemplate = "Container({containerTitle})/ManagedProperties?indexName={name}&jsonPath={jsonPath}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    //void CreateContainerManagedProperty(string containerTitle, string name, string jsonPath);

    //[OperationContract]
    //[WebInvoke(Method = "DELETE", UriTemplate = "Container({containerTitle})/ManagedProperty({name})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    //void DeleteContainerManagedProperty(string containerTitle, string name);


    //[OperationContract]
    //[WebGet(UriTemplate = "Container({containerTitle})/ManagedProperty({name})", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    //string GetContainerManagedProperty(string containerTitle, string name);

    //[OperationContract]
    //[WebGet(UriTemplate = "Container({containerTitle})/ManagedProperties", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
    //void ListContainerManagedProperties(string containerTitle);
    #endregion
  }
}
