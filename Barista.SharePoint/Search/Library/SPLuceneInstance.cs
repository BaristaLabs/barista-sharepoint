using Barista.SharePoint.Library;
using Lucene.Net.Analysis;
using Microsoft.SharePoint.Utilities;

namespace Barista.SharePoint.Search.Library
{
  using Barista.SharePoint.Search;
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Index;
  using Lucene.Net.QueryParsers;
  using Lucene.Net.Search;
  using Microsoft.SharePoint;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  [Serializable]
  public class SPLuceneInstance : ObjectInstance
  {
    //TODO: This will fail in a scenario where the Barista Serivce Application is installed on multiple servers in the farm.
    //The first IndexWriter obtained will lock and block the second index writer.
    //this should be switched on over to a mechanism that ensures a farm-wide singleton.
    public static readonly Dictionary<string, IndexWriter> IndexWriters = new Dictionary<string, IndexWriter>();

    public SPLuceneInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "hasIndexWriter")]
    public bool HasIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      var fullFolderUrl = SPUtility.ConcatUrls(targetFolder.ParentWeb.Url, targetFolder.ServerRelativeUrl);
      return IndexWriters.ContainsKey(fullFolderUrl);
    }

    [JSFunction(Name = "getIndexWriter")]
    public IndexWriterInstance GetIndexWriter(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var fullFolderUrl = SPUtility.ConcatUrls(targetFolder.ParentWeb.Url, targetFolder.ServerRelativeUrl);
      if (IndexWriters.ContainsKey(fullFolderUrl))
      {
        lock (IndexWriters)
        {
          if (IndexWriters.ContainsKey(fullFolderUrl))
          {
            return new IndexWriterInstance(this.Engine.Object.InstancePrototype, IndexWriters[fullFolderUrl]);
          }
        }
      }

      var bCreateIndex = false;
      if (createIndex != null && createIndex != Null.Value && createIndex != Undefined.Value && createIndex is Boolean)
        bCreateIndex = (bool)createIndex;

      SPDirectory directory = new SPDirectory(targetFolder);
      var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
      IndexWriter writer = new IndexWriter(directory, analyzer, bCreateIndex, IndexWriter.MaxFieldLength.UNLIMITED);

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

      return new IndexWriterInstance(this.Engine.Object.InstancePrototype, IndexWriters[fullFolderUrl]);
    }

    [JSFunction(Name = "addObjectToIndex")]
    public void AddObjectToIndex(object folder, object obj, object createIndex)
    {
      var indexWriter = GetIndexWriter(folder, createIndex);

      var objString = JSONObject.Stringify(this.Engine, obj, null, null);
      var objectToAdd = JObject.Parse(objString);

      var doc = LuceneHelper.ConvertObjectToDocument(objectToAdd);

      try
      {
        indexWriter.IndexWriter.AddDocument(doc);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(folder);
      }
    }

    [JSFunction(Name = "removeFromIndex")]
    public void RemoveFromIndex(object folder, object query)
    {
      if (query == Null.Value || query == Undefined.Value || query == null)
        throw new JavaScriptException(this.Engine, "Error", "A query parameter must be specified to indicate the documents to remove from the index. Use '*' to specify all documents.");

      Query lQuery;

      var queryString = query.ToString();
      if (queryString == "*")
        lQuery = new MatchAllDocsQuery();
      else
      {
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "contents", new SimpleAnalyzer());
        lQuery = parser.Parse(query.ToString());
      }
      
      var indexWriter = GetIndexWriter(folder, false);

      try
      {
        indexWriter.IndexWriter.DeleteDocuments(lQuery);
      }
      catch (OutOfMemoryException)
      {
        CloseIndexWriter(folder);
      }
    }

    [JSFunction(Name = "closeIndexWriter")]
    public void CloseIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);
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

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object folder, string query)
    {
      IndexSearcher searcher;
      if (HasIndexWriter(folder))
      {
        var indexWriter = GetIndexWriter(folder, false);
        var reader = indexWriter.IndexWriter.GetReader();
        searcher = new IndexSearcher(reader);
      }
      else
      {
        var targetFolder = GetFolderFromObject(folder);
        var directory = new SPDirectory(targetFolder);
        searcher = new IndexSearcher(directory);
      }

      try
      {
        var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "contents", new SimpleAnalyzer());
        var lQuery = parser.Parse(query);

        var hits = searcher.Search(lQuery, 100);

        //iterate over the results.
        var searchResults = hits.ScoreDocs.AsQueryable()
                                 .OrderByDescending(hit => hit.Score)
                                 .Select(hit => new Hit
                                 {
                                   Score = hit.Score,
                                   Document = searcher.Doc(hit.Doc)
                                 })
                                 .ToList();

        var hitInstances = searchResults
        .Select(hit => new SearchHitInstance(this.Engine.Object.InstancePrototype, hit))
        .ToArray();

        return this.Engine.Array.Construct(hitInstances);
      }
      finally
      {
        searcher.Dispose();
      }
    }

    private SPFolder GetFolderFromObject(object folder)
    {
      if (folder == Null.Value || folder == Undefined.Value || folder == null)
        throw new JavaScriptException(this.Engine, "Error", "A folder must be specified.");

      SPFolder targetFolder;
      if (folder is SPFolderInstance)
      {
        var folderInstance = folder as SPFolderInstance;
        targetFolder = folderInstance.Folder;
      }
      else if (folder is string)
      {
        SPSite site;
        SPWeb web;

        var url = folder as string;
        if (SPHelper.TryGetSPFolder(url, out site, out web, out targetFolder) == false)
          throw new JavaScriptException(this.Engine, "Error", "A folder is not available at the specified url.");
      }
      else
        throw new JavaScriptException(this.Engine, "Error",
                                      "Cannot create a folder with the specified object: " +
                                      TypeConverter.ToString(folder));

      return targetFolder;
    }
  }
}
