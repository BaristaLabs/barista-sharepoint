namespace Barista.Search
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class SearchArguments
  {
    public SearchArguments()
    {
      Filter = null;
      Sort = null;
      Skip = null;
      Take = 100;
    }

    [DataMember]
    public Query Query
    {
      get;
      set;
    }

    [DataMember]
    public Filter Filter
    {
      get;
      set;
    }

    [DataMember]
    public IList<string> GroupByFields
    {
      get;
      set;
    }

    [DataMember]
    public Sort Sort
    {
      get;
      set;
    }

    [DataMember]
    public int? Skip
    {
      get;
      set;
    }

    [DataMember]
    public int? Take
    {
      get;
      set;
    }
  }
}
