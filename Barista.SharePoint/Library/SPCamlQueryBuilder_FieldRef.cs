namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Text;
  using System.Xml;

  [Serializable]
  public class SPCamlQueryBuilder_FieldRef : ObjectInstance
  {
    public SPCamlQueryBuilder_FieldRef(ObjectInstance prototype, SPCamlQueryBuilderInstance builder, string name, string valueType, bool lookupId)
      : base(prototype)
    {
      this.Builder = builder;
      this.StartIndex = builder.Tree.Count;
      this.ValueType = valueType;

      this.Membership = new SPCamlQueryBuilder_Membership(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);

      this.Builder.Tree.Add(new CamlHump()
      {
        Element = CamlHumpElementType.FieldRef,
        Name = name,
        LookupId = lookupId,
      });

      this.PopulateFields();
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

    internal string ValueType
    {
      get;
      private set;
    }
    #endregion

    #region Functions

    [JSProperty(Name = "Membership")]
    public SPCamlQueryBuilder_Membership Membership
    {
      get;
      private set;
    }

    /// <summary>
    /// Used within a query to return items that are empty (Null).
    /// </summary>
    [JSFunction(Name = "IsNull")]
    public SPCamlQueryBuilder_Token IsNull()
    {
      SPCamlQueryBuilderInstance.UnaryOperator(this.Builder, this.StartIndex, "IsNull");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Used within a query to return items that are not empty (Null).
    /// </summary>
    [JSFunction(Name = "IsNotNull")]
    public SPCamlQueryBuilder_Token IsNotNull()
    {
      SPCamlQueryBuilderInstance.UnaryOperator(this.Builder, this.StartIndex, "IsNotNull");
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "equal to" and is used within a query.
    /// </summary>
    [JSFunction(Name = "EqualTo")]
    public SPCamlQueryBuilder_Token EqualTo(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Eq", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "greater than" and is used within a query.
    /// </summary>
    [JSFunction(Name = "GreaterThan")]
    public SPCamlQueryBuilder_Token GreaterThan(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Gt", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "less than" and is used within a query.
    /// </summary>
    [JSFunction(Name = "LessThan")]
    public SPCamlQueryBuilder_Token LessThan(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Lt", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "greater than or equal to" and is used within a query.
    /// </summary>
    [JSFunction(Name = "GreaterThanOrEqualTo")]
    public SPCamlQueryBuilder_Token GreaterThanOrEqualTo(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Geq", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "less than or equal to" and is used within a query.
    /// </summary>
    [JSFunction(Name = "LessThanOrEqualTo")]
    public SPCamlQueryBuilder_Token LessThanOrEqualTo(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Leq", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Arithmetic operator that means "not equal to" and is used within a query.
    /// </summary>
    [JSFunction(Name = "NotEqualTo")]
    public SPCamlQueryBuilder_Token NotEqualTo(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Neq", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// If the specified field is a Lookup field that allows multiple values, specifies that the Value element is included in the list item for the field that is specified by the FieldRef element.
    /// </summary>
    [JSFunction(Name = "NotIncludes")]
    public SPCamlQueryBuilder_Token NotIncludes(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "NotIncludes", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// If the specified field is a Lookup field that allows multiple values, specifies that the Value element is excluded from the list item for the field that is specified by the FieldRef element.
    /// </summary>
    [JSFunction(Name = "Includes")]
    public SPCamlQueryBuilder_Token Includes(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Includes", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Searches for a string anywhere within a column that holds Text or Note field type values.
    /// </summary>
    [JSFunction(Name = "Contains")]
    public SPCamlQueryBuilder_Token Contains(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "Contains", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Searches for a string at the start of a column that holds Text or Note field type values.
    /// </summary>
    [JSFunction(Name = "BeginsWith")]
    public SPCamlQueryBuilder_Token BeginsWith(object value)
    {
      SPCamlQueryBuilderInstance.BinaryOperator(this.Builder, this.StartIndex, "BeginsWith", this.ValueType, value.ToString());
      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }

    /// <summary>
    /// Specifies whether the value of a list item for the field specified by the FieldRef element is equal to one of the values specified by the Values element.
    /// </summary>
    [JSFunction(Name = "In")]
    public SPCamlQueryBuilder_Token In(ArrayInstance array)
    {
      this.Builder.Tree.Insert(this.StartIndex, new CamlHump()
      {
        Element = CamlHumpElementType.Start,
        Name = "In"
      });

      this.Builder.Tree.Add(new CamlHump()
      {
        Element = CamlHumpElementType.Start,
        Name = "Values",
      });

      for (uint i = 0; i < array.Length; i++)
      {
        this.Builder.Tree.Add(new CamlHump()
        {
          Element = CamlHumpElementType.Value,
          ValueType = this.ValueType,
          Value = array[i].ToString(),
        });
      }

      this.Builder.Tree.Add(new CamlHump()
      {
        Element = CamlHumpElementType.End,
      });

      this.Builder.Tree.Add(new CamlHump()
      {
        Element = CamlHumpElementType.End,
      });

      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, this.Builder, this.StartIndex);
    }
    #endregion
  }
}
