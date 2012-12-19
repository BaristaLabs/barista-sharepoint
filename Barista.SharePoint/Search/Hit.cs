namespace Barista.SharePoint.Search
{
  using Lucene.Net.Documents;

  public class Hit
  {
    public float Score
    {
      get;
      set;
    }

    public Document Document
    {
      get;
      set;
    }
  }
}
