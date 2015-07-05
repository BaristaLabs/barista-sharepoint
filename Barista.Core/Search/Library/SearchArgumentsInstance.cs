namespace Barista.Search.Library
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using Barista.Newtonsoft.Json;

  [Serializable]
  public class SearchArgumentsConstructor : ClrFunction
  {
    public SearchArgumentsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchArguments", new SearchArgumentsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SearchArgumentsInstance Construct()
    {
      return new SearchArgumentsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SearchArgumentsInstance : ObjectInstance
  {
    public SearchArgumentsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "query")]
    [JsonProperty("query")]
    public object Query
    {
      get;
      set;
    }

    [JSProperty(Name = "filter")]
    [JsonProperty("filter")]
    public object Filter
    {
      get;
      set;
    }

    [JSProperty(Name = "groupByFields")]
    [JsonProperty("groupByFields")]
    [JSDoc("ternPropertyType", "[string]")]
    public ArrayInstance GroupByFields
    {
      get;
      set;
    }

    [JSProperty(Name = "sort")]
    [JsonProperty("sort")]
    public object Sort
    {
      get;
      set;
    }

    [JSProperty(Name = "skip")]
    [JsonProperty("skip")]
    public int Skip
    {
      get;
      set;
    }

    [JSProperty(Name = "take")]
    [JsonProperty("take")]
    public int Take
    {
      get;
      set;
    }

    public SearchArguments GetSearchArguments()
    {
      var result = new SearchArguments();

      //Determine the Query value.
      if (this.Query == null || this.Query == Null.Value || this.Query == Undefined.Value)
        throw new JavaScriptException(this.Engine, "Error", "The Query property of the argument instance cannot be null or undefined.");

      var searchQueryType = this.Query.GetType();

      if (searchQueryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = searchQueryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        result.Query = queryProperty.GetValue(this.Query, null) as Query;
      }
      else
      {
        var parser = new QueryParserQuery
          {
            Query = TypeConverter.ToString(this.Query)
          };
        result.Query = parser;
      }

      //Determine the Filter value.

      var searchFilterType = this.Filter.GetType();

      if (searchFilterType.IsSubclassOfRawGeneric(typeof(FilterInstance<>)))
      {
        var filterProperty = searchFilterType.GetProperty("Filter", BindingFlags.Instance | BindingFlags.Public);
        result.Filter = filterProperty.GetValue(this.Filter, null) as Filter;
      }
      else
      {
        var parser = new QueryParserQuery
        {
          Query = TypeConverter.ToString(this.Query)
        };
        result.Filter = new QueryWrapperFilter {Query = parser};
      }

      //Determine the Sort value.
      if (this.Sort is SortInstance)
      {
        result.Sort = (this.Sort as SortInstance).Sort;
      }
      else if (TypeUtilities.IsString(this.Sort))
      {
        result.Sort = new Sort
          {
            SortFields = new List<SortField>
              {
                new SortField
                  {
                    FieldName = this.Sort as string,
                    Type = SortFieldType.String
                  }
              }
          };
      }

      if (this.GroupByFields != null && this.GroupByFields.Length > 0)
      {
        result.GroupByFields = this.GroupByFields.ElementValues.Select(v => TypeConverter.ToString(v)).ToList();
      }

      if (Skip > 0)
        result.Skip = this.Skip;

      if (Take > 0)
        result.Take = this.Take;

      return result;
    }
  }
}
