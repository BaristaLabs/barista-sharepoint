namespace Barista.DocumentStore
{
  using System;
  using System.Runtime.Serialization;

  public interface IAttachment : IDSObject, IDSComments, IDSMetadata, IDSPermissions
  {
    [DataMember]
    string FileName
    {
      get;
      set;
    }

    [DataMember]
    string Category
    {
      get;
      set;
    }

    [DataMember]
    string Path
    {
      get;
      set;
    }

    [DataMember]
    DateTime TimeLastModified
    {
      get;
      set;
    }

    [DataMember]
    string ETag
    {
      get;
      set;
    }

    [DataMember]
    string MimeType
    {
      get;
      set;
    }

    [DataMember]
    long Size
    {
      get;
      set;
    }

    [DataMember]
    string Url
    {
      get;
      set;
    }
  }
}