namespace Barista.SharePoint.HostService
{
  using System;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  [KnownType(typeof(FieldBase<>))]
  [KnownType(typeof(DateField))]
  [KnownType(typeof(StringField))]
  [KnownType(typeof(NumericField))]
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
  [KnownType(typeof(DateField))]
  [KnownType(typeof(StringField))]
  [KnownType(typeof(NumericField))]
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
  public class DateField : FieldBase<DateTime>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class StringField : FieldBase<String>
  {
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class NumericField : FieldBase<Double>
  {
    [DataMember]
    public int PrecisionStep
    {
      get;
      set;
    }
  }
}
