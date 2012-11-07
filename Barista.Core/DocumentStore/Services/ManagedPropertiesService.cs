namespace Barista.DocumentStore
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
  public class DocumentStoreManagedPropertiesService :
    DocumentStoreServiceBase,
    IDocumentStoreManagedPropertiesService
  {
    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public string GetContainerManagedProperty(string containerTitle, string name)
    {
      throw new NotImplementedException();
    }

    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void CreateContainerManagedProperty(string containerTitle, string name, string jsonPath)
    {
      //Adds an indexed column to the document library, instructs the event receiver to update the index on Entity Create/Update
      throw new NotImplementedException();
    }

    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void DeleteContainerManagedProperty(string containerTitle, string name)
    {
      throw new NotImplementedException();
    }

    
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public void ListContainerManagedProperties(string containerTitle)
    {
      throw new NotImplementedException();
    }

    public void AddPromotedProperty()
    {
    }

    public void RemovePromotedProperty()
    {
    }

    public void ListPromotedProperties()
    {
    }

    public void AddExternalPromotedProperty()
    {
    }

    public void RemoveExternalPromotedProperty()
    {
    }

    public void ListExternalPromotedProperties()
    {
    }
  }
}
