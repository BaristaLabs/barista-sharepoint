namespace Barista.OrcaDB
{
  using System;

  public interface ILockCapableDocumentStore
  {
    LockStatus GetEntityLockStatus(string containerTitle, string path, Guid entityId);

    void LockEntity(string containerTitle, string path, Guid entityId, int? timeoutMs = 60000);

    void UnlockEntity(string containerTitle, string path, Guid entityId);

    void WaitForEntityLockRelease(string containerTitle, string path, Guid entityId, int? timeoutMs = 60000);
  }
}
