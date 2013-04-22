namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Runtime.Serialization;
  using Lucene.Net.Analysis.Standard;
  using Version = Lucene.Net.Util.Version;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(MatchAllDocsQuery))]
  [KnownType(typeof(TermQuery))]
  [KnownType(typeof(TermRangeQuery))]
  [KnownType(typeof(PhraseQuery))]
  [KnownType(typeof(PrefixQuery))]
  [KnownType(typeof(RegexQuery))]
  [KnownType(typeof(FuzzyQuery))]
  [KnownType(typeof(BooleanQuery))]
  [KnownType(typeof(DoubleNumericRangeQuery))]
  [KnownType(typeof(FloatNumericRangeQuery))]
  [KnownType(typeof(IntNumericRangeQuery))]
  [KnownType(typeof(LongNumericRangeQuery))]
  [KnownType(typeof(WildcardQuery))]
  [KnownType(typeof(QueryParserQuery))]
  [KnownType(typeof(ODataQuery))]
  public abstract class Query
  {
    [DataMember]
    public float? Boost
    {
      get;
      set;
    }

    public static Lucene.Net.Search.Query ConvertQueryToLuceneQuery(Query query)
    {
      if (query == null)
        throw new ArgumentNullException("query");

      Lucene.Net.Search.Query lQuery;

      if (query is MatchAllDocsQuery)
      {
        var lMatchAllDocsQuery = new Lucene.Net.Search.MatchAllDocsQuery();
        lQuery = lMatchAllDocsQuery;
      }
      else if (query is TermQuery)
      {
        var termQuery = query as TermQuery;
        var term = Term.ConvertToLuceneTerm(termQuery.Term);

        lQuery = new Lucene.Net.Search.TermQuery(term);
      }
      else if (query is TermRangeQuery)
      {
        var termRangeQuery = query as TermRangeQuery;
        var lTermRangeQuery = new Lucene.Net.Search.TermRangeQuery(termRangeQuery.FieldName,
                                                                   termRangeQuery.LowerTerm,
                                                                   termRangeQuery.UpperTerm,
                                                                   termRangeQuery.LowerInclusive,
                                                                   termRangeQuery.UpperInclusive);

        lQuery = lTermRangeQuery;
      }
      else if (query is PhraseQuery)
      {
        var phraseQuery = query as PhraseQuery;
        var lPhraseQuery = new Lucene.Net.Search.PhraseQuery();
        foreach (var term in phraseQuery.Terms)
        {
          var lTerm = Term.ConvertToLuceneTerm(term);
          lPhraseQuery.Add(lTerm);
        }

        if (phraseQuery.Slop.HasValue)
          lPhraseQuery.Slop = phraseQuery.Slop.Value;

        lQuery = lPhraseQuery;
      }
      else if (query is PrefixQuery)
      {
        var prefixQuery = query as PrefixQuery;
        var term = Term.ConvertToLuceneTerm(prefixQuery.Term);
        var lPrefixQuery = new Lucene.Net.Search.PrefixQuery(term);

        lQuery = lPrefixQuery;
      }
      else if (query is RegexQuery)
      {
        var regexQuery = query as RegexQuery;
        var term = Term.ConvertToLuceneTerm(regexQuery.Term);
        var lRegexQuery = new Contrib.Regex.RegexQuery(term);

        lQuery = lRegexQuery;
      }
      else if (query is FuzzyQuery)
      {
        var fuzzyQuery = query as FuzzyQuery;
        var term = Term.ConvertToLuceneTerm(fuzzyQuery.Term);
        var lFuzzyQuery = new Lucene.Net.Search.FuzzyQuery(term);

        lQuery = lFuzzyQuery;
      }
      else if (query is BooleanQuery)
      {
        var booleanQuery = query as BooleanQuery;
        var lBooleanQuery = new Lucene.Net.Search.BooleanQuery();
        foreach (var clause in booleanQuery.Clauses)
        {
          var lNestedQuery = Query.ConvertQueryToLuceneQuery(clause.Query);
          Lucene.Net.Search.Occur lOccur;
          switch (clause.Occur)
          {
            case Occur.Must:
              lOccur = Lucene.Net.Search.Occur.MUST;
              break;
            case Occur.MustNot:
              lOccur = Lucene.Net.Search.Occur.MUST_NOT;
              break;
            case Occur.Should:
              lOccur = Lucene.Net.Search.Occur.MUST_NOT;
              break;
            default:
              throw new InvalidOperationException("Occur not implemented or defined.");
          }

          var lClause = new Lucene.Net.Search.BooleanClause(lNestedQuery, lOccur);
          lBooleanQuery.Add(lClause);
        }

        if (booleanQuery.MinimumNumberShouldMatch.HasValue)
          lBooleanQuery.MinimumNumberShouldMatch = booleanQuery.MinimumNumberShouldMatch.Value;

        lQuery = lBooleanQuery;
      }
      else if (query is WildcardQuery)
      {
        var wildcardQuery = query as WildcardQuery;
        var lTerm = Term.ConvertToLuceneTerm(wildcardQuery.Term);
        var lWildcardQuery = new Lucene.Net.Search.WildcardQuery(lTerm);

        lQuery = lWildcardQuery;
      }
      else if (query is DoubleNumericRangeQuery)
      {
        var doubleNumericRangeQuery = query as DoubleNumericRangeQuery;

        var ldoubleNumericRangeQuery = Lucene.Net.Search.NumericRangeQuery.NewDoubleRange(
          doubleNumericRangeQuery.FieldName,
          doubleNumericRangeQuery.Min,
          doubleNumericRangeQuery.Max,
          doubleNumericRangeQuery.MinInclusive,
          doubleNumericRangeQuery.MaxInclusive);

        lQuery = ldoubleNumericRangeQuery;
      }
      else if (query is FloatNumericRangeQuery)
      {
        var floatNumericRangeQuery = query as FloatNumericRangeQuery;

        var lfloatNumericRangeQuery = Lucene.Net.Search.NumericRangeQuery.NewFloatRange(
          floatNumericRangeQuery.FieldName,
          floatNumericRangeQuery.Min,
          floatNumericRangeQuery.Max,
          floatNumericRangeQuery.MinInclusive,
          floatNumericRangeQuery.MaxInclusive);

        lQuery = lfloatNumericRangeQuery;
      }
      else if (query is IntNumericRangeQuery)
      {
        var intNumericRangeQuery = query as IntNumericRangeQuery;

        var lintNumericRangeQuery = Lucene.Net.Search.NumericRangeQuery.NewIntRange(
          intNumericRangeQuery.FieldName,
          intNumericRangeQuery.Min,
          intNumericRangeQuery.Max,
          intNumericRangeQuery.MinInclusive,
          intNumericRangeQuery.MaxInclusive);

        lQuery = lintNumericRangeQuery;
      }
      else if (query is LongNumericRangeQuery)
      {
        var longNumericRangeQuery = query as LongNumericRangeQuery;

        var llongNumericRangeQuery = Lucene.Net.Search.NumericRangeQuery.NewLongRange(
          longNumericRangeQuery.FieldName,
          longNumericRangeQuery.Min,
          longNumericRangeQuery.Max,
          longNumericRangeQuery.MinInclusive,
          longNumericRangeQuery.MaxInclusive);

        lQuery = llongNumericRangeQuery;
      }
      else if (query is QueryParserQuery)
      {
        var queryParserQuery = query as QueryParserQuery;

        var queryParser = new Lucene.Net.QueryParsers.QueryParser(Version.LUCENE_30,
                                                                  queryParserQuery.DefaultField,
                                                                  new StandardAnalyzer(Version.LUCENE_30))
          {
            AllowLeadingWildcard =
              queryParserQuery.AllowLeadingWildcard
          };

        lQuery = queryParser.Parse(queryParserQuery.Query);
      }
      else
      {
        throw new ArgumentException(@"Unknown or invalid query object", "query");
      }

      if (query.Boost.HasValue)
        lQuery.Boost = query.Boost.Value;

      return lQuery;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class MatchAllDocsQuery : Query
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class PhraseQuery : Query
  {
    public PhraseQuery()
    {
      this.Terms = new List<Term>();
    }

    [DataMember]
    public int? Slop
    {
      get;
      set;
    }

    [DataMember]
    public IList<Term> Terms
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class PrefixQuery : Query
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class RegexQuery : Query
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class FuzzyQuery : Query
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class BooleanQuery : Query
  {
    public BooleanQuery()
    {
      this.Clauses = new List<BooleanClause>();
    }

    [DataMember]
    public int? MinimumNumberShouldMatch
    {
      get;
      set;
    }

    [DataMember]
    public IList<BooleanClause> Clauses
    {
      get;
      set;
    }
  }


  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class TermQuery : Query
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class TermRangeQuery : Query
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
    public bool LowerInclusive
    {
      get;
      set;
    }

    [DataMember]
    public bool UpperInclusive
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class WildcardQuery : Query
  {
    [DataMember]
    public Term Term
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class NumericRangeQueryBase : Query
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
  [KnownType(typeof(DoubleNumericRangeQuery))]
  [KnownType(typeof(FloatNumericRangeQuery))]
  [KnownType(typeof(IntNumericRangeQuery))]
  [KnownType(typeof(LongNumericRangeQuery))]
  public abstract class NumericRangeQueryBase<T> : NumericRangeQueryBase
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
  public class LongNumericRangeQuery : NumericRangeQueryBase<long>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class IntNumericRangeQuery : NumericRangeQueryBase<int>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class DoubleNumericRangeQuery : NumericRangeQueryBase<double>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class FloatNumericRangeQuery : NumericRangeQueryBase<float>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class QueryParserQuery : Query
  {
    [DataMember]
    public bool AllowLeadingWildcard
    {
      get;
      set;
    }

    [DataMember]
    public string DefaultField
    {
      get;
      set;
    }

    [DataMember]
    public string Query
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class ODataQuery : Query
  {
    [DataMember]
    public bool AllowLeadingWildcard
    {
      get;
      set;
    }

    [DataMember]
    public string DefaultField
    {
      get;
      set;
    }

    [DataMember]
    public string Query
    {
      get;
      set;
    }
  }
}
