namespace Barista.DocumentStore.Library
{
  using System.Collections.Generic;
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Linq;

  [Serializable]
  public class PrincipalRoleInfoInstance : ObjectInstance, IPrincipalRoleInfo
  {
    private readonly IPrincipalRoleInfo m_principalRoleInfo;

    public PrincipalRoleInfoInstance(ScriptEngine engine, IPrincipalRoleInfo principalRoleInfo)
      : base(engine)
    {
      if (principalRoleInfo == null)
        throw new ArgumentNullException("principalRoleInfo");

      m_principalRoleInfo = principalRoleInfo;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IPrincipalRoleInfo PrincipalRoleInfo
    {
      get { return m_principalRoleInfo; }
    }

    [JSProperty(Name = "principal")]
    public PrincipalInstance Principal
    {
      get { return new PrincipalInstance(this.Engine, m_principalRoleInfo.Principal); }
      set { m_principalRoleInfo.Principal = value; }
    }

    IPrincipal IPrincipalRoleInfo.Principal
    {
      get { return m_principalRoleInfo.Principal; }
      set { m_principalRoleInfo.Principal = value; }
    }

    [JSProperty(Name = "roles")]
    public ArrayInstance Roles
    {
      get { return this.Engine.Array.Construct(m_principalRoleInfo.Roles.Select( r => new RoleInstance(this.Engine, r))); }
      set
      {
        if (value == null)
        {
          m_principalRoleInfo.Roles = null;
          return;
        }

        m_principalRoleInfo.Roles = value.ElementValues.OfType<IRole>().ToList();
      }
    }

    IList<IRole> IPrincipalRoleInfo.Roles
    {
      get { return m_principalRoleInfo.Roles; }
      set { m_principalRoleInfo.Roles = value; }
    }
  }
}
