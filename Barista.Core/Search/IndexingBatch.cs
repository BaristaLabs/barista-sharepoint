namespace Barista.Search
{
  using System;
  using System.Collections.Generic;

  public class IndexingBatch
  {
    public IndexingBatch()
    {
      Ids = new List<string>();
      Docs = new List<JsonDocument>();
      SkipDeleteFromIndex = new List<bool>();
    }

    public readonly List<string> Ids;
    public readonly List<JsonDocument> Docs;
    public readonly List<bool> SkipDeleteFromIndex;
    public DateTime? DateTime;

    public void Add(JsonDocument doc, bool skipDeleteFromIndex)
    {
      Ids.Add(doc.DocumentId);
      Docs.Add(doc);
      SkipDeleteFromIndex.Add(skipDeleteFromIndex);
    }
  }
}
