namespace Barista.OrcaDB
{
  using System;

  public interface ILockCapableDocumentStore
  {
    LockStatus GetEntityLockStatus(string containerTitle, string path, Guid entityId);

    void LockEntity(string containerTitle, string path, Guid entityId, int? timeoutMs);

    void UnlockEntity(string containerTitle, string path, Guid entityId);

    void WaitForEntityLockRelease(string containerTitle, string path, Guid entityId, int? timeoutMs);
  }
}
