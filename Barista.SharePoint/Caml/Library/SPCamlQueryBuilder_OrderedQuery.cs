namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic.Library;

  [Serializable]
// ReSharper disable InconsistentNaming
  public class SPCamlQueryBuilder_OrderedQuery : ObjectInstance
// ReSharper restore InconsistentNaming
  {
    public SPCamlQueryBuilder_OrderedQuery(ObjectInstance prototype, SPCamlQueryBuilderInstance builder)
      : base(prototype)
    {
      this.Builder = builder;
      this.PopulateFunctions();
    }

    internal SPCamlQueryBuilderInstance Builder
    {
      get;
      private set;
    }

    /// <summary>
    /// Specifies primary sort field for retrieved items
    /// </summary>
    [JSFunction(Name = "ThenBy")]
    public SPCamlQueryBuilder_OrderedQuery ThenBy(string sortFieldName, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      this.Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = sortFieldName,
      });
      return new SPCamlQueryBuilder_OrderedQuery(this.Engine.Object.InstancePrototype, this.Builder);
    }

    /// <summary>
    /// Specifies primary sort field for retrieved items
    /// </summary>
    [JSFunction(Name = "ThenByDesc")]
    public SPCamlQueryBuilder_OrderedQuery ThenByDesc(string sortFieldName, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      this.Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = sortFieldName,
        IsDescending = true,
      });
      return new SPCamlQueryBuilder_OrderedQuery(this.Engine.Object.InstancePrototype, this.Builder);
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return SPCamlQueryBuilderInstance.Finalize(this.Builder);
    }

    [JSFunction(Name = "ToString")]
    public string ToString2()
    {
      return this.ToString();
    }
  }
}
