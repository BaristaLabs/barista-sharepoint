using RavenDB = Raven.Client;

namespace Barista.Raven.Library
{
  using System.Net;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using System;
  using Barista.Newtonsoft.Json;
  using global::Raven.Client.Document;
  using Barista.Newtonsoft.Json.Linq;

  [Serializable]
  public class DocumentStoreConstructor : ClrFunction
  {
    public DocumentStoreConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "DocumentStore", new DocumentStoreInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DocumentStoreInstance Construct(object creationInfo)
    {
      if (creationInfo != null && creationInfo != Null.Value && creationInfo != Undefined.Value &&
          creationInfo is ObjectInstance)
      {
        return JurassicHelper.Coerce<DocumentStoreInstance>(this.Engine, creationInfo);
      }
      else
        return new DocumentStoreInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class DocumentStoreInstance : ObjectInstance
  {
    private readonly RavenDB.Document.DocumentStore m_documentStore = new DocumentStore();

    public DocumentStoreInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DocumentStoreInstance(ObjectInstance prototype, RavenDB.Document.DocumentStore documentStore)
      : this(prototype)
    {
      if (documentStore == null)
        throw new ArgumentNullException("documentStore");

      m_documentStore = documentStore;
    }

    public RavenDB.Document.DocumentStore DocumentStore
    {
      get { return m_documentStore; }
    }

    [JSProperty(Name = "apiKey")]
    [JSDoc("Gets or sets the API key to use when authenticating against a RavenDB server that supports API Key Authentication.")]
    public string ApiKey
    {
      get { return m_documentStore.ApiKey; }
      set { m_documentStore.ApiKey = value; }
    }

    [JSProperty(Name = "credentials")]
    [JsonProperty("credentials")]
    [JSDoc("Gets or sets the credentials associated with the document store. Ex. ds.credentials = new NetworkCredential(...)")]
    public object Credentials
    {
      get
      {
        if (m_documentStore.Credentials is NetworkCredential)
          return new NetworkCredentialInstance(this.Engine.Object.InstancePrototype,
                                               m_documentStore.Credentials as NetworkCredential);
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          return;

        if (value is JContainer)
          value = JSONObject.Parse(this.Engine, (value as JContainer).ToString(), null);

        if (value is NetworkCredentialInstance)
          m_documentStore.Credentials = (value as NetworkCredentialInstance).NetworkCredential;
        else if (value is ObjectInstance)
          m_documentStore.Credentials = JurassicHelper.Coerce<NetworkCredentialInstance>(this.Engine, value).NetworkCredential;
      }
    }

    [JSProperty(Name = "databaseCommands")]
    [JSDoc("Gets the database commands")]
    [JsonIgnore]
    public DatabaseCommandsInstance DatabaseCommands
    {
      get { return new DatabaseCommandsInstance(this.Engine.Object.InstancePrototype, m_documentStore.DatabaseCommands); }
    }

    [JSProperty(Name = "defaultDatabase")]
    [JSDoc("Gets or sets the default database name")]
    public string DefaultDatabase
    {
      get { return m_documentStore.DefaultDatabase; }
      set { m_documentStore.DefaultDatabase = value; }
    }

    [JSProperty(Name = "maxNumberOfCachedRequests")]
    [JSDoc("Gets or sets the max number of cached requests (Default: 2048)")]
    public int MaxNumberOfCachedRequests
    {
      get { return m_documentStore.MaxNumberOfCachedRequests; }
      set { m_documentStore.MaxNumberOfCachedRequests = value; }
    }

    [JSProperty(Name = "url")]
    [JSDoc("Gets or sets the base URL of the target RavenDB server instance.")]
    public string Url
    {
      get { return m_documentStore.Url; }
      set { m_documentStore.Url = value; }
    }

    [JSFunction(Name = "initialize")]
    [JSDoc("Initializes the DocumentStore.")]
    public DocumentStoreInstance Initialize()
    {
      m_documentStore.Initialize();
      return this;
    }

    [JSFunction(Name = "openSession")]
    [JSDoc("Opens a new session for a particular database")]
    public DocumentSessionInstance OpenSession(object arg)
    {
      RavenDB.IDocumentSession session;

      if (arg == null || arg == Null.Value || arg == Undefined.Value)
        session = m_documentStore.OpenSession();
      else if (arg is ObjectInstance)
      {
        var openSessionOptions = JurassicHelper.Coerce<OpenSessionsOptionsInstance>(this.Engine, arg);

        session = m_documentStore.OpenSession(openSessionOptions.OpenSessionOptions);
      }
      else
        session = m_documentStore.OpenSession(TypeConverter.ToString(arg));

      return new DocumentSessionInstance(this.Engine.Object.InstancePrototype, session);
    }
  }
}
