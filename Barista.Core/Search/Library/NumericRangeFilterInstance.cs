namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class NumericRangeFilterConstructor<T> : ClrFunction
    where T : struct, IComparable<T>
  {
    public NumericRangeFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "NumericRangeFilter", new NumericRangeFilterInstance<T>(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public NumericRangeFilterInstance<T> Construct()
    {
      return new NumericRangeFilterInstance<T>(this.InstancePrototype);
    }
  }

  [Serializable]
  public class NumericRangeFilterInstance<T> : ObjectInstance
    where T : struct, IComparable<T>
  {
    private readonly NumericRangeFilterBase<T> m_numericRangeFilter;

    public NumericRangeFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public NumericRangeFilterInstance(ObjectInstance prototype, NumericRangeFilterBase<T> numericRangeFilter)
      : this(prototype)
    {
      if (numericRangeFilter == null)
        throw new ArgumentNullException("numericRangeFilter");

      m_numericRangeFilter = numericRangeFilter;
    }

    public NumericRangeFilterBase<T> Filter
    {
      get { return m_numericRangeFilter; }
    }
  }
}
