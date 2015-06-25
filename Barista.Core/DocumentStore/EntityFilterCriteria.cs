namespace Barista.DocumentStore
{
  using System.Collections.Generic;

  public sealed class EntityFilterCriteria
  {
    public EntityFilterCriteria()
    {
      this.NamespaceMatchType = DocumentStore.NamespaceMatchType.Equals;
      this.QueryPairs = new Dictionary<string, string>();
      this.Path = null;
      this.Skip = null;
      this.Top = null;
      this.IncludeData = true;
    }

    public string Path
    {
      get;
      set;
    }

    public string Namespace
    {
      get;
      set;
    }

    public IDictionary<string, string> QueryPairs
    {
      get;
      set;
    }

    public NamespaceMatchType NamespaceMatchType
    {
      get;
      set;
    }

    public uint? Skip
    {
      get;
      set;
    }

    public uint? Top
    {
      get;
      set;
    }

    public bool IncludeData
    {
      get;
      set;
    }

    public IDictionary<string, string> FieldValues
    {
      get;
      set;
    }
  }

  public enum NamespaceMatchType
  {
    Equals,
    StartsWith,
    EndsWith,
    Contains,
    StartsWithMatchAnyQueryPairs,
    StartsWithMatchAnyQueryPairsContainsValue,
    StartsWithMatchAllQueryPairs,
    StartsWithMatchAllQueryPairsContainsValue,
  }
}
