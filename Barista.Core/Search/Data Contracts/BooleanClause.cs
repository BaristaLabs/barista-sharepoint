namespace Barista.Search
{
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class BooleanClause
  {
    public Query Query
    {
      get;
      set;
    }

    public Occur Occur
    {
      get;
      set;
    }
  }
}
