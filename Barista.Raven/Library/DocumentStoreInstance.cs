using RavenDB = Raven.Client;

namespace Barista.Raven.Library
{
  using System.Net;
  using Barista.Jurassic;
using Barista.Jurassic.Library;
using Barista.Library;
using System;
  using Barista.Newtonsoft.Json;

  [Serializable]
  public class DocumentStoreConstructor : ClrFunction
  {
    public DocumentStoreConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "DocumentStore", new DocumentStoreInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DocumentStoreInstance Construct()
    {
      var ds = new RavenDB.Document.DocumentStore();

      return new DocumentStoreInstance(this.InstancePrototype, ds);
    }
  }

  [Serializable]
  public class DocumentStoreInstance : ObjectInstance
  {
    private readonly RavenDB.Document.DocumentStore m_documentStore;

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
    public string ApiKey
    {
      get { return m_documentStore.ApiKey; }
      set { m_documentStore.ApiKey = value; }
    }

    [JSProperty(Name = "credentials")]
    [JsonProperty("credentials")]
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

        if (value is NetworkCredentialInstance)
          m_documentStore.Credentials = (value as NetworkCredentialInstance).NetworkCredential;
        else if (value is ObjectInstance)
          m_documentStore.Credentials = JurassicHelper.Coerce<NetworkCredentialInstance>(this.Engine, value).NetworkCredential;

      }
    }

    [JSProperty(Name = "databaseCommands")]
    public DatabaseCommandsInstance DatabaseCommands
    {
      get { return new DatabaseCommandsInstance(this.Engine.Object.InstancePrototype, m_documentStore.DatabaseCommands); }
    }

    [JSProperty(Name = "defaultDatabase")]
    public string DefaultDatabase
    {
      get { return m_documentStore.DefaultDatabase; }
      set { m_documentStore.DefaultDatabase = value; }
    }

    [JSProperty(Name = "maxNumberOfCachedRequests")]
    public int MaxNumberOfCachedRequests
    {
      get { return m_documentStore.MaxNumberOfCachedRequests; }
      set { m_documentStore.MaxNumberOfCachedRequests = value; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return m_documentStore.Url; }
      set { m_documentStore.Url = value; }
    }

    [JSFunction(Name = "openSession")]
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
