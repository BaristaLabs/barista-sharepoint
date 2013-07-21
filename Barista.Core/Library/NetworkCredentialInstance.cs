namespace Barista.Library
{
  using System.Net;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Newtonsoft.Json;

  [Serializable]
  public class NetworkCredentialConstructor : ClrFunction
  {
    public NetworkCredentialConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "NetworkCredential", new NetworkCredentialInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public NetworkCredentialInstance Construct()
    {
      return new NetworkCredentialInstance(this.InstancePrototype, new NetworkCredential());
    }
  }

  [Serializable]
  public class NetworkCredentialInstance : ObjectInstance
  {
    private readonly NetworkCredential m_networkCredential;

    public NetworkCredentialInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public NetworkCredentialInstance(ObjectInstance prototype, NetworkCredential networkCredential)
      : this(prototype)
    {
      if (networkCredential == null)
        throw new ArgumentNullException("networkCredential");

      m_networkCredential = networkCredential;
    }

    public NetworkCredential NetworkCredential
    {
      get { return m_networkCredential; }
    }

    [JSProperty(Name = "domain")]
    [JsonProperty("domain")]
    public string Domain
    {
      get { return m_networkCredential.Domain; }
      set { m_networkCredential.Domain = value; }
    }

    [JSProperty(Name = "password")]
    [JsonProperty("password")]
    public string Password
    {
      get { return m_networkCredential.Password; }
      set { m_networkCredential.Password = value; }
    }

    [JSProperty(Name = "userName")]
    [JsonProperty("userName")]
    public string UserName
    {
      get { return m_networkCredential.UserName; }
      set { m_networkCredential.UserName = value; }
    }
  }
}
