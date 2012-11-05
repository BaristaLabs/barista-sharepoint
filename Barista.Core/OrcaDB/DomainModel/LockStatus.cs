namespace OFS.OrcaDB.Core
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public enum LockStatus : int
  {
    [EnumMember]
    Unlocked = 0,
    [EnumMember]
    Locked = 1,
  }
}
