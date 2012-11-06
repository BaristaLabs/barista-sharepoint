namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using Newtonsoft.Json;
  using Barista.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreContainersService :
    DocumentStoreServiceBase,
    IDocumentStoreContainersService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream GetContainer(string containerTitle)
    {
      IDocumentStore documentStore = GetDocumentStore();
      var result = documentStore.GetContainer(containerTitle);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream CreateContainer(string containerTitle, string description)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentException("The title value must contain text.", containerTitle);

      IDocumentStore documentStore = GetDocumentStore();
      var result = documentStore.CreateContainer(containerTitle, description);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void UpdateContainer(Stream data)
    {
      var container = this.GetObjectFromJsonStream<Container>(data);

      IDocumentStore documentStore = GetDocumentStore();
      documentStore.UpdateContainer(container);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void DeleteContainer(string containerTitle)
    {
      if (String.IsNullOrEmpty(containerTitle))
        throw new ArgumentNullException("containerTitle");

      IDocumentStore documentStore = GetDocumentStore();
      documentStore.DeleteContainer(containerTitle);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public Stream ListContainers()
    {
      IDocumentStore documentStore = GetDocumentStore();
      var result = documentStore.ListContainers();
      return this.GetJsonStream(result);
    }
  }
}
