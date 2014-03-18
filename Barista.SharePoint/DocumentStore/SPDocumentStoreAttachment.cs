namespace Barista.SharePoint.DocumentStore
{
  using System;
  using Barista.DocumentStore;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;

  /// <summary>
  /// Represents an attachment whose backing store is a SPFile.
  /// </summary>
  public class SPDocumentStoreAttachment : IAttachment
  {
    private SPFile m_attachmentFile;

    public SPDocumentStoreAttachment(SPFile file)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      m_attachmentFile = file;

      this.Category = file.Item["Category"] as string;
      this.Path = file.Item["Path"] as string;
      this.ETag = file.ETag;
      this.FileName = file.Name;
      this.MimeType = StringHelper.GetMimeTypeFromFileName(file.Name);
      this.Size = file.Length;
      this.Url = SPUtility.ConcatUrls(file.Web.Url, file.Url);
      this.Created = ((DateTime) file.Item[SPBuiltInFieldId.Created]).ToLocalTime();
      this.Modified = ((DateTime) file.Item[SPBuiltInFieldId.Modified]).ToLocalTime();

      var createdByUser = file.Item[SPBuiltInFieldId.Created_x0020_By] as SPFieldUserValue;
      if (createdByUser != null)
      {
        this.CreatedBy = new SPDocumentStoreUser(createdByUser.User);
      }

      var modifiedByUser = file.Item[SPBuiltInFieldId.Modified_x0020_By] as SPFieldUserValue;
      if (modifiedByUser != null)
      {
        this.ModifiedBy = new SPDocumentStoreUser(modifiedByUser.User);
      }
    }

    public string FileName
    {
      get;
      set;
    }

    public string Category
    {
      get;
      set;
    }

    public string Path
    {
      get;
      set;
    }

    public DateTime TimeLastModified
    {
      get;
      set;
    }

    public string ETag
    {
      get;
      set;
    }

    public string MimeType
    {
      get;
      set;
    }

    public long Size
    {
      get;
      set;
    }

    public string Url
    {
      get;
      set;
    }

    public DateTime Modified
    {
      get;
      set;
    }

    public IUser ModifiedBy
    {
      get;
      set;
    }

    public DateTime Created
    {
      get;
      set;
    }

    public IUser CreatedBy
    {
      get;
      set;
    }
  }
}
