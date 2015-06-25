namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class BooleanClause
  {
    [DataMember]
    public Query Query
    {
      get;
      set;
    }

    [DataMember]
    public Occur Occur
    {
      get;
      set;
    }
  }
}
