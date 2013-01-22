namespace Barista.SharePoint.HostService
{
  using System.Runtime.Serialization;

  [DataContract(Namespace=Barista.Constants.ServiceNamespace)]
  public class Term
  {
    public string FieldName
    {
      get;
      set;
    }

    public string Value
    {
      get;
      set;
    }

    public Lucene.Net.Index.Term GetLuceneTerm()
    {
      return new Lucene.Net.Index.Term(this.FieldName, this.Value);
    }
  }
}
