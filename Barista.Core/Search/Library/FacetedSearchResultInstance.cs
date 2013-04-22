namespace Barista.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Linq;

  [Serializable]
  public class FacetedSearchResultConstructor : ClrFunction
  {
    public FacetedSearchResultConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "FacetedSearchResult", new FacetedSearchResultInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public FacetedSearchResultInstance Construct()
    {
      return new FacetedSearchResultInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class FacetedSearchResultInstance : ObjectInstance
  {
    private readonly FacetedSearchResult m_facetedSearchResult;

    public FacetedSearchResultInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public FacetedSearchResultInstance(ObjectInstance prototype, FacetedSearchResult facetedSearchResult)
      : this(prototype)
    {
      if (facetedSearchResult == null)
        throw new ArgumentNullException("facetedSearchResult");

      m_facetedSearchResult = facetedSearchResult;
    }

    public FacetedSearchResult FacetedSearchResult
    {
      get { return m_facetedSearchResult; }
    }

    [JSProperty(Name = "facetName")]
    public string FacetName
    {
      get { return m_facetedSearchResult.FacetName; }
      set { m_facetedSearchResult.FacetName = value; }
    }

    [JSProperty(Name = "hitCount")]
    public int HitCount
    {
      get { return (int)m_facetedSearchResult.HitCount; }
      set { m_facetedSearchResult.HitCount = value; }
    }

    [JSProperty(Name = "documents")]
    public ArrayInstance Documents
    {
      get
      {
        var docs = m_facetedSearchResult.Documents.Select(d => new SearchResultInstance(this.Engine.Object.InstancePrototype, d));
// ReSharper disable CoVariantArrayConversion
        return this.Engine.Array.Construct(docs.ToArray());
// ReSharper restore CoVariantArrayConversion
      }
      set
      {
        m_facetedSearchResult.Documents =
          value.ElementValues.OfType<SearchResultInstance>().Select(s => s.SearchResult).ToList();
      }
    }
  }
}
