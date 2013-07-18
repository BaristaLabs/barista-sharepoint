namespace Barista.Library
{
  using Barista.Helpers;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class DeviceConstructor : ClrFunction
  {
    public DeviceConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Device", new DeviceInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DeviceInstance Construct()
    {
      return new DeviceInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class DeviceInstance : ObjectInstance
  {
    private readonly Device m_device;

    public DeviceInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DeviceInstance(ObjectInstance prototype, Device device)
      : this(prototype)
    {
      if (device == null)
        throw new ArgumentNullException("device");

      m_device = device;
    }

    public Device Device
    {
      get { return m_device; }
    }

    [JSProperty(Name = "family")]
    public string Family
    {
      get { return m_device.Family; }
    }

    [JSProperty(Name = "isSpider")]
    public bool IsSpider
    {
      get { return m_device.IsSpider; }
    }
  }
}
