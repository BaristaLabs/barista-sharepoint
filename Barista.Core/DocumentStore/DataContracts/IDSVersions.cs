namespace Barista.DocumentStore
{
  using System.Collections.Generic;

  public interface IDSVersions
  {
    IList<IVersion> ListVersions();

    IVersion GetVersion(int versionId);

    IVersion RevertToVersion(int versionId);
  }
}
