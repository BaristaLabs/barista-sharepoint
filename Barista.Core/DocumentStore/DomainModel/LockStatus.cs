namespace Barista.DocumentStore
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public enum LockStatus
  {
    [EnumMember]
    Unlocked = 0,
    [EnumMember]
    Locked = 1,
  }
}
