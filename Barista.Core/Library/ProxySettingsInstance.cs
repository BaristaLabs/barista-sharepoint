namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Barista.Newtonsoft.Json;
  using System;

  [Serializable]
  public class ProxySettingsConstructor : ClrFunction
  {
    public ProxySettingsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ProxySettings", new ProxySettingsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ProxySettingsInstance Construct()
    {
      return new ProxySettingsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ProxySettingsInstance : ObjectInstance
  {
    public ProxySettingsInstance(ObjectInstance prototype)
      : base(prototype)
    {
        this.PopulateFunctions();
    }

    [JSProperty(Name = "address")]
    [JsonProperty("address")]
    public string Address
    {
      get;
      set;
    }

    [JSProperty(Name = "useDefaultCredentials")]
    [JsonProperty("useDefaultCredentials")]
    public bool UseDefaultCredentials
    {
      get;
      set;
    }
  }
}
