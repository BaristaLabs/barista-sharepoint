namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum Occur
  {
    [EnumMember]
    Must = 0,
    [EnumMember]
    Should = 1,
    [EnumMember]
    MustNot = 2,
  }
}
