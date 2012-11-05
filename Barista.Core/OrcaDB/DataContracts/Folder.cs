namespace OFS.OrcaDB.Core
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Folder : DSEditableObject
  {
    [DataMember]
    public string Name
    {
      get;
      set;
    }

    [DataMember]
    public string FullPath
    {
      get;
      set;
    }

    [DataMember]
    public int EntityCount
    {
      get;
      set;
    }
  }
}
