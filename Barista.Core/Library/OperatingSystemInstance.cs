namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class OperatingSystemConstructor : ClrFunction
  {
    public OperatingSystemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "OperatingSystem", new OperatingSystemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public OperatingSystemInstance Construct()
    {
      return new OperatingSystemInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class OperatingSystemInstance : ObjectInstance
  {
    private readonly OperatingSystem m_operatingSystem;

    public OperatingSystemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public OperatingSystemInstance(ObjectInstance prototype, OperatingSystem operatingSystem)
      : this(prototype)
    {
      if (operatingSystem == null)
        throw new ArgumentNullException("operatingSystem");

      m_operatingSystem = operatingSystem;
    }

    public OperatingSystem OperatingSystem
    {
      get { return m_operatingSystem; }
    }

    [JSProperty(Name = "platform")]
    public string Platform
    {
      get
      {
        return m_operatingSystem.Platform.ToString();
      }
    }

    [JSProperty(Name = "servicePack")]
    public string ServicePack
    {
      get
      {
        return m_operatingSystem.ServicePack;
      }
    }

    [JSProperty(Name = "version")]
    public string Version
    {
      get
      {
        return m_operatingSystem.Version.ToString();
      }
    }

    [JSProperty(Name = "versionString")]
    public string VersionString
    {
      get
      {
        return m_operatingSystem.VersionString;
      }
    }
  }
}
