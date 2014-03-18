namespace Barista.SharePoint.DocumentStore
{
  using System;
  using Barista.DocumentStore;
  using Microsoft.SharePoint;

  public class SPDocumentStoreUser : IUser
  {
    private readonly SPUser m_user;

    public SPDocumentStoreUser(SPUser user)
    {
      if (m_user == null)
        throw new ArgumentNullException("user");

      m_user = user;

      this.LoginName = m_user.LoginName;
      this.Email = m_user.Email;
      this.Name = m_user.Name;
    }

    public string LoginName
    {
      get;
      set;
    }

    public string Email
    {
      get;
      set;
    }

    public string Name
    {
      get;
      set;
    }
  }
}
