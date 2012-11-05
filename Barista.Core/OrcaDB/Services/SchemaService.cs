namespace OFS.OrcaDB.Core
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
  using OFS.OrcaDB.Core.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreSchemaService :
    DocumentStoreServiceBase
  {
    public void DefineSchemaForNamespace(string containerName, string @namespace, SchemaOptions schemaOptions, string schema)
    {
      throw new NotImplementedException();
    }

    public void DefineSchemaForNamespaceEntityPart(string containerName, string @namespace, string partName, SchemaOptions schemaOptions, string schema)
    {
      throw new NotImplementedException();
    }
  }
}
