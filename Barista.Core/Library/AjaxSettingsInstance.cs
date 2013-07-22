namespace Barista.Library
{
  using System.Net;
  using Barista.Newtonsoft.Json.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Barista.Newtonsoft.Json;
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
    private ICredentials m_credentials;

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

    [JSDoc("Gets or sets the credentials associated with the ajax request. Ex. { credentials: new NetworkCredential(...) }")]
    [JSProperty(Name = "credentials")]
    [JsonProperty("credentials")]
    public object Credentials
    {
      get
      {
        if (m_credentials is NetworkCredential)
          return new NetworkCredentialInstance(this.Engine.Object.InstancePrototype,
                                               m_credentials as NetworkCredential);
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          return;

        if (value is JContainer)
          value = JSONObject.Parse(this.Engine, (value as JContainer).ToString(), null);

        if (value is NetworkCredentialInstance)
          m_credentials = (value as NetworkCredentialInstance).NetworkCredential;
        else if (value is ObjectInstance)
          m_credentials = JurassicHelper.Coerce<NetworkCredentialInstance>(this.Engine, value).NetworkCredential;
      }
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

    [JSProperty(Name = "dataType")]
    [JsonProperty("dataType")]
    public string DataType
    {
      get;
      set;
    }
  }
}
