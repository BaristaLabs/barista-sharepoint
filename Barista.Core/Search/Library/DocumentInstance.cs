namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Documents;
  using System;
  using System.Linq;

  [Serializable]
  public class DocumentConstructor : ClrFunction
  {
    public DocumentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Document", new DocumentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DocumentInstance Construct()
    {
      return new DocumentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class DocumentInstance : ObjectInstance
  {
    private readonly Document m_document;

    public DocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DocumentInstance(ObjectInstance prototype, Document document)
      : this(prototype)
    {
      if (document == null)
        throw new ArgumentNullException("document");

      m_document = document;
    }

    public Document Document
    {
      get { return m_document; }
    }

    [JSProperty(Name = "fields")]
    public ArrayInstance Fields
    {
      get
      {
        var fields = m_document.GetFields()
                                    .OfType<Field>()
                                    .Select(f => new SearchFieldInstance(this.Engine.Object.InstancePrototype, f))
                                    .ToArray();

        var result = this.Engine.Array.Construct(fields);
        return result;
      }
    }
  }
}
