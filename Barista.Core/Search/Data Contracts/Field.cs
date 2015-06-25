namespace Barista.Search
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(DateFieldDto))]
  [KnownType(typeof(StringFieldDto))]
  [KnownType(typeof(NumericFieldDto))]
  public abstract class FieldBase
  {
    [DataMember]
    public FieldIndexType Index
    {
      get;
      set;
    }

    [DataMember]
    public FieldStorageType Store
    {
      get;
      set;
    }

    [DataMember]
    public FieldTermVectorType TermVector
    {
      get;
      set;
    }

    [DataMember]
    public float? Boost
    {
      get;
      set;
    }

    [DataMember]
    public string Name
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(DateFieldDto))]
  [KnownType(typeof(StringFieldDto))]
  [KnownType(typeof(NumericFieldDto))]
  public abstract class FieldBase<T> : FieldBase
  {
    [DataMember]
    public T Value
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class DateFieldDto : FieldBase<DateTime>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class StringFieldDto : FieldBase<String>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class NumericFieldDto : FieldBase<Double>
  {
    [DataMember]
    public int PrecisionStep
    {
      get;
      set;
    }
  }
}
