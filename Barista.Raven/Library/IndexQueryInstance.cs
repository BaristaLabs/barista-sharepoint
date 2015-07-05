using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Linq;
  using System.Text;
  using Barista.Library;

  [Serializable]
  public class IndexQueryConstructor : ClrFunction
  {
    public IndexQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexQuery", new IndexQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexQueryInstance Construct()
    {
      return new IndexQueryInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexQueryInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Data.IndexQuery m_indexQuery;

    public IndexQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexQueryInstance(ObjectInstance prototype, RavenDB.Abstractions.Data.IndexQuery indexQuery)
      : this(prototype)
    {
      if (indexQuery == null)
        throw new ArgumentNullException("indexQuery");

      m_indexQuery = indexQuery;
    }

    public RavenDB.Abstractions.Data.IndexQuery IndexQuery
    {
      get { return m_indexQuery; }
    }

    #region Properties

    [JSProperty(Name = "cutoff")]
    [JSDoc("Gets or sets the cutoff date for the query")]
    public object Cutoff
    {
      get
      {
        if (m_indexQuery.Cutoff.HasValue == false)
          return Null.Value;

        return JurassicHelper.ToDateInstance(this.Engine, m_indexQuery.Cutoff.Value);
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_indexQuery.Cutoff = null;
        else if (value is DateInstance)
          m_indexQuery.Cutoff = DateTime.Parse((value as DateInstance).ToIsoString());
      }
    }

    //[JSProperty(Name = "cutoffEtag")]
    //[JSDoc("Gets or sets the cutoff eTag for the query")]
    //public object CutoffEtag
    //{
    //  get
    //  {
    //    if (m_indexQuery.CutoffEtag.HasValue == false)
    //      return Null.Value;

    //    return new GuidInstance(this.Engine.Object.InstancePrototype, m_indexQuery.CutoffEtag.Value);
    //  }
    //  set
    //  {
    //    if (value == null || value == Null.Value || value == Undefined.Value)
    //      m_indexQuery.CutoffEtag = null;
    //    else
    //      m_indexQuery.CutoffEtag = GuidInstance.ConvertFromJsObjectToGuid(value);
    //  }
    //}

    [JSProperty(Name = "defaultField")]
    [JSDoc("Gets or sets the default field to use when querying directly on the lucene index.")]
    public string DefaultField
    {
      get { return m_indexQuery.DefaultField; }
      set { m_indexQuery.DefaultField = value; }
    }

    [JSProperty(Name = "defaultOperator")]
    [JSDoc("Gets or sets the default operator to use when querying directly on the lucene index.")]
    public string DefaultOperator
    {
      get { return m_indexQuery.DefaultOperator.ToString(); }
      set
      {
        RavenDB.Abstractions.Data.QueryOperator defaultOperator;

        if (value.TryParseEnum(true, out defaultOperator))
          m_indexQuery.DefaultOperator = defaultOperator;
      }
    }

    [JSProperty(Name = "fieldsToFetch")]
    [JSDoc("Gets or sets the fields to fetch.")]
    public ArrayInstance FieldsToFetch
    {
// ReSharper disable CoVariantArrayConversion
      get { return this.Engine.Array.Construct(m_indexQuery.FieldsToFetch); }
// ReSharper restore CoVariantArrayConversion
      set
      {
        if (value == null)
          m_indexQuery.FieldsToFetch = null;
        else
        {
          m_indexQuery.FieldsToFetch = value.ElementValues
            .ToList()
            .Select(TypeConverter.ToString)
            .ToArray();
        }
      }
    }

//    [JSProperty(Name = "groupBy")]
//    [JSDoc("Gets or sets the fields to group the query by.")]
//    public ArrayInstance GroupBy
//    {
//// ReSharper disable CoVariantArrayConversion
//      get { return this.Engine.Array.Construct(m_indexQuery.GroupBy); }
//// ReSharper restore CoVariantArrayConversion
//      set
//      {
//        if (value == null)
//          m_indexQuery.GroupBy = null;
//        else
//        {
//          m_indexQuery.GroupBy = value.ElementValues
//            .Select(TypeConverter.ToString)
//            .ToArray();
//        }
//      }
//    }

    //TODO: Highlighted-*

    [JSProperty(Name = "pageSize")]
    [JSDoc("Gets or sets the size of the page.")]
    public int PageSize
    {
      get { return m_indexQuery.PageSize; }
      set { m_indexQuery.PageSize = value; }
    }

    [JSProperty(Name = "query")]
    [JSDoc("Gets or sets the query.")]
    public string Query
    {
      get { return m_indexQuery.Query; }
      set { m_indexQuery.Query = value; }
    }

    //[JSProperty(Name = "skipTransformResults")]
    //[JSDoc("If set to true, raven won't execute the transform results function returning just the raw results instead.")]
    //public bool SkipTransformResults
    //{
    //  get { return m_indexQuery.SkipTransformResults; }
    //  set { m_indexQuery.SkipTransformResults = value; }
    //}

    //TODO: SortedFields

    [JSProperty(Name = "start")]
    [JSDoc("Gets or sets the start of records to read.")]
    public int Start
    {
      get { return m_indexQuery.Start; }
      set { m_indexQuery.Start = value; }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "appendQueryString")]
    public void AppendQueryString(string queryString)
    {
      m_indexQuery.AppendQueryString(new StringBuilder(queryString));
    }

    [JSFunction(Name = "clone")]
    [JSDoc("Clones the index query.")]
    public IndexQueryInstance Clone()
    {
      return new IndexQueryInstance(this.Engine.Object.InstancePrototype, m_indexQuery.Clone());
    }

    [JSFunction(Name = "getIndexQueryUrl")]
    [JSDoc("Returns the index query url.")]
    public string GetIndexQueryUrl(string operationUrl, string index, string operationName)
    {
      return m_indexQuery.GetIndexQueryUrl(operationUrl, index, operationName);
    }

    [JSFunction(Name = "getMinimalQueryString")]
    public string GetMinimalQueryString()
    {
      return m_indexQuery.GetMinimalQueryString();
    }

    [JSFunction(Name = "getQueryString")]
    public string GetQueryString()
    {
      return m_indexQuery.GetQueryString();
    }
    #endregion
  }
}
