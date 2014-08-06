namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System.Linq;
  using System;

  [Serializable]
  public class HitsPerFacetConstructor : ClrFunction
  {
    public HitsPerFacetConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "HitsPerFacet", new HitsPerFacetInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public HitsPerFacetInstance Construct()
    {
      return new HitsPerFacetInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class HitsPerFacetInstance : ObjectInstance
  {
    private readonly Lucene.Net.Search.SimpleFacetedSearch.HitsPerFacet m_hitsPerFacet;

    public HitsPerFacetInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public HitsPerFacetInstance(ObjectInstance prototype, Lucene.Net.Search.SimpleFacetedSearch.HitsPerFacet hitsPerFacet)
      : this(prototype)
    {
      if (hitsPerFacet == null)
        throw new ArgumentNullException("hitsPerFacet");

      m_hitsPerFacet = hitsPerFacet;
    }

    public Lucene.Net.Search.SimpleFacetedSearch.HitsPerFacet HitsPerFacet
    {
      get { return m_hitsPerFacet; }
    }

    [JSProperty(Name = "facetName")]
    public string FacetName
    {
      get { return m_hitsPerFacet.Name.ToString(); }
    }

    [JSProperty(Name = "hitCount")]
    public double HitCount
    {
      get { return m_hitsPerFacet.HitCount; }
    }

    [JSProperty(Name = "documents")]
    public ArrayInstance Documents
    {
      get
      {
        var docs =
          m_hitsPerFacet.Documents.Select(d => new DocumentInstance(this.Engine, d));
// ReSharper disable once CoVariantArrayConversion
        return this.Engine.Array.Construct(docs.ToArray());
      }
    }
  }
}
