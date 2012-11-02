namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Text;
  using System.Xml;

  public class SPCamlQueryBuilder_Token : ObjectInstance
  {
    public SPCamlQueryBuilder_Token(ObjectInstance prototype, SPCamlQueryBuilderInstance builder, int startIndex)
      : base(prototype)
    {
      this.Builder = builder;
      this.StartIndex = startIndex;
      this.PopulateFunctions();
    }

    #region Internal Properties
    internal SPCamlQueryBuilderInstance Builder
    {
      get;
      private set;
    }

    internal int StartIndex
    {
      get;
      private set;
    }
    #endregion
    #region Functions

    /// <summary>
    /// And clause. Be aware! Operations sequence is always right-to-left, nomatter if there are Ands or Ors, or both.
    /// </summary>
    [JSFunction(Name = "And")]
    public SPCamlQueryBuilder_Where And()
    {
      this.Builder.Tree.Insert(this.StartIndex, new CamlHump()
      {
        Element = CamlHumpElementType.Start,
        Name = "And",
      });
      this.Builder.UnclosedTags++;

      return new SPCamlQueryBuilder_Where(this.Engine.Object.InstancePrototype, this.Builder);
    }

    /// <summary>
    /// Or clause. Be aware! Operations sequence is always right-to-left, nomatter if there are Ands or Ors, or both.
    /// </summary>
    [JSFunction(Name = "Or")]
    public SPCamlQueryBuilder_Where Or()
    {
      this.Builder.Tree.Insert(this.StartIndex, new CamlHump()
      {
        Element = CamlHumpElementType.Start,
        Name = "Or",
      });
      this.Builder.UnclosedTags++;

      return new SPCamlQueryBuilder_Where(this.Engine.Object.InstancePrototype, this.Builder);
    }

    /// <summary>
    /// Specifies grouping field for retrieved items
    /// </summary>
    [JSFunction(Name = "GroupBy")]
    public SPCamlQueryBuilder_GroupedQuery GroupBy(string groupFieldName, bool collapse)
    {
      SPCamlQueryBuilderInstance.StartGroupBy(this.Builder, groupFieldName, collapse);
      return new SPCamlQueryBuilder_GroupedQuery(this.Engine.Object.InstancePrototype, this.Builder);
    }

    /// <summary>
    /// Specifies primary sort field for retrieved items
    /// </summary>
    [JSFunction(Name = "OrderBy")]
    public SPCamlQueryBuilder_OrderedQuery OrderBy(string sortFieldName, bool overrideOrderBy, bool useIndexForOrderBy)
    {
      SPCamlQueryBuilderInstance.StartOrderBy(this.Builder, overrideOrderBy, useIndexForOrderBy);
      this.Builder.Tree.Add(new CamlHump()
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
      this.Builder.Tree.Add(new CamlHump()
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
    #endregion
  }
}
