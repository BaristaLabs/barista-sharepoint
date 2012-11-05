using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core
{
  public interface IVersionHistoryCapableDocumentStore
  {
    IList<EntityVersion> ListEntityVersions(string containerTitle, Guid entityId);

    EntityVersion GetEntityVersion(string containerTitle, Guid entityId, int versionId);

    EntityVersion RevertEntityToVersion(string containerTitle, Guid entityId, int versionId);
  }
}
