namespace Barista.SharePoint.DocumentStore
{
  using Barista.DocumentStore;
  using System;

  public class SPDocumentStoreFolder : IFolder
  {

    #region Properties
    public string Name
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

    public string FullPath
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

    public int EntityCount
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

    public DateTime Modified
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

    public IUser ModifiedBy
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
    #endregion

    #region Methods

    public IEntity GetEntity(Guid id, bool recursive)
    {
    }

    public IFolder GetFolder(string path)
    {
    }

    #endregion
  }
}
