namespace Barista
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Text;

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
