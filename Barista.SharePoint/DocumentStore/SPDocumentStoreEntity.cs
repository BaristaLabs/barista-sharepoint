namespace Barista.SharePoint.DocumentStore
{
  using System.Collections.Generic;
  using Barista.DocumentStore;
  using Microsoft.Office.DocumentManagement.DocumentSets;
  using Microsoft.SharePoint;
  using System;
  using System.Text;
  using Microsoft.SharePoint.ApplicationPages.Calendar.Exchange;

  public class SPDocumentStoreEntity : IEntity
  {
    public SPDocumentStoreEntity(DocumentSet documentSet, string data) : this(documentSet, null, data)
    {
    }

    /// <summary>
    /// Returns an entity that represents the specified document set
    /// </summary>
    /// <param name="documentSet">The entity document set.</param>
    /// <param name="file">The file that represents the default entity part. If no file is supplied, and no data is supplied, the data value of the entity will be null.</param>
    /// <param name="data">Optionally, the data within the default entity part. If a null string is provided, the file's contents will be retrieved.</param>
    /// <returns></returns>
    public SPDocumentStoreEntity(DocumentSet documentSet, SPFile file, string data)
    {
      if (documentSet == null)
        throw new ArgumentNullException("documentSet",
                                        @"When creating an SPDocumentStoreEntity, the document set that represents the entity must not be null.");

      try
      {
        var id = documentSet.Item[Constants.DocumentEntityGuidFieldId] as string;

        if (id != null)
          this.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      this.Namespace = documentSet.Item[Constants.NamespaceFieldId] as string;

      this.Title = documentSet.Item.Title;
      this.Description = documentSet.Item["DocumentSetDescription"] as string;
      this.Created = ((DateTime)documentSet.Item[SPBuiltInFieldId.Created]).ToLocalTime();
      this.Modified = ((DateTime)documentSet.Item[SPBuiltInFieldId.Modified]).ToLocalTime();

      this.ContentsETag = documentSet.Item["DocumentEntityContentsHash"] as string;

      if (documentSet.Item["DocumentEntityContentsLastModified"] != null)
      {
        this.ContentsModified = (DateTime)documentSet.Item["DocumentEntityContentsLastModified"];
      }

      this.Path = documentSet.ParentFolder.Url.Substring(documentSet.ParentList.RootFolder.Url.Length);
      this.Path = this.Path.TrimStart('/');

      if (data != null)
      {
        this.ETag = StringHelper.CreateMD5Hash(data);
        this.Data = data;
      }
      else
      {
        if (file != null && file.Exists)
        {
          this.ETag = file.ETag;
          this.Data = Encoding.UTF8.GetString(file.OpenBinary());
        }
      }

      var createdByUserValue = documentSet.Item[SPBuiltInFieldId.Author] as String;
      var createdByUser = new SPFieldUserValue(documentSet.Folder.ParentWeb, createdByUserValue);

      if (createdByUser.User != null)
      {
        this.CreatedBy = new SPDocumentStoreUser(createdByUser.User);
      }

      var modifiedByUserValue = documentSet.Item[SPBuiltInFieldId.Editor] as String;
      var modifiedByUser = new SPFieldUserValue(documentSet.Folder.ParentWeb, modifiedByUserValue);

      if (modifiedByUser.User != null)
      {
        this.ModifiedBy = new SPDocumentStoreUser(modifiedByUser.User);
      }
    }

    #region Properties

    public Guid Id
    {
      get;
      set;
    }

    public string Title
    {
      get;
      set;
    }

    public string Description
    {
      get;
      set;
    }

    public string Namespace
    {
      get;
      set;
    }

    public string ETag
    {
      get;
      set;
    }

    public string ContentsETag
    {
      get;
      set;
    }

    public DateTime ContentsModified
    {
      get;
      set;
    }

    public string Path
    {
      get;
      set;
    }

    public string Data
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

    #endregion

    #region Methods

    public IDictionary<string, string> GetMetadata()
    {
    }

    public IList<IEntityPart> ListEntityParts()
    {
    }

    public IEntityPart GetEntityPart(string partName)
    {
    }

    public IList<IEntityPartVersion> GetVersions()
    {
    }

    #endregion
  }
}
