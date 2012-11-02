namespace Barista.SharePoint
{
  using System.Runtime.Serialization;

  [DataContract]
  public sealed class InfoPathAttachment
  {
    [DataMember]
    public string FileName
    {
      get;
      set;
    }

    [DataMember]
    public string FileDisplayName
    {
      get;
      set;
    }

    [DataMember]
    public string Description
    {
      get;
      set;
    }

    [DataMember]
    public byte[] Data
    {
      get;
      set;
    }
  }
}