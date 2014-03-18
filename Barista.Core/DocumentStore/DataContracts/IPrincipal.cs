namespace Barista.DocumentStore
{
  using System.Runtime.Serialization;

  public interface IPrincipal
  {
    [DataMember]
    string Name
    {
      get;
      set;
    }
  }
}
