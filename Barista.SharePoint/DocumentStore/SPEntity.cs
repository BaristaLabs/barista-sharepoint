namespace Barista.SharePoint.DocumentStore
{
  using System;
  using System.Linq;
  using Microsoft.SharePoint;
  using Barista.DocumentStore;
  using Microsoft.Office.DocumentManagement.DocumentSets;
  using System.IO;
  using System.Collections.Generic;
  using System.Web;

  /// <summary>
  /// Represents a SharePoint based entity. Defers loading/serialization until properties are accessed.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SPEntity : Entity
  {
    private object m_syncRoot = new object();
    private SPListItem m_listItem = null;
    private bool m_hasDataBeenSet = false;

    public SPEntity(SPListItem listItem)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem", "When creating an SPEntity, the SPListItem that represents the entity must not be null.");

      m_listItem = listItem;
      MapListItemToProperties();
    }

    internal SPListItem ListItem
    {
      get { return m_listItem; }
    }

    public override string Data
    {
      get
      {
        if (base.Data == null && m_hasDataBeenSet == false)
        {
          lock (m_syncRoot)
          {
            if (base.Data == null && m_hasDataBeenSet == false)
            {
              var url = HttpUtility.UrlPathEncode(m_listItem.Web.Url + "/" +m_listItem.Folder.Url + "/" + Constants.DocumentStoreDefaultEntityPartFileName);
              using (var site = new SPSite(m_listItem.Web.Site.ID))
              {
                using (var web = site.OpenWeb(m_listItem.Web.ID))
                {
                  base.Data = web.GetFileAsString(url);
                }
              }
            }
          }
        }

        return base.Data;
      }
      set
      {
         base.Data = value;
        m_hasDataBeenSet = true;
      }
    }

    private void MapListItemToProperties()
    {
      try
      {
        string id = m_listItem[Constants.DocumentEntityGuidFieldId] as string;

        this.Id = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      this.Namespace = m_listItem[Constants.NamespaceFieldId] as string;

      var docSet = DocumentSet.GetDocumentSet(m_listItem.Folder);

      SPFile dataFile = null;

      try
      {
        dataFile = m_listItem.Web.GetFile(m_listItem.Folder.Url + "/" + Constants.DocumentStoreDefaultEntityPartFileName);
      }
      catch (Exception) { /* Do Nothing... */ };

      if (dataFile == null) //The default entity part file doesn't exist, get outta dodge (something happened here...)
        throw new InvalidOperationException("No correpsonding entity file exists on the SP Doc Set that represents the entity.");


      this.ETag = dataFile.ETag;
      this.Title = docSet.Item.Title;
      this.Description = docSet.Item["DocumentSetDescription"] as string;
      this.Created = (DateTime)docSet.Item[SPBuiltInFieldId.Created];
      this.Modified = (DateTime)docSet.Item[SPBuiltInFieldId.Modified];

      var latestFile = m_listItem.Folder.Files.OfType<SPFile>().OrderByDescending(f => f.TimeLastModified).FirstOrDefault();
      var combinedETag = String.Join(", ", m_listItem.Folder.Files.OfType<SPFile>().Select(f => f.ETag).ToArray());
      this.ContentsETag = StringHelper.CreateMD5Hash(combinedETag);
      this.ContentsModified = latestFile.TimeLastModified;

      this.Path = docSet.ParentFolder.Url.Substring(m_listItem.ParentList.RootFolder.Url.Length);
      this.Path = this.Path.TrimStart('/');

      var createdByUserValue = m_listItem[SPBuiltInFieldId.Author] as String;
      SPFieldUserValue createdByUser = new SPFieldUserValue(m_listItem.Web, createdByUserValue);

      if (createdByUser != null)
      {
        this.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = m_listItem[SPBuiltInFieldId.Editor] as String;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(m_listItem.Web, createdByUserValue);

      if (modifiedByUser != null)
      {
        this.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }
    }
  }

  /// <summary>
  /// Represents a strongly-typed entity that wraps SPEntity for lazy-loading goodness.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SPEntity<T> : Entity<T>
  {
    private object m_syncRoot = new object();

    SPEntity m_entity = null;
    private bool m_hasValueBeenSet = false;

    public SPEntity(SPEntity entity)
      : base()
    {
      this.ContentsETag = entity.ContentsETag;
      this.ContentsModified = entity.ContentsModified;
      this.Created = entity.Created;
      this.CreatedBy = entity.CreatedBy;
      this.Description = entity.Description;
      this.ETag = entity.ETag;
      this.Id = entity.Id;
      this.Modified = entity.Modified;
      this.ModifiedBy = entity.ModifiedBy;
      this.Namespace = entity.Namespace;
      this.Title = entity.Title;
      this.Path = entity.Path;

      m_entity = entity;
    }

    public override string Data
    {
      get
      {
        if (m_hasValueBeenSet)
          return base.Data;
        else
          return m_entity.Data;
      }
      set
      {
        if (m_hasValueBeenSet)
          base.Data = value;
        else
          m_entity.Data = value;
      }
    }

    public override T Value
    {
      get
      {
        if (EqualityComparer<T>.Default.Equals(base.Value, default(T)) && m_hasValueBeenSet == false)
        {
          lock (m_syncRoot)
          {
            if (EqualityComparer<T>.Default.Equals(base.Value, default(T)) && m_hasValueBeenSet == false)
            {
              base.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(m_entity.Data);
            }
          }
        }

        return base.Value;
      }
      set
      {
        base.Value = value;
        m_hasValueBeenSet = true;
      }
    }
  }
}
