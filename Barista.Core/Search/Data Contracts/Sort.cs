namespace Barista.Search
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class Sort
  {
    [DataMember]
    public IList<SortField> SortFields
    {
      get;
      set;
    }

    public static Lucene.Net.Search.Sort ConvertSortToLuceneSort(Sort sort)
    {
      if (sort == null || sort.SortFields == null || sort.SortFields.Count == 0)
        return new Lucene.Net.Search.Sort();

      var lSortFields = sort.SortFields
        .Select(sortField => new Lucene.Net.Search.SortField(sortField.FieldName,
          (int) sortField.Type,
          sortField.Reverse))
        .ToList();

      return new Lucene.Net.Search.Sort(lSortFields.ToArray());
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class SortField
  {
     [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public SortFieldType Type
    {
      get;
      set;
    }

    [DataMember]
    public bool Reverse
    {
      get;
      set;
    }
  }

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public enum SortFieldType
  {
    [EnumMember]
    Score = 0,
    [EnumMember]
    Doc = 1,
    [EnumMember]
    String = 3,
    [EnumMember]
    Int = 4,
    [EnumMember]
    Float = 5,
    [EnumMember]
    Long = 6,
    [EnumMember]
    Double = 7,
    [EnumMember]
    Short = 8,
    [EnumMember]
    Custom = 9,
    [EnumMember]
    Byte = 10,
    [EnumMember]
    StringValue = 11,
  }
}
