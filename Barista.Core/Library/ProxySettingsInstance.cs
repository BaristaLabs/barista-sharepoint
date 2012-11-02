namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;

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


  public class ProxySettingsInstance : ObjectInstance
  {
    public ProxySettingsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      //this.PopulateFunctions();
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
