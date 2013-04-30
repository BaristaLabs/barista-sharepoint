namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;
  using System;

  [Serializable]
  public class SPProcessIdentityConstructor : ClrFunction
  {
    public SPProcessIdentityConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPProcessIdentity", new SPProcessIdentityInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPProcessIdentityInstance Construct()
    {
      return new SPProcessIdentityInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPProcessIdentityInstance : ObjectInstance
  {
    private readonly SPProcessIdentity m_processIdentity;

    public SPProcessIdentityInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPProcessIdentityInstance(ObjectInstance prototype, SPProcessIdentity processIdentity)
      : this(prototype)
    {
      if (processIdentity == null)
        throw new ArgumentNullException("processIdentity");

      m_processIdentity = processIdentity;
    }

    public SPProcessIdentity SPProcessIdentity
    {
      get { return m_processIdentity; }
    }

    [JSProperty(Name = "currentIdentityType")]
    public string CurrentIdentityType
    {
      get { return m_processIdentity.CurrentIdentityType.ToString(); }
    }

    [JSProperty(Name = "id")]
    public GuidInstance Id
    {
      get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_processIdentity.Id);}
    }

    [JSProperty(Name = "status")]
    public string Status
    {
      get { return m_processIdentity.Status.ToString(); }
    }

    [JSProperty(Name = "typeName")]
    public string TypeName
    {
      get { return m_processIdentity.TypeName; }
    }

    [JSProperty(Name = "username")]
    public string Username
    {
      get { return m_processIdentity.Username; }
    }
  }
}
