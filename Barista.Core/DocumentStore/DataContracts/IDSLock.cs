namespace Barista.DocumentStore
{
  using System;

  public interface IDSLock
  {
    LockStatus GetLockStatus(string containerTitle, string path, Guid entityId);

    void Lock(string containerTitle, string path, Guid entityId, int? timeoutMs);

    void Unlock(string containerTitle, string path, Guid entityId);

    void WaitForLockRelease(string containerTitle, string path, Guid entityId, int? timeoutMs);
  }
}
