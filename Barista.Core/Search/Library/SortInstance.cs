namespace Barista.Search.Library
{
  using Barista.Extensions;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class SortConstructor : ClrFunction
  {
    public SortConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Sort", new SortInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SortInstance Construct()
    {
      return new SortInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  [JSDoc("Represents a sort operator used with the Search function of the Barista Search Index bundle")]
  public class SortInstance : ObjectInstance
  {
    private readonly Sort m_sort;

    public SortInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SortInstance(ObjectInstance prototype, Sort sort)
      : this(prototype)
    {
      if (sort == null)
        throw new ArgumentNullException("sort");

      m_sort = sort;
    }

    public Sort Sort
    {
      get { return m_sort; }
    }

    [JSFunction(Name = "addSortField")]
    [JSDoc("Adds a sort field to the sort object")]
    public SortInstance AddSortField(
      [JSDoc("(string) Name of the field to sort by")]
      string fieldName,
      [JSDoc("(bool) (Optional) Indicates whether or not to reverse the sort (false is ascending, true is descending). Default is false.")]
      object reverse,
      [JSDoc("(string) (Optional) Indicates the field type to sort by. Possible values are: string, byte, double, float, int, long, short, string value, doc and score. Default is score.")]
      object fieldType)
    {
      if (fieldName.IsNullOrWhiteSpace())
        throw new JavaScriptException(this.Engine, "Error", "A field name must be specified as the first argument.");

      var reverseValue = JurassicHelper.GetTypedArgumentValue(this.Engine, reverse, false);
      var fieldTypeValue = JurassicHelper.GetTypedArgumentValue(this.Engine, fieldType, "string");
      
      SortFieldType fieldTypeEnum;
      if (fieldTypeValue.TryParseEnum(true, out fieldTypeEnum) == false)
        fieldTypeEnum = SortFieldType.String;

      Sort.SortFields.Add(new SortField
      {
        FieldName = fieldName,
        Reverse = reverseValue,
        Type = fieldTypeEnum,
      });

      return this;
    }
  }
}
