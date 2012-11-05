namespace Barista.OrcaDB
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Comment : DSObject
  {
    [DataMember]
    public int Id
    {
      get;
      set;
    }

    [DataMember]
    public string CommentText
    {
      get;
      set;
    }
  }
}
