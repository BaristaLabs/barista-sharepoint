namespace Barista.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;

  [Serializable]
  public class EntityFilterCriteriaInstance : ObjectInstance
  {
    public EntityFilterCriteriaInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.EntityFilterCriteria = new EntityFilterCriteria();

      this.PopulateFields();
    }

    public EntityFilterCriteria EntityFilterCriteria
    {
      get;
      set;
    }

    [JSProperty(Name = "path")]
    [JsonProperty("path")]
    public string Path
    {
      get { return EntityFilterCriteria.Path; }
      set { EntityFilterCriteria.Path = value; }
    }

    [JSProperty(Name = "namespace")]
    [JsonProperty("namespace")]
    public string Namespace
    {
      get { return EntityFilterCriteria.Namespace; }
      set { EntityFilterCriteria.Namespace = value; }
    }

    [JSProperty(Name = "namespaceMatchType")]
    [JsonProperty("namespaceMatchType")]
    public string NamespaceMatchType
    {
      get { return EntityFilterCriteria.NamespaceMatchType.ToString(); }
      set { EntityFilterCriteria.NamespaceMatchType = (NamespaceMatchType)Enum.Parse(typeof(NamespaceMatchType), value); }
    }

    [JSProperty(Name = "queryPairs")]
    [JsonProperty("queryPairs")]
    public IDictionary<string, string> QueryPairs
    {
      get { return EntityFilterCriteria.QueryPairs; }
      set { EntityFilterCriteria.QueryPairs = value; }
    }

    [JSProperty(Name = "skip")]
    [JsonProperty("skip")]
    public int? Skip
    {
      get { return (int)EntityFilterCriteria.Skip; }
      set { EntityFilterCriteria.Skip = (uint)value; }
    }

    [JSProperty(Name = "top")]
    [JsonProperty("top")]
    public int? Top
    {
      get { return (int)EntityFilterCriteria.Top; }
      set { EntityFilterCriteria.Top = (uint)value; }
    }
  }
}
