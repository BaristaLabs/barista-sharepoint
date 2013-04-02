namespace Barista.Services
{
  using System.Collections.Generic;
  using System.Linq;
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
  using Directory = Lucene.Net.Store.Directory;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public class BaristaSearchService : IBaristaSearch
  {
    private static readonly ConcurrentDictionary<IndexDefinition, IndexWriter> IndexWriters = new ConcurrentDictionary<IndexDefinition, IndexWriter>();

    /// <summary>
    /// Deletes all documents from the specified index.
    /// </summary>
    /// <param name="definition"></param>
    public void DeleteAll(IndexDefinition definition)
    {
      var indexWriter = GetOrAddIndexWriter(definition, true);
      try
      {
        indexWriter.DeleteAll();
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    /// <summary>
    /// Commits all pending changes to the specified index and syncs all referenced index files.
    /// </summary>
    /// <param name="definition"></param>
    public void Commit(IndexDefinition definition)
    {
      var indexWriter = GetOrAddIndexWriter(definition, true);
      try
      {
        indexWriter.Commit();
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    /// <summary>
    /// Adds a document to the specified index.
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="document"></param>
    public void AddDocumentToIndex(IndexDefinition definition, Document document)
    {
      if (document == null)
        throw new InvalidOperationException("A document must be specified.");

      if (document.Fields == null || !document.Fields.Any())
        throw new InvalidOperationException("A document must have at least one field specified.");

      var indexWriter = GetOrAddIndexWriter(definition, true);

      var luceneDocument = Document.ConvertToLuceneDocument(document);

      try
      {
        //Add it to the index.
        indexWriter.AddDocument(luceneDocument);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    /// <summary>
    /// Updates 
    /// </summary>
    /// <param name="definition"></param>
    /// <param name="term"></param>
    /// <param name="document"></param>
    public void UpdateDocumentInIndex(IndexDefinition definition, Term term, Document document)
    {
      var indexWriter = GetOrAddIndexWriter(definition, true);

      //Convert WCF document to Lucene document
      var luceneDocument = Document.ConvertToLuceneDocument(document);

      //Convert WCF term to Lucene term
      var luceneTerm = Term.ConvertToLuceneTerm(term);

      try
      {
        //Update the index.
        indexWriter.UpdateDocument(luceneTerm, luceneDocument);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    public void DeleteDocuments(IndexDefinition definition, Term term)
    {
      var indexWriter = GetOrAddIndexWriter(definition, true);

      var luceneTerm = Term.ConvertToLuceneTerm(term);

      try
      {
        //Remove the documents from the index
        indexWriter.DeleteDocuments(luceneTerm);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(definition, false);
      }
    }

    public IList<Hit> Search(IndexDefinition definition, string defaultField, string query, int maxResults)
    {
      var indexWriter = GetOrAddIndexWriter(definition, false);
      var indexReader = indexWriter.GetReader();
      var indexSearcher = new IndexSearcher(indexReader);
      try
      {
        var parser = new QueryParser(Version.LUCENE_30, defaultField, new StandardAnalyzer(Version.LUCENE_30));
        var lQuery = parser.Parse(query);

        var hits = indexSearcher.Search(lQuery, maxResults);

        //iterate over the results.
        var results = hits.ScoreDocs.AsQueryable()
                   .OrderByDescending(hit => hit.Score)
                   .Select(hit => new Hit
                   {
                     Score = hit.Score,
                     DocumentId = hit.Doc,
                     //Document = indexSearcher.Doc(hit.Doc)
                   })
                   .ToList();
        return results;
      }
      finally
      {
        indexSearcher.Dispose();
      }
    }

    public IList<Hit> SearchOData(IndexDefinition definition, string defaultField, IDictionary<string, string> filterParameters)
    {
      throw new NotImplementedException();
    }

    public static IndexWriter GetOrAddIndexWriter(IndexDefinition definition, bool createIndex)
    {
      return IndexWriters.GetOrAdd(definition, indexDefinition =>
        {
          Directory targetDirectory;
          switch (indexDefinition.DirectoryType)
          {
            case DirectoryType.SharePointDirectory:
              //targetDirectory = new SPDirectory(definition.DirectoryUri);
              throw new NotImplementedException();
              break;
            case DirectoryType.SimpleFileSystemDirectory:
              var targetDirectoryInfo = new DirectoryInfo(indexDefinition.DirectoryUri);
              targetDirectory = new SimpleFSDirectory(targetDirectoryInfo);
              break;
            default:
              throw new ArgumentOutOfRangeException("Unknown or unsupported Directory Type: " + indexDefinition.DirectoryType);
          }
          var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
          var writer = new IndexWriter(targetDirectory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED);
          return writer;
        });
    }

    public static void CloseAllIndexWriters()
    {
      foreach (var kvp in IndexWriters)
      {
        CloseIndexWriter(kvp.Key, true);
      }
    }

    public static void CloseIndexWriter(IndexDefinition definition, bool waitForMerges)
    {
      IndexWriter indexWriter;
      if (IndexWriters.TryRemove(definition, out indexWriter))
      {
        try
        {
          indexWriter.Dispose(waitForMerges);
        }
        catch (OutOfMemoryException)
        {
          indexWriter.Dispose(false);
        }
      }
    }
  }
}
