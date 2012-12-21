namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Index;
  using System;

  [Serializable]
  public class IndexWriterConstructor : ClrFunction
  {
    public IndexWriterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexWriter", new IndexWriterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexWriterInstance Construct()
    {
      return new IndexWriterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexWriterInstance : ObjectInstance, IDisposable
  {
    private readonly IndexWriter m_indexWriter;

    public IndexWriterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexWriterInstance(ObjectInstance prototype, IndexWriter indexWriter)
      : this(prototype)
    {
      if (indexWriter == null)
        throw new ArgumentNullException("indexWriter");

      m_indexWriter = indexWriter;
    }

    public IndexWriter IndexWriter
    {
      get { return m_indexWriter; }
    }

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      if (m_indexWriter != null)
      {
        m_indexWriter.Dispose();
      }
    }
  }
}
