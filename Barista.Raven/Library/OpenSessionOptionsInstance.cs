using RavenDB = Raven.Client;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Barista.Newtonsoft.Json;
  using System.Net;

  [Serializable]
  public class OpenSessionsOptionsConstructor : ClrFunction
  {
    public OpenSessionsOptionsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "OpenSessionsOptions", new OpenSessionsOptionsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public OpenSessionsOptionsInstance Construct()
    {
      return new OpenSessionsOptionsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class OpenSessionsOptionsInstance : ObjectInstance
  {
    private readonly RavenDB.Document.OpenSessionOptions m_openSessionOptions;

    public OpenSessionsOptionsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public OpenSessionsOptionsInstance(ObjectInstance prototype, RavenDB.Document.OpenSessionOptions openSessionOptions)
      : this(prototype)
    {
      if (openSessionOptions == null)
        throw new ArgumentNullException("openSessionOptions");

      m_openSessionOptions = openSessionOptions;
    }

    public RavenDB.Document.OpenSessionOptions OpenSessionOptions
    {
      get { return m_openSessionOptions; }
    }

    [JSProperty(Name = "credentials")]
    [JsonProperty("credentials")]
    public object Credentials
    {
      get
      {
        if (m_openSessionOptions.Credentials is NetworkCredential)
          return new NetworkCredentialInstance(this.Engine.Object.InstancePrototype,
                                               m_openSessionOptions.Credentials as NetworkCredential);
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          return;

        if (value is NetworkCredentialInstance)
          m_openSessionOptions.Credentials = (value as NetworkCredentialInstance).NetworkCredential;
        else if (value is ObjectInstance)
          m_openSessionOptions.Credentials = JurassicHelper.Coerce<NetworkCredentialInstance>(this.Engine, value).NetworkCredential;

      }
    }


    [JSProperty(Name = "database")]
    [JsonProperty("database")]
    public string Database
    {
      get { return m_openSessionOptions.Database; }
      set { m_openSessionOptions.Database = value; }
    }

    [JSProperty(Name = "forceReadFromMaster")]
    [JsonProperty("forceReadFromMaster")]
    public bool ForceReadFromMaster
    {
      get { return m_openSessionOptions.ForceReadFromMaster; }
      set { m_openSessionOptions.ForceReadFromMaster = value; }
    }
  }
}
