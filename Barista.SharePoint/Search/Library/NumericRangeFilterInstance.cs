namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
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
  public class NumericRangeFilterInstance<T> : FilterInstance<NumericRangeFilter<T>>
    where T : struct, IComparable<T>
  {
    private readonly NumericRangeFilter<T> m_numericRangeFilter;

    public NumericRangeFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public NumericRangeFilterInstance(ObjectInstance prototype, NumericRangeFilter<T> numericRangeFilter)
      : this(prototype)
    {
      if (numericRangeFilter == null)
        throw new ArgumentNullException("numericRangeFilter");

      m_numericRangeFilter = numericRangeFilter;
    }

    public override NumericRangeFilter<T> Filter
    {
      get { return m_numericRangeFilter; }
    }
  }
}
