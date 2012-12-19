namespace Barista.SharePoint.Search
{
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Documents;
  using Lucene.Net.Index;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Microsoft.SharePoint;
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
    /// <summary>
    /// Adds the index of the object to the index that is contained in the specified folder
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="create"></param>
    /// <param name="obj">The obj.</param>
    public static void AddObjectToIndex(SPFolder targetFolder, bool create, object obj)
    {
      var directory = new SPDirectory(targetFolder);

      var doc = ConvertObjectToDocument(obj);

      var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

      using (var writer = new IndexWriter(directory, analyzer, create, IndexWriter.MaxFieldLength.UNLIMITED))
      {
        writer.AddDocument(doc);
      }
    }

    /// <summary>
    /// Adds the index of the object to the index that is contained in the specified folder
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="jObj">The obj.</param>
    public static void AddObjectToIndex(SPFolder targetFolder, bool create, JObject jObj)
    {
      var directory = new SPDirectory(targetFolder);

      var doc = ConvertObjectToDocument(jObj);

      var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

      using (var writer = new IndexWriter(directory, analyzer, create, IndexWriter.MaxFieldLength.UNLIMITED))
      {
        writer.AddDocument(doc);
      }
    }

    /// <summary>
    /// Searches the specified target folder.
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="query">The query.</param>
    /// <returns>IList{Document}.</returns>
    public static IList<Document> Search(SPFolder targetFolder, string query)
    {
      var directory = new SPDirectory(targetFolder);

      //create an index searcher that will perform the search
      using (var searcher = new IndexSearcher(directory))
      {
        var parser = new QueryParser(Version.LUCENE_30, "contents", new SimpleAnalyzer());
        var lQuery = parser.Parse(query);

        var hits = searcher.Search(lQuery, 100);

        //iterate over the results.
        return hits.ScoreDocs.AsQueryable()
                   .OrderByDescending(hit => hit.Score)
                   .Select(hit => searcher.Doc(hit.Doc))
                   .ToList();
      }
    }

    /// <summary>
    /// Searches the specified target folder.
    /// </summary>
    /// <param name="targetFolder">The target folder.</param>
    /// <param name="searchTerms">The search terms.</param>
    /// <returns>IList{Document}.</returns>
    public static IList<Document> Search(SPFolder targetFolder, IDictionary<string, string> searchTerms)
    {
      var directory = new SPDirectory(targetFolder);

      //create an index searcher that will perform the search
      using (var searcher = new IndexSearcher(directory))
      {
        //build a query object
        var termQueries = searchTerms
          .Select(kvp => new TermQuery(new Term(kvp.Key, kvp.Value)))
          .ToList();

        //TODO: Use more than the first search Term

        //execute the query
        var hits = searcher.Search(termQueries.First(), 100);

        //iterate over the results.
        return hits.ScoreDocs.AsQueryable()
                   .OrderByDescending(hit => hit.Score)
                   .Select(hit => searcher.Doc(hit.Doc))
                   .ToList();
      }
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
      var tokenDictionary = new Dictionary<string, Field>();
      TokenizeObject(jObj, "", ref tokenDictionary);

      //Add the full json doc as a "contents" field.
      var contentsTxt = JsonConvert.SerializeObject(obj);
      var contentsField = new Field("contents", contentsTxt, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO);
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

      var tokenDictionary = new Dictionary<string, Field>();
      TokenizeObject(jObj, "", ref tokenDictionary);

      //Add the full json doc as a "contents" field.
      var contentsTxt = jObj.ToString();
      var contentsField = new Field("contents", contentsTxt, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO);
      doc.Add(contentsField);

      //Add individual fields.
      foreach (var kvp in tokenDictionary)
      {
        doc.Add(kvp.Value);
      }

      return doc;
    }

    /// <summary>
    /// Recursively tokenize the object into fields whose names are available via dot notation.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    /// <param name="dictionary"></param>
    private static void TokenizeObject(JObject obj, string prefix, ref Dictionary<string, Field> dictionary)
    {
      if (obj == null)
        return;

      //TODO: Add attribute-based conventions that allow the parameters to be customized.
      foreach (var property in obj.Properties().Where(p => p.Value != null))
      {
        var fieldName = String.IsNullOrEmpty(prefix) ? property.Name : prefix + "." + property.Name;
        switch (property.Value.Type)
        {
          case JTokenType.Object:
            TokenizeObject(property.Value as JObject, fieldName, ref dictionary);
            break;
          default:
            if (String.IsNullOrEmpty(property.Value.ToString()) == false)
            {
              var stringField = new Field(fieldName, property.Value.ToString(), Field.Store.YES, Field.Index.ANALYZED,
                                    Field.TermVector.YES);
              dictionary.Add(fieldName, stringField);
            }
            break;
        }
      }
    }
  }
}
