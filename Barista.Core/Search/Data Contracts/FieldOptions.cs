namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class FieldOptions
  {
    [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public FieldIndexType? Index
    {
      get;
      set;
    }

    [DataMember]
    public FieldStorageType? Storage
    {
      get;
      set;
    }

    [DataMember]
    public FieldTermVectorType? TermVectorType
    {
      get;
      set;
    }
  }
}
