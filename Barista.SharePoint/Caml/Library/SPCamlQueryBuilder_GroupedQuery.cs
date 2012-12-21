namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic.Library;

  [Serializable]
// ReSharper disable InconsistentNaming
  public class SPCamlQueryBuilder_GroupedQuery : ObjectInstance
// ReSharper restore InconsistentNaming
  {
    public SPCamlQueryBuilder_GroupedQuery(ObjectInstance prototype, SPCamlQueryBuilderInstance builder)
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
    [JSFunction(Name = "OrderBy")]
    public SPCamlQueryBuilder_OrderedQuery OrderBy(string sortFieldName, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      SPCamlQueryBuilderInstance.StartOrderBy(this.Builder, overrideOrderBy, useIndexForOrderBy);
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
    [JSFunction(Name = "OrderByDesc")]
    public SPCamlQueryBuilder_OrderedQuery OrderByDesc(string sortFieldName, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      SPCamlQueryBuilderInstance.StartOrderBy(this.Builder, overrideOrderBy, useIndexForOrderBy);
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
