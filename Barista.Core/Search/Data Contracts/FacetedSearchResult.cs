namespace Barista.Search
{
  using System.Collections.Generic;
  using System.Runtime.Serialization;

  [DataContract(Namespace = Barista.Constants.ServiceNamespace)]
  public class FacetedSearchResult
  {

    [DataMember]
    public string FacetName
    {
      get;
      set;
    }

    [DataMember]
    public double HitCount
    {
      get;
      set;
    }

    [DataMember]
    public IList<SearchResult> Documents
    {
      get;
      set;
    }
  }
}
