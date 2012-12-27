namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using Newtonsoft.Json;

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

    [JSProperty(Name = "sort")]
    [JsonProperty("sort")]
    public object Sort
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
  }
}
