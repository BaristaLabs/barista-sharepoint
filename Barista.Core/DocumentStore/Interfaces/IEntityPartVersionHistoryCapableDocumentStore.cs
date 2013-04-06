namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;

  public interface IEntityPartVersionHistoryCapableDocumentStore
  {
    IList<EntityPartVersion> ListEntityPartVersions(string containerTitle, Guid entityId, string partName);

    EntityPartVersion GetEntityPartVersion(string containerTitle, Guid entityId, string partName, int versionId);

    EntityPartVersion RevertEntityPartToVersion(string containerTitle, Guid entityId, string partName, int versionId);
  }
}
