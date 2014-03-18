namespace Barista.SharePoint.DocumentStore
{
  using System;
  using Barista.DocumentStore;
  using Microsoft.SharePoint;

  /// <summary>
  /// Represents a container that is the physical embodiment of an SPList.
  /// </summary>
  public class SPDocumentStoreContainer : IContainer
  {
    private readonly SPList m_list;

    public SPDocumentStoreContainer(SPList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      m_list = list;
      this.Id = list.ID;
      this.Title = list.Title;
      this.Description = list.Description;
      this.EntityCount = list.ItemCount;
      this.Url = list.RootFolder.Url.Substring(list.RootFolder.Url.IndexOf('/') + 1);
      this.Created = list.Created.ToLocalTime();
      this.CreatedBy = new SPDocumentStoreUser(list.Author);
      this.Modified = (DateTime) list.RootFolder.Properties["vti_timelastmodified"];
      //TODO: ModifiedBy using the last item added to the list.
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

    public string Url
    {
      get;
      set;
    }

    public int EntityCount
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

    public IFolder RootFolder
    {
      get;
      set;
    }

    #endregion
  }
}