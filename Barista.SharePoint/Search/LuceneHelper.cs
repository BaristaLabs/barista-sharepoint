namespace Barista.SharePoint.Search
{
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Documents;
  using Lucene.Net.Index;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Lucene.Net.Search.Vectorhighlight;
  using Lucene.Net.Store;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Utilities;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Version = Lucene.Net.Util.Version;

  /// <summary>
  /// Contains helper methods for interacting with Lucene.
  /// </summary>
  public static class LuceneHelper
  {
    //TODO: This will fail in a scenario where the Barista Serivce Application is installed on multiple servers in the farm.
    //The first IndexWriter obtained will lock and block the second index writer.
    //this should be switched on over to a mechanism that ensures a farm-wide singleton.
    public static readonly Dictionary<string, IndexWriter> IndexWriters = new Dictionary<string, IndexWriter>();

    public static IndexWriter GetIndexWriterSingleton(SPFolder targetFolder, bool createIndex)
    {
      var fullFolderUrl = SPUtility.ConcatUrls(targetFolder.ParentWeb.Url, targetFolder.ServerRelativeUrl);
      if (IndexWriters.ContainsKey(fullFolderUrl))
      {
        lock (IndexWriters)
        {
          if (IndexWriters.ContainsKey(fullFolderUrl))
          {
            return IndexWriters[fullFolderUrl];
          }
        }
      }

      var directory = new SPDirectory(targetFolder);

      //Block until a write lock is no longer present on the target folder.
      var spLock = directory.MakeLock(directory.GetLockId() + "-write");
      spLock.Obtain(Lock.LOCK_OBTAIN_WAIT_FOREVER);
      spLock.Release();

      var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
      var writer = new IndexWriter(directory, analyzer, createIndex, IndexWriter.MaxFieldLength.UNLIMITED);

      if (IndexWriters.ContainsKey(fullFolderUrl) == false)
      {
        lock (IndexWriters)
        {
          if (IndexWriters.ContainsKey(fullFolderUrl) == false)
          {
            IndexWriters.Add(fullFolderUrl, writer);
          }
        }
      }

      return IndexWriters[fullFolderUrl];
    }

    /// <summary>
    /// Utility method to get the most-relevant index searcher. If a index writer singleton exists for the target folder, uses that instance, otherwise returns a new index searcher. 
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <returns>IndexSearcher.</returns>
    public static IndexSearcher GetIndexSearcher(SPFolder targetFolder)
    {
      IndexSearcher searcher;
      if (HasIndexWriterSingleton(targetFolder))
      {
        var indexWriter = GetIndexWriterSingleton(targetFolder, false);
        var reader = indexWriter.GetReader();
        searcher = new IndexSearcher(reader);
      }
      else
      {
        var directory = new SPDirectory(targetFolder);
        searcher = new IndexSearcher(directory);
      }

      return searcher;
    }

    /// <summary>
    /// Utility method to get a default FastVectorHighlighter.
    /// </summary>
    /// <returns></returns>
    public static FastVectorHighlighter GetFastVectorHighlighter()
    {
      FragListBuilder fragListBuilder = new SimpleFragListBuilder();
      FragmentsBuilder fragmentBuilder = new ScoreOrderFragmentsBuilder(BaseFragmentsBuilder.COLORED_PRE_TAGS, BaseFragmentsBuilder.COLORED_POST_TAGS);
      return new FastVectorHighlighter(true, true, fragListBuilder, fragmentBuilder);
    }

    /// <summary>
    /// Utility method to get the most-relevant simple faceted search.
    /// </summary>
    /// <param name="targetFolder"></param>
    /// <param name="groupByFields"></param>
    /// <returns></returns>
    public static SimpleFacetedSearch GetSimpleFacetedSearch(SPFolder targetFolder, params string[] groupByFields)
    {
      SimpleFacetedSearch facetedSearch;
      if (HasIndexWriterSingleton(targetFolder))
      {
        var indexWriter = GetIndexWriterSingleton(targetFolder, false);
        var reader = indexWriter.GetReader();
        facetedSearch = new SimpleFacetedSearch(reader, groupByFields);
      }
      else
      {
        var directory = new SPDirectory(targetFolder);
        var reader = IndexReader.Open(directory, true);
        facetedSearch = new SimpleFacetedSearch(reader, groupByFields);
      }

      return facetedSearch;
    }

    /// <summary>
    /// Returns a value that indicates if a index writer singleton has been created for the target folder.
    /// </summary>
    /// <param name="targetFolder"></param>
    /// <returns></returns>
    public static bool HasIndexWriterSingleton(SPFolder targetFolder)
    {
      var fullFolderUrl = SPUtility.ConcatUrls(targetFolder.ParentWeb.Url, targetFolder.ServerRelativeUrl);
      return IndexWriters.ContainsKey(fullFolderUrl);
    }

    /// <summary>
    /// Utility method to add the the object to the index using the specified index writer.
    /// </summary>
    /// <param name="indexWriter"></param>
    /// <param name="obj">The obj.</param>
    public static void AddObjectToIndex(IndexWriter indexWriter, object obj)
    {
      var doc = ConvertObjectToDocument(obj);

      indexWriter.AddDocument(doc);
    }

    /// <summary>
    /// Utility method to add the JObject to the index using the specified index writer.
    /// </summary>
    /// <param name="indexWriter"></param>
    /// <param name="jObj">The obj.</param>
    public static void AddObjectToIndex(IndexWriter indexWriter, JObject jObj)
    {
      var doc = ConvertObjectToDocument(jObj);

      indexWriter.AddDocument(doc);
    }

    /// <summary>
    /// Utility method to add the list item to the index using the specified index writer.
    /// </summary>
    /// <param name="indexWriter">The index writer.</param>
    /// <param name="listItem">The list item.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    public static void AddListItemToIndex(IndexWriter indexWriter, SPListItem listItem)
    {
      var doc = ConvertListItemToDocument(listItem);

      indexWriter.AddDocument(doc);
    }

    /// <summary>
    /// Closes the index writer singleton.
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    public static void CloseIndexWriterSingleton(SPFolder targetFolder)
    {
      var fullFolderUrl = SPUtility.ConcatUrls(targetFolder.ParentWeb.Url, targetFolder.ServerRelativeUrl);

      IndexWriter indexWriter = null;
      if (IndexWriters.ContainsKey(fullFolderUrl))
      {
        lock (IndexWriters)
        {
          if (IndexWriters.ContainsKey(fullFolderUrl))
          {
            indexWriter = IndexWriters[fullFolderUrl];
            IndexWriters.Remove(fullFolderUrl);
          }
        }
      }

      if (indexWriter != null)
        indexWriter.Dispose();
    }

    /// <summary>
    /// Converts the specified object to a Lucene Document that is capable of being indexed.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Document ConvertObjectToDocument(object obj)
    {
      var doc = new Document();

      var jObj = JObject.FromObject(obj);
      var tokenDictionary = new Dictionary<string, AbstractField>();
      TokenizeObject(jObj, "", ref tokenDictionary);

      //Add the full json doc as a "_contents" field.
      var contentsTxt = JsonConvert.SerializeObject(obj);
      var contentsField = new Field("_contents", contentsTxt, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO);
      doc.Add(contentsField);

      //Add individual fields.
      foreach (var kvp in tokenDictionary)
      {
        doc.Add(kvp.Value);
      }

      return doc;
    }

    /// <summary>
    /// Converts the object to document.
    /// </summary>
    /// <param name="jObj">The obj.</param>
    /// <returns>Document.</returns>
    public static Document ConvertObjectToDocument(JObject jObj)
    {
      var doc = new Document();

      var tokenDictionary = new Dictionary<string, AbstractField>();
      TokenizeObject(jObj, "", ref tokenDictionary);

      //Add the full json doc as a "contents" field.
      var contentsTxt = jObj.ToString();
      var contentsField = new Field("_contents", contentsTxt, Field.Store.YES, Field.Index.NO, Field.TermVector.NO);
      doc.Add(contentsField);

      //Add individual fields.
      foreach (var kvp in tokenDictionary)
      {
        doc.Add(kvp.Value);
      }

      return doc;
    }

    /// <summary>
    /// Converts the list item to document.
    /// </summary>
    /// <param name="listItem">The list item.</param>
    /// <returns>Document.</returns>
    public static Document ConvertListItemToDocument(SPListItem listItem)
    {
      var tokenDictionary = new Dictionary<string, AbstractField>();

      foreach (var field in listItem.Fields.OfType<SPField>().Where(f => f.Hidden == false))
      {
        switch (field.Type)
        {
          case SPFieldType.DateTime:
             var dateString = DateTools.DateToString((DateTime) listItem[field.Id], DateTools.Resolution.MILLISECOND);
            var dateField = new Field(field.Title, dateString, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.YES);

            tokenDictionary.Add(field.Title, dateField);
            break;
          case SPFieldType.Integer:
            var intField = new NumericField(field.Title, Field.Store.YES, true);
            intField.SetIntValue((int)listItem[field.Id]);
            tokenDictionary.Add(field.Title, intField);
            break;
          case SPFieldType.Number:
            var numberField = new NumericField(field.Title, Field.Store.YES, true);
            numberField.SetFloatValue(Convert.ToSingle(listItem[field.Id]));
            tokenDictionary.Add(field.Title, numberField);
            break;
          default:
            var textValue = field.GetFieldValueAsText(listItem[field.Id]);
            var stringField = new Field(field.Title, textValue, Field.Store.YES, Field.Index.ANALYZED,
                                    Field.TermVector.WITH_POSITIONS_OFFSETS);
            tokenDictionary.Add(field.Title, stringField);
            break;
        }
      }

      var doc = new Document();

      //Add individual fields.
      foreach (var kvp in tokenDictionary)
      {
        doc.Add(kvp.Value);
      }

      return doc;
    }

    /// <summary>
    /// Utility method to search the specified target folder via a query parser.
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="query">The query.</param>
    /// <param name="n"></param>
    /// <returns>IList{Document}.</returns>
    public static IList<Hit> Search(SPFolder targetFolder, string query, int n)
    {
      var searcher = GetIndexSearcher(targetFolder);
      try
      {
        var parser = new QueryParser(Version.LUCENE_30, "_contents", new StandardAnalyzer(Version.LUCENE_30));
        var lQuery = parser.Parse(query);

        var hits = searcher.Search(lQuery, n);

        //iterate over the results.
        return hits.ScoreDocs.AsQueryable()
                   .OrderByDescending(hit => hit.Score)
                   .Select(hit => new Hit
                   {
                     Score = hit.Score,
                     DocumentId = hit.Doc,
                     Document = searcher.Doc(hit.Doc)
                   })
                   .ToList();
      }
      finally
      {
        searcher.Dispose();
      }
    }

    /// <summary>
    /// Utility method to remove all documents from the index using the specified writer.
    /// </summary>
    /// <param name="writer"></param>
    public static void RemoveAllDocumentsFromIndex(IndexWriter writer)
    {
      Query query = new MatchAllDocsQuery();

      writer.DeleteDocuments(query);
    }

    /// <summary>
    /// Recursively tokenize the object into fields whose names are available via dot notation.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    /// <param name="dictionary"></param>
    private static void TokenizeObject(JObject obj, string prefix, ref Dictionary<string, AbstractField> dictionary)
    {
      if (obj == null)
        return;

      //TODO: Add property-based ("$propertyName") conventions that allow the parameters to be customized.
      foreach (var property in obj.Properties().Where(p => p.Value != null))
      {
        var fieldName = String.IsNullOrEmpty(prefix) ? property.Name : prefix + "." + property.Name;
        switch (property.Value.Type)
        {
          case JTokenType.Date:
            var dateString = DateTools.DateToString((DateTime) property.Value, DateTools.Resolution.MILLISECOND);
            var dateField = new Field(fieldName, dateString, Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.YES);

            dictionary.Add(fieldName, dateField);
            break;
          case JTokenType.TimeSpan:
            var timeSpanField = new NumericField(fieldName, Field.Store.YES, true);
            timeSpanField.SetLongValue(((TimeSpan)property.Value).Ticks);
            dictionary.Add(fieldName, timeSpanField);
            break;
          case JTokenType.Integer:
            var intField = new NumericField(fieldName, Field.Store.YES, true);
            intField.SetIntValue((int)property.Value);
            dictionary.Add(fieldName, intField);
            break;
          case JTokenType.Float:
            var floatField = new NumericField(fieldName, Field.Store.YES, true);
            floatField.SetFloatValue((float)property.Value);
            dictionary.Add(fieldName, floatField);
            break;
          case JTokenType.Guid:
            var guidField = new Field(fieldName, property.Value.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED,
                                    Field.TermVector.NO);
              dictionary.Add(fieldName, guidField);
            break;
          case JTokenType.Object:
            TokenizeObject(property.Value as JObject, fieldName, ref dictionary);
            break;
          default:
            if (String.IsNullOrEmpty(property.Value.ToString()) == false)
            {
              var stringField = new Field(fieldName, property.Value.ToString(), Field.Store.YES, Field.Index.ANALYZED,
                                    Field.TermVector.WITH_POSITIONS_OFFSETS);
              dictionary.Add(fieldName, stringField);
            }
            break;
        }
      }
    }
  }
}
