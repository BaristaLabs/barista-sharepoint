namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class Term
  {
    [DataMember]
    public string FieldName
    {
      get;
      set;
    }

    [DataMember]
    public string Value
    {
      get;
      set;
    }

    public static Lucene.Net.Index.Term ConvertToLuceneTerm(Term term)
    {
      return new Lucene.Net.Index.Term(term.FieldName, term.Value);
    }
  }
}
