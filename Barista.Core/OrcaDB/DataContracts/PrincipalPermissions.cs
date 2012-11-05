namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  /// <summary>
  /// Represents a class associated with permissions on a container, entity or attachment.
  /// </summary>
  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class PermissionsInfo
  {
    [DataMember]
    public bool HasUniqueRoleAssignments
    {
      get;
      set;
    }

    [DataMember]
    public IList<PrincipalRoleInfo> Principals
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Describes a principal defined on a list item and and the roles associated with it.
  /// </summary>
  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class PrincipalRoleInfo
  {
    [DataMember]
    public Principal Principal
    {
      get;
      set;
    }

    [DataMember]
    public IList<Role> Roles
    {
      get;
      set;
    }
  }


  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Role
  {
    [DataMember]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    public int Order
    {
      get;
      set;
    }

    [DataMember]
    public IList<string> BasePermissions
    {
      get;
      set;
    }
  }
}
