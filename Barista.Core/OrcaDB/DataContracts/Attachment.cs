namespace OFS.OrcaDB.Core
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Constants.ServiceV1Namespace)]
  public class Attachment : DSEditableObject
  {
    [DataMember]
    public string FileName
    {
      get;
      set;
    }

    [DataMember]
    public string Category
    {
      get;
      set;
    }

    [DataMember]
    public string Path
    {
      get;
      set;
    }

    [DataMember]
    public DateTime TimeLastModified
    {
      get;
      set;
    }

    [DataMember]
    public string ETag
    {
      get;
      set;
    }

    [DataMember]
    public string MimeType
    {
      get;
      set;
    }

    [DataMember]
    public long Size
    {
      get;
      set;
    }

    [DataMember]
    public string Url
    {
      get;
      set;
    }
  }
}
