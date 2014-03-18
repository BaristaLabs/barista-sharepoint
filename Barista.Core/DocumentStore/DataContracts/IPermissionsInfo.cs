namespace Barista.DocumentStore
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  public interface IPermissionsInfo
  {
    [DataMember]
    bool HasUniqueRoleAssignments
    {
      get;
      set;
    }

    [DataMember]
    IList<IPrincipalRoleInfo> Principals
    {
      get;
      set;
    }
  }
}