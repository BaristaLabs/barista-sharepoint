namespace Barista.Services
{
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Extensions;
  using Barista.Newtonsoft.Json;
  using Barista.Newtonsoft.Json.Linq;
  using Barista.Search;
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Index;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Lucene.Net.Store;
  using System;
  using System.Collections.Concurrent;
  using System.IO;
  using System.ServiceModel;
  using Version = Lucene.Net.Util.Version;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public class BaristaSearchService : IBaristaSearch, IDisposable
  {
    private const string IndexVersion = "1.0.0.0";
    private static readonly Analyzer DummyAnalyzer = new SimpleAnalyzer();
    private static readonly ConcurrentDictionary<DirectoryDefinition, Index> Indexes = new ConcurrentDictionary<DirectoryDefinition, Index>();

    /// <summary>
    /// Deletes all documents from the specified index.
    /// </summary>
    /// <param name="definition"></param>
    public void DeleteAllDocuments(DirectoryDefinition definition)
    {
      var index = GetOrAddIndex(definition, true);
      try
      {
        index.DeleteAll();
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    public void DeleteDocuments(DirectoryDefinition definition, IEnumerable<string> keys)
    {
      var index = GetOrAddIndex(definition, true);

      try
      {
        //Remove the documents from the index
        index.Remove(keys.ToArray());
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    /// <summary>
    /// Indexes the specified document.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="document"></param>
    public void IndexDocument(DirectoryDefinition definition, Document document)
    {
      if (document == null)
        throw new InvalidOperationException("A document must be specified.");

      var index = GetOrAddIndex(definition, true);

      try
      {
        //Add it to the index.
        throw new NotImplementedException();
        //var batch = new IndexingBatch();
        //batch.Add(document, false);
        //index.IndexDocuments(batch);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    /// <summary>
    /// Indexes the specified document.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="document"></param>
    public void IndexJsonDocument(DirectoryDefinition definition, JsonDocument document)
    {
      if (document == null)
        throw new ArgumentNullException("document", @"A document must be specified.");

      IndexJsonDocuments(definition, new List<JsonDocument> { document });
    }

    public void IndexJsonDocuments(DirectoryDefinition definition, IEnumerable<JsonDocument> documents)
    {
      if (documents == null)
        throw new ArgumentNullException("documents", @"A collection of documents must be specified.");

      var jsonDocuments = documents as IList<JsonDocument> ?? documents.ToList();

      if (jsonDocuments.Any() == false)
        throw new ArgumentNullException("documents", @"At least one document must be contained within the collection.");

      var index = GetOrAddIndex(definition, true);

      try
      {
        //Add it to the index.
        var batch = new IndexingBatch();

        //Attempt to create a new Search.JsonDocument from the document
        foreach (var searchDocument in jsonDocuments.Select(document => new Search.JsonDocument
          {
            DocumentId = document.DocumentId,
            Metadata = document.MetadataAsJson.IsNullOrWhiteSpace() == false
                         ? JObject.Parse(document.MetadataAsJson)
                         : new JObject(),
            DataAsJson = JObject.Parse(document.DataAsJson)
          }))
        {
          batch.Add(searchDocument, false);
        }

        index.IndexDocuments(batch);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    public JsonDocument Retrieve(DirectoryDefinition definition, string documentId)
    {
      var index = GetOrAddIndex(definition, false);
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

    public IList<SearchResult> Search(DirectoryDefinition definition, string defaultField, string query, int maxResults)
    {
      var index = GetOrAddIndex(definition, false);
      IndexSearcher indexSearcher;
      using (index.GetSearcher(out indexSearcher))
      {
        var parser = new QueryParser(Version.LUCENE_30, defaultField, new StandardAnalyzer(Version.LUCENE_30));
        var lQuery = parser.Parse(query);

        var hits = indexSearcher.Search(lQuery, maxResults);

        return RetrieveSearchResults(indexSearcher, hits);
      }
    }

    public IList<SearchResult> SearchOData(DirectoryDefinition definition, string defaultField, IDictionary<string, string> filterParameters)
    {
      throw new NotImplementedException();
    }

    public static Index GetOrAddIndex(DirectoryDefinition definition, bool createIndex)
    {
      return Indexes.GetOrAdd(definition, key =>
        {
          var di = new DirectoryInfo(key.IndexStoragePath);
          var targetDirectory = new RAMDirectory();
          
          var indexDefinition = new IndexDefinition();

          if (!IndexReader.IndexExists(targetDirectory))
          {
            //if (createIfMissing == false)
            //  throw new InvalidOperationException("Index does not exists: " + indexDirectory);

            WriteIndexVersion(targetDirectory);

            //creating index structure if we need to
            new IndexWriter(targetDirectory, DummyAnalyzer, IndexWriter.MaxFieldLength.UNLIMITED).Dispose();
          }
          else
          {
            //EnsureIndexVersionMatches(indexName, directory);

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

              //CheckIndexAndRecover(directory, indexDirectory);
              targetDirectory.DeleteFile("writing-to-index.lock");
            }
          }

          //switch (indexDefinition.DirectoryType)
          //{
          //  case DirectoryType.SharePointDirectory:
          //    //targetDirectory = new SPDirectory(definition.DirectoryUri);
          //    throw new NotImplementedException();
          //    break;
          //  case DirectoryType.SimpleFileSystemDirectory:
          //    var targetDirectoryInfo = new DirectoryInfo(indexDefinition.DirectoryUri);
          //    targetDirectory = new SimpleFSDirectory(targetDirectoryInfo);
          //    break;
          //  default:
          //    throw new ArgumentOutOfRangeException("Unknown or unsupported Directory Type: " + indexDefinition.DirectoryType);
          //}

          
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
                     Document = JsonConvert.DeserializeObject<JsonDocument>(jsonDocumentField.StringValue)
                   };
                 })
                 .ToList();

      return results;
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

    public static void CloseIndexWriter(DirectoryDefinition definition, bool waitForMerges)
    {
      Index index;
      if (Indexes.TryRemove(definition, out index))
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
