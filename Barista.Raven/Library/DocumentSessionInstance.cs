using RavenDB = Raven.Client;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class DocumentSessionConstructor : ClrFunction
  {
    public DocumentSessionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "DocumentSession", new DocumentSessionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DocumentSessionInstance Construct()
    {
      return new DocumentSessionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class DocumentSessionInstance : ObjectInstance
  {
    private readonly RavenDB.IDocumentSession m_documentSession;

    public DocumentSessionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DocumentSessionInstance(ObjectInstance prototype, RavenDB.IDocumentSession documentSession)
      : this(prototype)
    {
      if (documentSession == null)
        throw new ArgumentNullException("documentSession");

      m_documentSession = documentSession;
    }

    public RavenDB.IDocumentSession DocumentSession
    {
      get { return m_documentSession; }
    }
  }
}
