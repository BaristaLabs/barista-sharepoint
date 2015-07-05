using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using System;

  [Serializable]
  public class QueryResultConstructor : ClrFunction
  {
    public QueryResultConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "QueryResult", new QueryResultInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public QueryResultInstance Construct()
    {
      return new QueryResultInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class QueryResultInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Data.QueryResult m_queryResult;

    public QueryResultInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public QueryResultInstance(ObjectInstance prototype, RavenDB.Abstractions.Data.QueryResult queryResult)
      : this(prototype)
    {
      if (queryResult == null)
        throw new ArgumentNullException("queryResult");

      m_queryResult = queryResult;
    }

    public RavenDB.Abstractions.Data.QueryResult QueryResult
    {
      get { return m_queryResult; }
    }

    [JSProperty(Name = "highlightings")]
    public object Highlightings
    {
      get
      {
        if (m_queryResult.Highlightings == null)
          return Null.Value;

        var result = this.Engine.Object.Construct();
        foreach (var highlightingKvp in m_queryResult.Highlightings)
        {
          var highlightObj = this.Engine.Object.Construct();
          foreach (var highlightKvp in highlightingKvp.Value)
          {
// ReSharper disable CoVariantArrayConversion
            var arr = this.Engine.Array.Construct(highlightKvp.Value);
// ReSharper restore CoVariantArrayConversion
            highlightObj.SetPropertyValue(highlightKvp.Key, arr, false);
          }
         result.SetPropertyValue(highlightingKvp.Key, highlightObj, false);
        }

        return result;
      }
    }

    [JSProperty(Name = "includes")]
    public object Includes
    {
      get
      {
        if (m_queryResult.Includes == null)
          return Null.Value;

        return this.Engine.Array.Construct(m_queryResult.Includes.Select(i => JSONObject.Parse(this.Engine, i.ToString(), null)).ToArray());
      }
    }

    //[JSProperty(Name = "indexEtag")]
    //public GuidInstance IndexEtag
    //{
    //  get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_queryResult.IndexEtag as Guid); }
    //  set { m_queryResult.IndexEtag = value == null ? default(Guid) : value.Value; }
    //}

    [JSProperty(Name = "indexName")]
    public string IndexName
    {
      get { return m_queryResult.IndexName; }
      set { m_queryResult.IndexName = value; }
    }

    [JSProperty(Name = "indexTimestamp")]
    public DateInstance IndexTimestamp
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_queryResult.IndexTimestamp); }
      set { m_queryResult.IndexTimestamp = DateTime.Parse(value.ToIsoString()); }
    }

    [JSProperty(Name = "isStale")]
    public bool IsStale
    {
      get { return m_queryResult.IsStale; }
      set { m_queryResult.IsStale = value; }
    }

    [JSProperty(Name = "lastQueryTime")]
    public DateInstance LastQueryTime
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_queryResult.LastQueryTime); }
      set { m_queryResult.LastQueryTime = DateTime.Parse(value.ToIsoString()); }
    }

    [JSProperty(Name = "nonAuthoritativeInformation")]
    public bool NonAuthoritativeInformation
    {
      get { return m_queryResult.NonAuthoritativeInformation; }
      set { m_queryResult.NonAuthoritativeInformation = value; }
    }

    //[JSProperty(Name = "resultEtag")]
    //public GuidInstance ResultEtag
    //{
    //  get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_queryResult.ResultEtag); }
    //  set { m_queryResult.ResultEtag = value == null ? default(Guid) : value.Value; }
    //}

    [JSProperty(Name = "results")]
    public object Results
    {
      get
      {
        if (m_queryResult.Results == null)
          return null;

        return this.Engine.Array.Construct(m_queryResult.Results.Select(r => JSONObject.Parse(this.Engine, r.ToString(), null)).ToArray());
      }
    }

    [JSProperty(Name = "skippedResults")]
    public int SkippedResults
    {
      get { return m_queryResult.SkippedResults; }
      set { m_queryResult.SkippedResults = value; }
    }

    [JSProperty(Name = "totalResults")]
    public int TotalResults
    {
      get { return m_queryResult.TotalResults; }
      set { m_queryResult.TotalResults = value; }
    }
  }
}
