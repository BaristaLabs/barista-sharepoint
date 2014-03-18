namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class PrincipalInstance : ObjectInstance, IPrincipal
  {
    private readonly IPrincipal m_principal;

    public PrincipalInstance(ScriptEngine engine, IPrincipal principal)
      : base(engine)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      m_principal = principal;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "type")]
    public string Type
    {
      get
      {
        if (m_principal is IUser)
          return "User";

        return "Group";
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_principal.Name; }
      set { m_principal.Name = value; }
    }

    [JSProperty(Name = "loginName")]
    public string LoginName
    {
      get
      {
        if (m_principal is IUser)
          return (m_principal as IUser).LoginName;

        return (m_principal as IGroup) == null
          ? ""
          : (m_principal as IGroup).LoginName;
      }
      set
      {
        if (m_principal is IUser)
          (m_principal as IUser).LoginName = value;
        else if (m_principal is IGroup)
          (m_principal as IGroup).LoginName = value;
      }
    }

    [JSProperty(Name = "email")]
    public string Email
    {
      get
      {
        if (m_principal is IUser)
          return (m_principal as IUser).Email;
        return (m_principal as IGroup) == null ? "" : (m_principal as IGroup).DistributionGroupEmail;
      }
      set
      {
        if (m_principal is IUser)
          (m_principal as IUser).Email = value;
        else if (m_principal is IGroup)
          (m_principal as IGroup).DistributionGroupEmail = value;
      }
    }
    
  }
}
