namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Search;
  using System;

  [Serializable]
  public class SearchResultConstructor : ClrFunction
  {
    public SearchResultConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchResult", new SearchResultInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchResultInstance Construct()
    {
      return new SearchResultInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchResultInstance : ObjectInstance
  {
    private readonly SearchResult m_searchResult;

    public SearchResultInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchResultInstance(ObjectInstance prototype, SearchResult searchResult)
      : this(prototype)
    {
      if (searchResult == null)
        throw new ArgumentNullException("searchResult");

      m_searchResult = searchResult;
    }

    public SearchResult SearchResult
    {
      get { return m_searchResult; }
    }

    [JSProperty(Name = "score")]
    public double Score
    {
      get { return m_searchResult.Score; }
      set { m_searchResult.Score = Convert.ToSingle(value); }
    }

    [JSProperty(Name = "luceneDocId")]
    public int LuceneDocId
    {
      get { return m_searchResult.LuceneDocId; }
      set { m_searchResult.LuceneDocId = value; }
    }

    [JSProperty(Name = "document")]
    public JsonDocumentInstance Document
    {
      get { return new JsonDocumentInstance(this.Engine.Object, m_searchResult.Document); }
      set { m_searchResult.Document = value.JsonDocument; }
    }
  }
}
