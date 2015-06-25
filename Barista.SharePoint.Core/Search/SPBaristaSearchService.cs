namespace Barista.SharePoint.Search
{
  using System;
  using Barista.Search;

  public class SPBaristaSearchService : BaristaSearchService
  {
    protected override Lucene.Net.Store.Directory GetLuceneDirectoryFromIndexName(string indexName)
    {
      var directory = BaristaHelper.GetDirectoryFromIndexName(indexName);

      if (directory == null)
        throw new InvalidOperationException("Unable to locate an index with the specified name: " + indexName);

      return directory;
    }
  }
}
