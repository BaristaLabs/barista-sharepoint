namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic.Library;

  [Serializable]
// ReSharper disable InconsistentNaming
  public class SPCamlQueryBuilder_Where : ObjectInstance
// ReSharper restore InconsistentNaming
  {
    public SPCamlQueryBuilder_Where(ObjectInstance prototype, SPCamlQueryBuilderInstance builder)
      : base(prototype)
    {
      this.Builder = builder;
      this.PopulateFunctions();
    }

    #region Internal Properties
    internal SPCamlQueryBuilderInstance Builder
    {
      get;
      private set;
    }
    #endregion

    #region Functions
    /// <summary>
    /// Specifies reference to the Integer field with given internal name
    /// </summary>
    [JSFunction(Name = "IntegerField")]
    public SPCamlQueryBuilder_FieldRef IntegerField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Integer", false);
    }

    /// <summary>
    /// Specifies reference to the Number field with given internal name
    /// </summary>
    [JSFunction(Name = "NumberField")]
    public SPCamlQueryBuilder_FieldRef NumberField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Integer", false);
    }

    /// <summary>
    /// Specifies reference to the Counter field with given internal name (usually it is the "ID" field)
    /// </summary>
    [JSFunction(Name = "CounterField")]
    public SPCamlQueryBuilder_FieldRef CounterField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Counter", false);
    }

    /// <summary>
    /// Specifies reference to the Text field with given internal name
    /// </summary>
    [JSFunction(Name = "TextField")]
    public SPCamlQueryBuilder_FieldRef TextField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Text", false);
    }

    /// <summary>
    /// Specifies reference to the DateTime field with given internal name, and specifies, that it's time value will not be included
    /// </summary>
    [JSFunction(Name = "DateField")]
    public SPCamlQueryBuilder_FieldRef DateField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Date", false);
    }

    [JSFunction(Name = "GuidField")]
    public SPCamlQueryBuilder_FieldRef GuidField(string name)
    {
        return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Guid", false);
    }

    /// <summary>
    /// Specifies reference to the datetime field with given internal name
    /// </summary>
    [JSFunction(Name = "DateTimeField")]
    public SPCamlQueryBuilder_FieldRef DateTimeField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "DateTime", false);
    }

    /// <summary>
    /// Specifies reference to the integer field with given internal name
    /// </summary>
    [JSFunction(Name = "UserField")]
    public SPCamlQueryBuilder_FieldRef UserField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "User", false);
    }

    /// <summary>
    /// Specifies reference to a lookup field, using it's display value for further comparisons
    /// </summary>
    [JSFunction(Name = "LookupField")]
    public SPCamlQueryBuilder_FieldRef LookupField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Lookup", false);
    }

    /// <summary>
    /// Specifies reference to a lookup field, using it's ID for further comparisons
    /// </summary>
    [JSFunction(Name = "LookupIdField")]
    public SPCamlQueryBuilder_FieldRef LookupIdField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "Integer", true);
    }

    /// <summary>
    /// Specifies reference to a lookup field, using it's ID for further comparisons
    /// </summary>
    [JSFunction(Name = "ContentTypeIdField")]
    public SPCamlQueryBuilder_FieldRef ContentTypeIdField(string name)
    {
      return new SPCamlQueryBuilder_FieldRef(this.Engine.Object.InstancePrototype, Builder, name, "ContentTypeId", false);
    }

    /// <summary>
    /// Used in queries to compare the dates in a recurring event with a specified DateTime value, to determine whether they overlap.
    /// </summary>
    [JSFunction(Name = "DateRangesOverlap")]
    public SPCamlQueryBuilder_Token DateRangesOverlap(string eventDateField, string endDateField, string recurrenceIdField, string dateTimeValue)
    {

      var pos = Builder.Tree.Count;

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.Start,
        Name = "DateRangesOverlap",
      });

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = eventDateField,
      });

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = endDateField,
      });

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.FieldRef,
        Name = recurrenceIdField,
      });

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.Value,
        ValueType = "DateTime",
        Value = dateTimeValue,
      });

      Builder.Tree.Add(new CamlHump
      {
        Element = CamlHumpElementType.End,
      });

      return new SPCamlQueryBuilder_Token(this.Engine.Object.InstancePrototype, Builder, pos);
    }
    #endregion
  }
}
