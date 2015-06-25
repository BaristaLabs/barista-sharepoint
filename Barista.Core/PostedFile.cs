namespace Barista
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceNamespace)]
  [Serializable]
  public class PostedFile
  {
    [DataMember]
    public byte[] Content
    {
      get;
      set;
    }

    [DataMember]
    public long ContentLength
    {
      get;
      set;
    }

    [DataMember]
    public string ContentType
    {
      get;
      set;
    }
    
    [DataMember]
    public string FileName
    {
      get;
      set;
    }

  }
}
