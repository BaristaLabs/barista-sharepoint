namespace Barista.SharePoint.DocumentStore
{
  using System.Linq;
  using Barista.DocumentStore;
  using System;
  using Microsoft.SharePoint;

  public class SPDocumentStoreComment : IComment
  {
    private readonly SPListItemVersion m_listItemVersion;

    public SPDocumentStoreComment(SPListItem listItem)
    {
      if (listItem == null)
        throw new ArgumentNullException("listItem");

      m_listItemVersion = listItem
        .Versions
        .OfType<SPListItemVersion>()
        .OrderByDescending(v => v.Created).First();
    }

    public SPDocumentStoreComment(SPListItemVersion listItemVersion)
    {
      m_listItemVersion = listItemVersion;

      this.Id = listItemVersion.VersionId;
      this.CommentText = listItemVersion["Comments"] as string;
      this.Created = listItemVersion.Created.ToLocalTime();
      this.CreatedBy = new SPDocumentStoreUser(listItemVersion.CreatedBy.User);
    }

    public int Id
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public string CommentText
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public DateTime Created
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    public IUser CreatedBy
    {
      get
      {
        throw new NotImplementedException();
      }
      set
      {
        throw new NotImplementedException();
      }
    }
  }
}
