namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(TermsFilter))]
  [KnownType(typeof(TermRangeFilter))]
  [KnownType(typeof(PrefixFilter))]
  [KnownType(typeof(QueryWrapperFilter))]
  [KnownType(typeof(DoubleNumericRangeFilter))]
  [KnownType(typeof(FloatNumericRangeFilter))]
  [KnownType(typeof(IntNumericRangeFilter))]
  [KnownType(typeof(LongNumericRangeFilter))]
  [KnownType(typeof(DuplicateFilter))]
  public abstract class Filter
  {
    public static Lucene.Net.Search.Filter ConvertFilterToLuceneFilter(Filter filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      Lucene.Net.Search.Filter lFilter;

      if (filter is QueryWrapperFilter)
      {
        var queryWrapperFilter = filter as QueryWrapperFilter;
        var lQuery = Query.ConvertQueryToLuceneQuery(queryWrapperFilter.Query);

        var lQueryWrapperFilter = new Lucene.Net.Search.QueryWrapperFilter(lQuery);

        lFilter = lQueryWrapperFilter;
      }
      else if (filter is DuplicateFilter)
      {
        var duplicateFilter = filter as DuplicateFilter;

        var lDuplicateFilter = new Lucene.Net.Search.DuplicateFilter(duplicateFilter.FieldName,
                                                                     (int) duplicateFilter.KeepMode,
                                                                     (int) duplicateFilter.ProcessingMode);

        lFilter = lDuplicateFilter;
      }
      else if (filter is TermsFilter)
      {
        var termsFilter = filter as TermsFilter;
        var lTermsFilter = new Lucene.Net.Search.TermsFilter();
        foreach (var term in termsFilter.Terms)
        {
          var lTerm = Term.ConvertToLuceneTerm(term);
          lTermsFilter.AddTerm(lTerm);
        }

        lFilter = lTermsFilter;
      }
      else if (filter is TermRangeFilter)
      {
        var termRangeFilter = filter as TermRangeFilter;
        var lTermRangeFilter = new Lucene.Net.Search.TermRangeFilter(termRangeFilter.FieldName,
                                                                     termRangeFilter.LowerTerm,
                                                                     termRangeFilter.UpperTerm,
                                                                     termRangeFilter.IncludeLower,
                                                                     termRangeFilter.IncludeUpper);
        lFilter = lTermRangeFilter;
      }
      else if (filter is PrefixFilter)
      {
        var prefixFilter = filter as PrefixFilter;
        var lTerm = Term.ConvertToLuceneTerm(prefixFilter.Term);
        var lPrefixFilter = new Lucene.Net.Search.PrefixFilter(lTerm);

        lFilter = lPrefixFilter;
      }
      else
      {
        throw new ArgumentException(@"Unknown or invalid filter object", "filter");
      }

      return lFilter;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class TermRangeFilter : Filter
  {
    [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public string LowerTerm
    {
      get;
      set;
    }

    [DataMember]
    public string UpperTerm
    {
      get;
      set;
    }

    [DataMember]
    public bool IncludeLower
    {
      get;
      set;
    }

    [DataMember]
    public bool IncludeUpper
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class TermsFilter : Filter
  {
    public TermsFilter()
    {
      this.Terms = new List<Term>();
    }

    [DataMember]
    public IList<Term> Terms
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class PrefixFilter : Filter
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class QueryWrapperFilter : Filter
  {
    [DataMember]
    public Query Query
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class NumericRangeFilterBase : Filter
  {
    [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public bool MinInclusive
    {
      get;
      set;
    }

    [DataMember]
    public bool MaxInclusive
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(DoubleNumericRangeFilter))]
  [KnownType(typeof(FloatNumericRangeFilter))]
  [KnownType(typeof(IntNumericRangeFilter))]
  [KnownType(typeof(LongNumericRangeFilter))]
  public abstract class NumericRangeFilterBase<T> : NumericRangeFilterBase
    where T : struct
  {
    [DataMember]
    public T? Min
    {
      get;
      set;
    }

    [DataMember]
    public T? Max
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class LongNumericRangeFilter : NumericRangeFilterBase<long>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class IntNumericRangeFilter : NumericRangeFilterBase<int>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class DoubleNumericRangeFilter : NumericRangeFilterBase<double>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class FloatNumericRangeFilter : NumericRangeFilterBase<float>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class DuplicateFilter : Filter
  {
    [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public KeepMode KeepMode
    {
      get;
      set;
    }

    [DataMember]
    public ProcessingMode ProcessingMode
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum KeepMode
  {
    [EnumMember]
    KeepFirst,
    [EnumMember]
    KeepLast,
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum ProcessingMode
  {
    [EnumMember]
    FastInvalidation,
    [EnumMember]
    FullValidation
  }
}
