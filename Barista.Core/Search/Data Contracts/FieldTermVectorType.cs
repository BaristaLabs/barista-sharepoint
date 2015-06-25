namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum FieldTermVectorType
  {
    [EnumMember]
    Yes,
    [EnumMember]
    WithPositions,
    [EnumMember]
    WithOffsets,
    [EnumMember]
    WithPositionsOffsets,
    [EnumMember]
    No,
  }
}
