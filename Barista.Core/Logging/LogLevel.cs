namespace Barista.Logging
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  public enum LogLevel
  {
    [EnumMember]
    Trace = 0,
    [EnumMember]
    Debug = 1,
    [EnumMember]
    Information = 2,
    [EnumMember]
    Warning = 3,
    [EnumMember]
    Error = 4,
    [EnumMember]
    Fatal = 5,
    [EnumMember]
    CatastrophicFailure = 6, //Do Not Use. ;)
  }
}
