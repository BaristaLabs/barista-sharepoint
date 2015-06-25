namespace Barista.Library
{
  using Barista.Helpers;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class UserAgentConstructor : ClrFunction
  {
    public UserAgentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "UserAgent", new UserAgentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public UserAgentInstance Construct()
    {
      return new UserAgentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class UserAgentInstance : ObjectInstance
  {
    private readonly UserAgent m_userAgent;

    public UserAgentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public UserAgentInstance(ObjectInstance prototype, UserAgent userAgent)
      : this(prototype)
    {
      if (userAgent == null)
        throw new ArgumentNullException("userAgent");

      m_userAgent = userAgent;
    }

    public UserAgent UserAgent
    {
      get { return m_userAgent; }
    }

    [JSProperty(Name = "family")]
    public string Family
    {
      get { return m_userAgent.Family; }
    }

    [JSProperty(Name = "major")]
    public string Major
    {
      get { return m_userAgent.Major; }
    }

    [JSProperty(Name = "minor")]
    public string Minor
    {
      get { return m_userAgent.Minor; }
    }

    [JSProperty(Name = "patch")]
    public string Patch
    {
      get { return m_userAgent.Patch; }
    }
  }
}
