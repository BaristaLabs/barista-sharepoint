namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Globalization;
  using System.Linq;
  using System.Threading;
  using Barista.Logging;
  using Directory = Lucene.Net.Store.Directory;

  public class SimpleIndex : Index
  {
    public SimpleIndex(Directory directory, string name, IndexDefinition indexDefinition)
      : base(directory, name, indexDefinition)
    {
    }

    public override void IndexDocuments(IndexingBatch batch)
    {
      var count = 0;
      var sourceCount = 0;
      var sw = Stopwatch.StartNew();
      var start = DateTime.UtcNow;
      Write((indexWriter, analyzer) =>
      {
        //TODO: The following would be a perfect candidate for a TPL DataFlow impl. Too bad we're currently on .Net 3.5
        
        var processedKeys = new HashSet<string>();

        var docIdTerm = new Lucene.Net.Index.Term(Constants.DocumentIdFieldName);
        var documentsWrapped = batch.Documents.Select((doc, i) =>
        {
          Interlocked.Increment(ref sourceCount);
          if (doc.DocumentId == null)
            throw new ArgumentException(
              string.Format("Cannot index something which doesn't have a document id, but got: '{0}'", doc));

          var documentId = doc.DocumentId.ToString(CultureInfo.InvariantCulture);

          if (processedKeys.Add(documentId) == false)
            return doc;

          if (doc.SkipDeleteFromIndex == false)
            indexWriter.DeleteDocuments(docIdTerm.CreateTerm(documentId.ToLowerInvariant()));

          return doc;
        })
          .ToList();

        foreach (var document in documentsWrapped)
        {
          Interlocked.Increment(ref count);
          
          LogIndexedDocument(document.DocumentId, document.Document);
          AddDocumentToIndex(indexWriter, document.Document, analyzer);

          indexWriter.Commit();
        }
        
        return sourceCount;
      });

      AddindexingPerformanceStat(new IndexingPerformanceStats
      {
        OutputCount = count,
        InputCount = sourceCount,
        Duration = sw.Elapsed,
        Operation = "Index",
        Started = start
      });

      LogIndexing.Debug("Indexed {0} documents for {1}", count, Name);
    }

    public override void Remove(string[] keys)
    {
      Write((writer, analyzer) =>
      {
        LogIndexing.Debug(() => string.Format("Deleting ({0}) from {1}", string.Join(", ", keys), Name));

        writer.DeleteDocuments(keys.Select(k => new Lucene.Net.Index.Term(Constants.DocumentIdFieldName, k.ToLowerInvariant())).ToArray());
        
        return keys.Length;
      });
    }
  }
}
