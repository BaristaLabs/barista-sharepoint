namespace Barista.SharePoint.HostService
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class SearchIndexOptions
  {
    public string IndexId
    {
      get;
      set;
    }

    //public IndexType Type
    //{
    //  get;
    //  set;
    //}

    public string IndexUrl
    {
      get;
      set;
    }
  }
}
