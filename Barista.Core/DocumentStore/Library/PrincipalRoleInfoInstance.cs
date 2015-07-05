namespace Barista.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Linq;

  [Serializable]
  public class PrincipalRoleInfoInstance : ObjectInstance
  {
    PrincipalInstance m_principalInstance;
    ArrayInstance m_roles;

    public PrincipalRoleInfoInstance(ScriptEngine engine, PrincipalRoleInfo principalRoleInfo)
      : base(engine)
    {
      if (principalRoleInfo == null)
        throw new ArgumentNullException("principalRoleInfo");

      m_principalInstance = new PrincipalInstance(Engine, principalRoleInfo.Principal);

      m_roles = Engine.Array.Construct(principalRoleInfo.Roles.Select( r => new RoleInstance(Engine, r)));

      PopulateFields();
      PopulateFunctions();
    }

    [JSProperty(Name = "principal")]
    public PrincipalInstance Principal
    {
      get { return m_principalInstance; }
      set { m_principalInstance = value; }
    }

    [JSProperty(Name = "roles")]
    [JSDoc("ternPropertyType", "[+Role]")]
    public ArrayInstance Roles
    {
      get { return m_roles; }
      set { m_roles = value; }
    }
  }
}
