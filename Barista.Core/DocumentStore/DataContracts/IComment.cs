namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  public interface IComment
  {
    [DataMember]
    int Id
    {
      get;
      set;
    }

    [DataMember]
    string CommentText
    {
      get;
      set;
    }

    [DataMember]
    DateTime Created
    {
      get;
      set;
    }

    [DataMember]
    IUser CreatedBy
    {
      get;
      set;
    }
  }
}