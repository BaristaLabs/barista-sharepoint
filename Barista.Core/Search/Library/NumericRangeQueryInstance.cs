namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class NumericRangeQueryConstructor<T> : ClrFunction
    where T : struct, IComparable<T>
  {
    public NumericRangeQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "NumericRangeQuery", new NumericRangeQueryInstance<T>(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public NumericRangeQueryInstance<T> Construct()
    {
      return new NumericRangeQueryInstance<T>(this.InstancePrototype);
    }
  }

  [Serializable]
  public class NumericRangeQueryInstance<T> : ObjectInstance
    where T : struct, IComparable<T>
  {
    private readonly NumericRangeQueryBase<T> m_numericRangeQuery;

    public NumericRangeQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public NumericRangeQueryInstance(ObjectInstance prototype, NumericRangeQueryBase<T> numericRangeQuery)
      : this(prototype)
    {
      if (numericRangeQuery == null)
        throw new ArgumentNullException("numericRangeQuery");

      m_numericRangeQuery = numericRangeQuery;
    }

    public NumericRangeQueryBase<T> Query
    {
      get { return m_numericRangeQuery; }
    }
  }
  
}
