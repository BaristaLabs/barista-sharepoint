namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Documents;
  using System;
  using System.Linq;

  [Serializable]
  public class SearchDocumentConstructor : ClrFunction
  {
    public SearchDocumentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchDocument", new SearchDocumentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchDocumentInstance Construct()
    {
      return new SearchDocumentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchDocumentInstance : ObjectInstance
  {
    private readonly Document m_document;

    public SearchDocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchDocumentInstance(ObjectInstance prototype, Document document)
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
