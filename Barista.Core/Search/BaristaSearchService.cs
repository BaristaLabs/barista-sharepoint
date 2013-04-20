namespace Barista.Services
{
  using Barista.Extensions;
  using Barista.Logging;
  using Barista.Newtonsoft.Json;
  using Barista.Newtonsoft.Json.Linq;
  using Barista.Search;
  using Barista.Search.OData2Lucene;
  using Lucene.Net.Analysis;
  using Lucene.Net.Index;
  using Lucene.Net.Search;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.ServiceModel;
  using TermQuery = Lucene.Net.Search.TermQuery;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public abstract class BaristaSearchService : IBaristaSearch, IDisposable
  {
    private const string IndexVersion = "1.0.0.0";
    private static readonly Analyzer DummyAnalyzer = new SimpleAnalyzer();
    private static readonly ConcurrentDictionary<string, Index> Indexes = new ConcurrentDictionary<string, Index>();

    private static readonly ILog Log = LogManager.GetCurrentClassLogger();
    private static readonly ILog StartupLog = LogManager.GetLogger(typeof(BaristaSearchService).FullName + ".Startup");

    /// <summary>
    /// Deletes all documents from the specified index.
    /// </summary>
    /// <param name="indexName"></param>
    public void DeleteAllDocuments(string indexName)
    {
      //try
      //{
        var index = GetOrAddIndex(indexName, true);
        try
        {
          index.DeleteAll();
        }
        catch (OutOfMemoryException)
        {
          CloseIndexWriter(indexName, false);
        }
      //}
      //catch (Exception ex)
      //{
      //  throw new FaultException(ex.Message);
      //}
      
    }

    /// <summary>
    /// Deletes the documents that have the specified document ids
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="keys"></param>
    public void DeleteDocuments(string indexName, IEnumerable<string> keys)
    {
      try
      {
        var index = GetOrAddIndex(indexName, true);

        try
        {
          //Remove the documents from the index
          index.Remove(keys.ToArray());
        }
        catch (OutOfMemoryException)
        {
          CloseIndexWriter(indexName, false);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
      
    }

    /// <summary>
    /// Indexes the specified document.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="documentId"></param>
    /// <param name="document"></param>
    public void IndexDocument(string indexName, string documentId, DocumentDto document)
    {
      try
      {
        if (documentId.IsNullOrWhiteSpace())
          throw new ArgumentNullException("documentId", @"A document id must be specified.");

        if (document == null)
          throw new ArgumentNullException("document", @"A document must be specified.");

        var index = GetOrAddIndex(indexName, true);

        try
        {
          //Add it to the index.
          var luceneDocument = DocumentDto.ConvertToLuceneDocument(document);

          var batch = new IndexingBatch();
          batch.Add(new BatchedDocument
            {
              DocumentId = documentId,
              Document = luceneDocument,
              SkipDeleteFromIndex = false,
            });

          index.IndexDocuments(batch);
        }
        catch (OutOfMemoryException)
        {
          CloseIndexWriter(indexName, false);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// Indexes the specified document.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="document"></param>
    public void IndexJsonDocument(string indexName, JsonDocumentDto document)
    {
      try
      {
        if (document == null)
          throw new ArgumentNullException("document", @"A document must be specified.");

        if (document.DocumentId.IsNullOrWhiteSpace())
          throw new InvalidOperationException(@"The json document must specify a document id.");

        IndexJsonDocuments(indexName, new List<JsonDocumentDto> { document });
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// Indexes the specified documents.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="documents"></param>
    public void IndexJsonDocuments(string indexName, IEnumerable<JsonDocumentDto> documents)
    {
      try
      {
        if (documents == null)
          throw new ArgumentNullException("documents", @"A collection of documents must be specified.");

        var jsonDocuments = documents as IList<JsonDocumentDto> ?? documents.ToList();

        if (jsonDocuments.Any() == false)
          throw new ArgumentNullException("documents", @"At least one document must be contained within the collection.");

        var index = GetOrAddIndex(indexName, true);

        //try
        //{
          //Add it to the index.
          var batch = new IndexingBatch();

          //Attempt to create a new Search.JsonDocument from the document
          var searchJsonDocuments = jsonDocuments.Select(document => new Search.JsonDocument
          {
            DocumentId = document.DocumentId,
            Metadata = document.MetadataAsJson.IsNullOrWhiteSpace() == false
                         ? JObject.Parse(document.MetadataAsJson)
                         : new JObject(),
            DataAsJson = JObject.Parse(document.DataAsJson)
          });

          var luceneDocuments =
            JsonDocumentToLuceneDocumentConverter.ConvertJsonDocumentToLuceneDocument(index.IndexDefinition,
                                                                                      searchJsonDocuments);

          foreach (var luceneDocument in luceneDocuments)
          {
            batch.Add(luceneDocument);
          }

          //TODO: Add the batch to a BlockingCollection<IndexingBatch> and run a thread that consumes the batches
          //See http://www.codethinked.com/blockingcollection-and-iproducerconsumercollection
          index.IndexDocuments(batch);
        }
        catch (OutOfMemoryException)
        {
          CloseIndexWriter(indexName, false);
        }
      //}
      //catch (Exception ex)
      //{
      //  throw new FaultException(ex.Message);
      //}
    }

    /// <summary>
    /// Retrieves the document with the corresponding document id.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public JsonDocumentDto Retrieve(string indexName, string documentId)
    {
      try
      {
        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;
        using (index.GetSearcher(out indexSearcher))
        {
          var term = new Lucene.Net.Index.Term(Constants.DocumentIdFieldName, documentId.ToLowerInvariant());

          var termQuery = new TermQuery(term);

          var hits = indexSearcher.Search(termQuery, 1);

          if (hits.TotalHits == 0)
            return null;

          var result = RetrieveSearchResults(indexSearcher, hits).FirstOrDefault();

          return result == null
            ? null
            : result.Document;
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    public IList<SearchResult> IntRangeQuery(string indexName, string fieldName, int? min, int? max, bool minInclusive, bool maxInclusive, int maxResults)
    {
      try
      {
        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;
        using (index.GetSearcher(out indexSearcher))
        {
          var rangeQuery = Lucene.Net.Search.NumericRangeQuery.NewIntRange(fieldName, min, max, minInclusive, maxInclusive);

          var hits = indexSearcher.Search(rangeQuery, maxResults);
          return RetrieveSearchResults(indexSearcher, hits);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    public IList<SearchResult> FloatRangeQuery(string indexName, string fieldName, float? min, float? max, bool minInclusive,
                                               bool maxInclusive, int maxResults)
    {
      try
      {
        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;
        using (index.GetSearcher(out indexSearcher))
        {
          var rangeQuery = Lucene.Net.Search.NumericRangeQuery.NewFloatRange(fieldName, min, max, minInclusive, maxInclusive);

          var hits = indexSearcher.Search(rangeQuery, maxResults);
          return RetrieveSearchResults(indexSearcher, hits);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// Returns documents that match the specified lucene query, limiting to the specified number of items.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="query"></param>
    /// <param name="maxResults"></param>
    /// <returns></returns>
    public IList<SearchResult> SearchWithQuery(string indexName, Barista.Search.Query query, int maxResults)
    {
      try
      {
        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;
        using (index.GetSearcher(out indexSearcher))
        {
          var lQuery = Barista.Search.Query.ConvertQueryToLuceneQuery(query);
          var hits = indexSearcher.Search(lQuery, maxResults);
          return RetrieveSearchResults(indexSearcher, hits);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// Returns documents that match the specified lucene query, limiting to the specified number of items.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="defaultField"></param>
    /// <param name="query"></param>
    /// <param name="maxResults"></param>
    /// <returns></returns>
    public IList<SearchResult> SearchWithQueryParser(string indexName, string defaultField, string query, int maxResults)
    {
      try
      {
        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;
        using (index.GetSearcher(out indexSearcher))
        {
          var lQuery = query.IsNullOrWhiteSpace()
                         ? new Lucene.Net.Search.MatchAllDocsQuery()
                         : LuceneModelFilter.ParseQuery(defaultField, query);
          
          var hits = indexSearcher.Search(lQuery, maxResults);
          return RetrieveSearchResults(indexSearcher, hits);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// Returns documents that matches the specified OData-based query.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="defaultField"></param>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public IList<SearchResult> SearchWithOData(string indexName, string defaultField, string queryString)
    {
      try
      {
        var parser = new ODataQueryParser();
        var filter = parser.ParseQuery(defaultField, queryString);

        var index = GetOrAddIndex(indexName, false);
        IndexSearcher indexSearcher;

        using (index.GetSearcher(out indexSearcher))
        {
          var lQuery = filter.Query.IsNullOrWhiteSpace()
                         ? new Lucene.Net.Search.MatchAllDocsQuery()
                         : LuceneModelFilter.ParseQuery(defaultField, filter.Query);

          var lFilter = filter.Filter.IsNullOrWhiteSpace()
                         ? null
                         : LuceneModelFilter.ParseFilter(defaultField, filter.Filter);

          var lSort = filter.Sort ?? new Sort(SortField.FIELD_SCORE);

          if (filter.Skip <= 0 && filter.Take > 0)
          {
            var hits = indexSearcher.Search(lQuery, lFilter, filter.Take, lSort);
            return RetrieveSearchResults(indexSearcher, hits)
              .Take(filter.Take)
              .ToList();
          }

          if (filter.Skip > 0 && filter.Take > 0)
          {
            var hits = indexSearcher.Search(lQuery, lFilter, filter.Skip + filter.Take, lSort);
            return RetrieveSearchResults(indexSearcher, hits)
              .Skip(filter.Skip)
              .Take(filter.Take)
              .ToList();
          }


          var allHits = indexSearcher.Search(lQuery, lFilter, 1024, lSort);
          return RetrieveSearchResults(indexSearcher, allHits);
        }
      }
      catch (Exception ex)
      {
        throw new FaultException(ex.Message);
      }
    }

    /// <summary>
    /// When implemented in a concrete class, returns the lucene directory that corresponds to the specified name.
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    protected abstract Lucene.Net.Store.Directory GetLuceneDirectoryFromIndexName(string indexName);

    protected Index GetOrAddIndex(string indexName, bool createIfMissing)
    {
      return Indexes.GetOrAdd(indexName, key =>
        {
          var targetDirectory = GetLuceneDirectoryFromIndexName(indexName);

          var indexDefinition = new IndexDefinition();

          if (!IndexReader.IndexExists(targetDirectory))
          {
            if (createIfMissing == false)
              throw new InvalidOperationException("Index does not exist: " + targetDirectory);

            WriteIndexVersion(targetDirectory);

            //creating index structure if we need to
            new IndexWriter(targetDirectory, DummyAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED).Dispose();
          }
          else
          {
            EnsureIndexVersionMatches(indexDefinition.Name, targetDirectory);

            if (targetDirectory.FileExists("write.lock"))// force lock release, because it was still open when we shut down
            {
              IndexWriter.Unlock(targetDirectory);
              // for some reason, just calling unlock doesn't remove this file
              targetDirectory.DeleteFile("write.lock");
            }

            if (targetDirectory.FileExists("writing-to-index.lock")) // we had an unclean shutdown
            {
              //if (configuration.ResetIndexOnUncleanShutdown)
              //  throw new InvalidOperationException("Rude shutdown detected on: " + indexDirectory);

              CheckIndexAndRecover(targetDirectory, indexName);
              targetDirectory.DeleteFile("writing-to-index.lock");
            }
          }

          var simpleIndex = new SimpleIndex(targetDirectory, "DefaultIndex", indexDefinition);
          return simpleIndex;
        });
    }

    private static IList<SearchResult> RetrieveSearchResults(IndexSearcher indexSearcher, TopDocs hits)
    {
      //iterate over the results.
      var results = hits.ScoreDocs.AsQueryable()
                 .OrderByDescending(hit => hit.Score)
                 .ToList()
                 .Select(hit =>
                 {
                   var jsonDocumentField = indexSearcher.Doc(hit.Doc).GetField(Constants.JsonDocumentFieldName);

                   if (jsonDocumentField == null)
                     return new SearchResult
                     {
                       Score = hit.Score,
                       Document = null
                     };

                   return new SearchResult
                   {
                     Score = hit.Score,
                     Document = JsonConvert.DeserializeObject<JsonDocumentDto>(jsonDocumentField.StringValue)
                   };
                 })
                 .ToList();

      return results;
    }

    private static void EnsureIndexVersionMatches(string indexName, Lucene.Net.Store.Directory directory)
    {
      if (directory.FileExists("index.version") == false)
      {
        throw new InvalidOperationException("Could not find index.version " + indexName + ", resetting index");
      }
      using (var indexInput = directory.OpenInput("index.version"))
      {
        var versionFromDisk = indexInput.ReadString();
        if (versionFromDisk != IndexVersion)
          throw new InvalidOperationException("Index " + indexName + " is of version " + versionFromDisk +
                            " which is not compatible with " + IndexVersion + ", resetting index");
      }
    }

    private static void CheckIndexAndRecover(Lucene.Net.Store.Directory directory, string indexName)
    {
      StartupLog.Warn("Unclean shutdown detected on {0}, checking the index for errors. This may take a while.", indexName);

      var memoryStream = new MemoryStream();
      var stringWriter = new StreamWriter(memoryStream);
      var checkIndex = new CheckIndex(directory);

      if (StartupLog.IsWarnEnabled)
        checkIndex.SetInfoStream(stringWriter);

      var sp = Stopwatch.StartNew();
      var status = checkIndex.CheckIndex_Renamed_Method();
      sp.Stop();
      if (StartupLog.IsWarnEnabled)
      {
        StartupLog.Warn("Checking index {0} took: {1}, clean: {2}", indexName, sp.Elapsed, status.clean);
        memoryStream.Position = 0;

        Log.Warn(new StreamReader(memoryStream).ReadToEnd());
      }

      if (status.clean)
        return;

      StartupLog.Warn("Attempting to fix index: {0}", indexName);
      sp.Stop();
      sp.Reset();
      sp.Start();
      checkIndex.FixIndex(status);
      StartupLog.Warn("Fixed index {0} in {1}", indexName, sp.Elapsed);
    }

    private static void WriteIndexVersion(Lucene.Net.Store.Directory directory)
    {
      using (var indexOutput = directory.CreateOutput("index.version"))
      {
        indexOutput.WriteString(IndexVersion);
        indexOutput.Flush();
      }
    }

    public static void CloseAllIndexes()
    {
      foreach (var kvp in Indexes)
      {
        CloseIndexWriter(kvp.Key, true);
      }
    }

    public static void CloseIndexWriter(string indexName, bool waitForMerges)
    {
      Index index;
      if (Indexes.TryRemove(indexName, out index))
      {
        index.Dispose();
      }
    }

    public void Dispose()
    {
      CloseAllIndexes();
    }
  }
}
