namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPPrincipalConstructor : ClrFunction
  {
    public SPPrincipalConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPPrincipal", new SPPrincipalInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPPrincipalInstance Construct()
    {
      return new SPPrincipalInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPPrincipalInstance : ObjectInstance
  {
    private readonly SPPrincipal m_principal;

    public SPPrincipalInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPPrincipalInstance(ObjectInstance prototype, SPPrincipal principal)
      : this(prototype)
    {
      if (principal == null)
        throw new ArgumentNullException("principal");

      m_principal = principal;
    }

    public SPPrincipal SPPrincipal
    {
      get { return m_principal; }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get
      {
        return m_principal.ID;
      }
    }

    [JSProperty(Name = "loginName")]
    public string LoginName
    {
      get
      {
        return m_principal.LoginName;
      }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get
      {
        return m_principal.Name;
      }
    }

    [JSProperty(Name = "parentWeb")]
    public SPWebInstance ParentWeb
    {
      get
      {
        return m_principal.ParentWeb == null
          ? null
          : new SPWebInstance(this.Engine, m_principal.ParentWeb);
      }
    }
    

    //Roles Property is Deprecated.
  }
}
