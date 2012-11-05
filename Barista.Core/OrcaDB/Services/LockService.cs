namespace Barista.OrcaDB
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Net;
  using System.ServiceModel;
  using System.ServiceModel.Activation;
  using System.ServiceModel.Web;
  using System.Web;
  using Barista.Framework;

  /// <summary>
  /// Represents a Service which serves as a REST-based service wrapper around an instance of a Document Store.
  /// </summary>
  [SilverlightFaultBehavior]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  [RawJsonRequestBehavior]
  [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
  public class DocumentStoreLockService :
    DocumentStoreServiceBase,
    IDocumentStoreLockService
  {
    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual Stream GetEntityLockStatusInPath(string containerTitle, string guid, string path)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return GetJsonStream(LockStatus.Unlocked);

      ILockCapableDocumentStore documentStore = GetDocumentStore<ILockCapableDocumentStore>();
      var result = documentStore.GetEntityLockStatus(containerTitle, path, id);
      return GetJsonStream(result);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void LockEntityInPath(string containerTitle, string guid, string path, string timeoutMs)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      int? timeoutToUse = null;
      int timeout;
      if (int.TryParse(timeoutMs, out timeout))
        timeoutToUse = timeout;

      ILockCapableDocumentStore documentStore = GetDocumentStore<ILockCapableDocumentStore>();
      documentStore.LockEntity(containerTitle, path, id, timeoutToUse);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void UnlockEntityInPath(string containerTitle, string guid, string path)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      ILockCapableDocumentStore documentStore = GetDocumentStore<ILockCapableDocumentStore>();
      documentStore.UnlockEntity(containerTitle, path, id);
    }

    [OperationBehavior(Impersonation = ImpersonationOption.Allowed)]
    [DynamicResponseType]
    public virtual void WaitForEntityLockReleaseInPath(string containerTitle, string guid, string path, string timeoutMs)
    {
      Guid id;
      if (TryParseEntityGuid(guid, out id) == false)
        return;

      ILockCapableDocumentStore documentStore = GetDocumentStore<ILockCapableDocumentStore>();

      int timeout;
      if (Int32.TryParse(timeoutMs, out timeout))
        documentStore.WaitForEntityLockRelease(containerTitle, path, id, timeout);
      else
        documentStore.WaitForEntityLockRelease(containerTitle, path, id);
    }
  }
}
