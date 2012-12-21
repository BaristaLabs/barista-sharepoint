namespace Barista.SharePoint.Search.Library
{
  using Barista.SharePoint.Search;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class SearchHitConstructor : ClrFunction
  {
    public SearchHitConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchHit", new SearchHitInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchHitInstance Construct()
    {
      return new SearchHitInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchHitInstance : ObjectInstance
  {
    private readonly Hit m_hit;

    public SearchHitInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchHitInstance(ObjectInstance prototype, Hit hit)
      : this(prototype)
    {
      if (hit == null)
        throw new ArgumentNullException("hit");

      m_hit = hit;
    }

    public Hit Hit
    {
      get { return m_hit; }
    }

    [JSProperty(Name = "score")]
    public double Score
    {
      get { return m_hit.Score; }
      set { m_hit.Score = Convert.ToSingle(value); }
    }

    [JSProperty(Name = "document")]
    public SearchDocumentInstance Document
    {
      get { return new SearchDocumentInstance(this.Engine.Object.InstancePrototype, m_hit.Document); }
      set
      {
        m_hit.Document = value == null
                           ? null
                           : value.Document;
      }
    }
  }
}
