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
  public class NumericRangeQueryInstance<T> : ObjectInstance, IQuery<NumericRangeQueryBase<T>>
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

    [JSProperty(Name = "boost")]
    public object Boost
    {
      get
      {
        if (Query.Boost.HasValue == false)
          return Null.Value;

        return (double)Query.Boost.Value;
      }
      set
      {
        if (value == Null.Value || value == Undefined.Value)
        {
          Query.Boost = null;
          return;
        }
        Query.Boost = (float)TypeConverter.ToNumber(value);
      }
    }

    public NumericRangeQueryBase<T> Query
    {
      get { return m_numericRangeQuery; }
    }
  }
  
}
