namespace Barista.DocumentStore
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  public interface IPrincipalRoleInfo
  {
    [DataMember]
    IPrincipal Principal
    {
      get;
      set;
    }

    [DataMember]
    IList<IRole> Roles
    {
      get;
      set;
    }
  }
}