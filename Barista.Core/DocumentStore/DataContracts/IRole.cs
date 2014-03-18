namespace Barista.DocumentStore
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  public interface IRole : IPrincipal
  {
    [DataMember]
    string Description
    {
      get;
      set;
    }

    [DataMember]
    int Order
    {
      get;
      set;
    }

    [DataMember]
    IList<string> BasePermissions
    {
      get;
      set;
    }
  }
}