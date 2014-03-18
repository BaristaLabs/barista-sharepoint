namespace Barista.DocumentStore
{
  using System.Runtime.Serialization;

  public interface IUser : IPrincipal
  {
    [DataMember]
    string LoginName
    {
      get;
      set;
    }

    [DataMember]
    string Email
    {
      get;
      set;
    }
  }
}