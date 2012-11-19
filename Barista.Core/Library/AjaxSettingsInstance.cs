namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;

  [Serializable]
  public class AjaxSettingsConstructor : ClrFunction
  {
    public AjaxSettingsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "AjaxSettings", new AjaxSettingsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public AjaxSettingsInstance Construct()
    {
      return new AjaxSettingsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class AjaxSettingsInstance : ObjectInstance
  {
    public AjaxSettingsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.Async = false;

      this.PopulateFields();
      //this.PopulateFunctions();
    }

    [JSProperty(Name = "accept")]
    [JsonProperty("accept")]
    public string Accept
    {
      get;
      set;
    }

    [JSProperty(Name = "async")]
    [JsonProperty("async")]
    public bool Async
    {
      get;
      set;
    }

    [JSProperty(Name = "username")]
    [JsonProperty("username")]
    public string Username
    {
      get;
      set;
    }

    [JSProperty(Name = "password")]
    [JsonProperty("password")]
    public string Password
    {
      get;
      set;
    }

    [JSProperty(Name = "domain")]
    [JsonProperty("domain")]
    public string Domain
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

    [JSProperty(Name = "proxy")]
    [JsonProperty("proxy")]
    public object Proxy
    {
      get;
      set;
    }
  }
}
