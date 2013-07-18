namespace Barista.Library
{
  using Barista.Helpers;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class ClientInfoConstructor : ClrFunction
  {
    public ClientInfoConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ClientInfo", new ClientInfoInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ClientInfoInstance Construct()
    {
      return new ClientInfoInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ClientInfoInstance : ObjectInstance
  {
    private readonly ClientInfo m_clientInfo;

    public ClientInfoInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ClientInfoInstance(ObjectInstance prototype, ClientInfo clientInfo)
      : this(prototype)
    {
      if (clientInfo == null)
        throw new ArgumentNullException("clientInfo");

      m_clientInfo = clientInfo;
    }

    public ClientInfo ClientInfo
    {
      get { return m_clientInfo; }
    }

    [JSProperty(Name = "device")]
    public DeviceInstance Device
    {
      get { return new DeviceInstance(this.Engine.Object.InstancePrototype, m_clientInfo.Device); }
    }

    [JSProperty(Name = "os")]
    public OSInstance OS
    {
      get { return new OSInstance(this.Engine.Object.InstancePrototype, m_clientInfo.OS); }
    }

    [JSProperty(Name = "userAgent")]
    public UserAgentInstance UserAgent
    {
      get { return new UserAgentInstance(this.Engine.Object.InstancePrototype, m_clientInfo.UserAgent); }
    }
  }
}
