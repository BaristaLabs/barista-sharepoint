namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum Occur
  {
    [EnumMember]
    Must,
    [EnumMember]
    MustNot,
    [EnumMember]
    Should,
  }
}
