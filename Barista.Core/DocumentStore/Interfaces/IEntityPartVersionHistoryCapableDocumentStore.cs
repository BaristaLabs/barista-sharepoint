using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
{
  public interface IEntityPartVersionHistoryCapableDocumentStore
  {
    IList<EntityPartVersion> ListEntityPartVersions(string containerTitle, Guid entityId, string partName);

    EntityPartVersion GetEntityPartVersion(string containerTitle, Guid entityId, string partName, int versionId);

    EntityPartVersion RevertEntityPartToVersion(string containerTitle, Guid entityId, string partName, int versionId);
  }
}
