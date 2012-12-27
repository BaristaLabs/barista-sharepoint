namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class ScoreDocConstructor : ClrFunction
  {
    public ScoreDocConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ScoreDoc", new ScoreDocInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ScoreDocInstance Construct()
    {
      return new ScoreDocInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ScoreDocInstance : ObjectInstance
  {
    private readonly ScoreDoc m_scoreDoc;
    private readonly IndexSearcher m_searcher;

    public ScoreDocInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ScoreDocInstance(ObjectInstance prototype, ScoreDoc scoreDoc)
      : this(prototype, scoreDoc, null)
    {
    }

    public ScoreDocInstance(ObjectInstance prototype, ScoreDoc scoreDoc, IndexSearcher searcher)
      : this(prototype)
    {
      if (scoreDoc == null)
        throw new ArgumentNullException("scoreDoc");

      m_scoreDoc = scoreDoc;

      m_searcher = searcher;
    }

    public ScoreDoc ScoreDoc
    {
      get { return m_scoreDoc; }
    }

    [JSProperty(Name = "score")]
    public double Score
    {
      get { return m_scoreDoc.Score; }
    }

    [JSProperty(Name = "documentId")]
    public int DocumentId
    {
      get { return m_scoreDoc.Doc; }
    }

    [JSProperty(Name = "document")]
    public DocumentInstance Document
    {
      get
      {
        return m_searcher != null
          ? new DocumentInstance(this.Engine.Object.InstancePrototype, m_searcher.Doc(m_scoreDoc.Doc))
          : null;
      }
    }
  }
}
