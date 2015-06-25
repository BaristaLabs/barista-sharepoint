namespace Barista.Search
{
  using System;
  using System.Collections.Generic;

  public class IndexingBatch
  {
    public List<BatchedDocument> Documents
    {
      get;
      set;
    }

    public DateTime? DateTime
    {
      get;
      set;
    }

    public IndexingBatch()
    {
      Documents = new List<BatchedDocument>();
    }

    public void Add(string documentId, Lucene.Net.Documents.Document doc, bool skipDeleteFromIndex)
    {
      Documents.Add(new BatchedDocument
        {
          DocumentId = documentId,
          Document = doc,
          SkipDeleteFromIndex = skipDeleteFromIndex
        });
    }

    public void Add(BatchedDocument document)
    {
      Documents.Add(document);
    }
  }

  public class BatchedDocument
  {
    public string DocumentId
    {
      get;
      set;
    }

    public Lucene.Net.Documents.Document Document
    {
      get;
      set;
    }

    public bool SkipDeleteFromIndex
    {
      get;
      set;
    }
  }
}
