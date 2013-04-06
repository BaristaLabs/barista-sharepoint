namespace Barista.Search
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Globalization;
  using System.Linq;
  using System.Threading;
  using Barista.Logging;
  using Lucene.Net.Documents;
  using Lucene.Net.Index;
  using Directory = Lucene.Net.Store.Directory;
  using Barista.Newtonsoft.Json;

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
        var processedKeys = new HashSet<string>();

        var docIdTerm = new Term(Constants.DocumentIdFieldName);
        var documentsWrapped = batch.Docs.Select((doc, i) =>
        {
          Interlocked.Increment(ref sourceCount);
          if (doc.DocumentId == null)
            throw new ArgumentException(
              string.Format("Cannot index something which doesn't have a document id, but got: '{0}'", doc));

          var documentId = doc.DocumentId.ToString(CultureInfo.InvariantCulture);

          if (processedKeys.Add(documentId) == false)
            return doc;

          if (batch.SkipDeleteFromIndex[i] == false)
            indexWriter.DeleteDocuments(docIdTerm.CreateTerm(documentId.ToLowerInvariant()));

          return doc;
        })
          .ToList();

        var jsonDocumentToLuceneDocumentConverter = new JsonDocumentToLuceneDocumentConverter(IndexDefinition);

        foreach (var jsonDocument in documentsWrapped)
        {
          Interlocked.Increment(ref count);
          var fields = jsonDocumentToLuceneDocumentConverter.Index(jsonDocument, Field.Store.NO);

          var luceneDoc = new Document();
          var documentIdField = new Field(Constants.DocumentIdFieldName, jsonDocument.DocumentId, Field.Store.YES,
                                          Field.Index.ANALYZED_NO_NORMS);
          luceneDoc.Add(documentIdField);

          var tempJsonDocument = new Services.JsonDocument
            {
              DocumentId = jsonDocument.DocumentId,
              MetadataAsJson = jsonDocument.Metadata.ToString(),
              DataAsJson = jsonDocument.DataAsJson.ToString()
            };

          var jsonDocumentField = new Field(Constants.JsonDocumentFieldName, JsonConvert.SerializeObject(tempJsonDocument), Field.Store.YES,
                                          Field.Index.NOT_ANALYZED_NO_NORMS);
         
          luceneDoc.Add(jsonDocumentField);

          foreach (var field in fields)
          {
            luceneDoc.Add(field);
          }
          LogIndexedDocument(jsonDocument.DocumentId, luceneDoc);
          AddDocumentToIndex(indexWriter, luceneDoc, analyzer);

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

        writer.DeleteDocuments(keys.Select(k => new Term(Constants.DocumentIdFieldName, k.ToLowerInvariant())).ToArray());
        
        return keys.Length;
      });
    }
  }
}
