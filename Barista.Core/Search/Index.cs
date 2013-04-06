namespace Barista.Search
{
  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using Barista.Logging;
  using Barista.Search.Analyzers;
  using Lucene.Net.Analysis;
  using Lucene.Net.Analysis.Standard;
  using Lucene.Net.Documents;
  using Lucene.Net.Index;
  using Lucene.Net.Search;
  using Directory = Lucene.Net.Store.Directory;
  using Version = Lucene.Net.Util.Version;
  using Barista.Newtonsoft.Json.Linq;

  /// <summary>
  /// This is a thread safe, single instance for a particular index.
  /// </summary>
  public abstract class Index : IDisposable
  {
    #region Fields

    protected static readonly ILog LogIndexing = LogManager.GetLogger(typeof (Index).FullName + ".Indexing");
    protected static readonly ILog LogQuerying = LogManager.GetLogger(typeof (Index).FullName + ".Querying");
    private static readonly StopAnalyzer StopAnalyzer = new StopAnalyzer(Version.LUCENE_30);
    protected readonly IndexDefinition IndexDefinition;

    internal readonly string Name;
    private readonly List<AbstractAnalyzerGenerator> m_analyzerGenerators;
    private readonly IndexSearcherHolder m_currentIndexSearcherHolder = new IndexSearcherHolder();
    private readonly List<Document> m_currentlyIndexedDocuments = new List<Document>();
    private readonly Directory m_directory;

    private readonly ConcurrentQueue<IndexingPerformanceStats> m_indexingPerformanceStats =
      new ConcurrentQueue<IndexingPerformanceStats>();

    private readonly object m_writeLock = new object();
    private volatile bool m_disposed;
    private IndexWriter m_indexWriter;
    private SnapshotDeletionPolicy m_snapshotter;
    private volatile string m_waitReason;

    #endregion

    #region Constructor

    protected Index(Directory directory, string name, IndexDefinition indexDefinition)
    {
      if (directory == null)
        throw new ArgumentNullException("directory");

      if (name == null)
        throw new ArgumentNullException("name");

      if (indexDefinition == null)
        throw new ArgumentNullException("indexDefinition");

      m_analyzerGenerators = new List<AbstractAnalyzerGenerator>
        {
          new BaristaAnalyzerGenerator()
        };
      Name = name;
      this.m_directory = directory;
      this.IndexDefinition = indexDefinition;

      RecreateSearcher();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the last time a query was performed on the index.
    /// </summary>
    public DateTime? LastQueryTime
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the last time a document was indexed.
    /// </summary>
    public DateTime LastIndexTime
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the duration of the last index operation
    /// </summary>
    public TimeSpan LastIndexingDuration
    {
      get;
      protected set;
    }

    /// <summary>
    /// Gets the average duration of indexing documents.
    /// </summary>
    public long TimePerDoc
    {
      get;
      protected set;
    }

    #endregion

    #region IDisposable
    public void Dispose()
    {
      try
      {
        // this is here so we can give good logs in the case of a long shutdown process
        if (Monitor.TryEnter(m_writeLock, 100) == false)
        {
          var localReason = m_waitReason;
          if (localReason != null)
            LogIndexing.Warn(
              "Waiting for {0} to complete before disposing of index {1}, that might take a while if the server is very busy",
              localReason, Name);

          Monitor.Enter(m_writeLock);
        }

        m_disposed = true;
        if (m_currentIndexSearcherHolder != null)
        {
          var item = m_currentIndexSearcherHolder.SetIndexSearcher(null, true);
          if (item.WaitOne(TimeSpan.FromSeconds(5)) == false)
          {
            LogIndexing.Warn(
              "After closing the index searching, we waited for 5 seconds for the searching to be done, but it wasn't. Continuing with normal shutdown anyway.");
          }
        }

        if (m_indexWriter != null)
        {
          var writer = m_indexWriter;
          m_indexWriter = null;

          try
          {
            writer.Analyzer.Close();
          }
          catch (Exception e)
          {
            LogIndexing.ErrorException("Error while closing the index (closing the analyzer failed)", e);
          }

          try
          {
            writer.Dispose();
          }
          catch (Exception e)
          {
            LogIndexing.ErrorException("Error when closing the index", e);
          }
        }

        try
        {
          m_directory.Dispose();
        }
        catch (Exception e)
        {
          LogIndexing.ErrorException("Error when closing the directory", e);
        }
      }
      finally
      {
        Monitor.Exit(m_writeLock);
      }
    }
    #endregion

    #region Protected Methods
    protected void AddindexingPerformanceStat(IndexingPerformanceStats stats)
    {
      m_indexingPerformanceStats.Enqueue(stats);
      if (m_indexingPerformanceStats.Count > 25)
        m_indexingPerformanceStats.TryDequeue(out stats);
    }

    protected virtual IndexQueryResult RetrieveDocument(Document document, FieldsToFetch fieldsToFetch, ScoreDoc score)
    {
      return new IndexQueryResult
      {
        Score = score.Score,
        Key = document.Get(Constants.DocumentIdFieldName),
        Projection = fieldsToFetch.IsProjection ? CreateDocumentFromFields(document, fieldsToFetch) : null
      };
    }

    protected void Write(Func<IndexWriter, Analyzer, int> action)
    {
      if (m_disposed)
        throw new ObjectDisposedException("Index " + Name + " has been disposed");

      LastIndexTime = DateTime.UtcNow;
      lock (m_writeLock)
      {
        bool shouldRecreateSearcher;
        var toDispose = new List<Action>();
        Analyzer searchAnalyzer = null;
        try
        {
          m_waitReason = "Write";
          searchAnalyzer = CreateAnalyzer(new LowerCaseKeywordAnalyzer(), toDispose, false);

          if (m_indexWriter == null)
          {
            CreateIndexWriter();
          }

          var locker = m_directory.MakeLock("writing-to-index.lock");
          try
          {
            var changedDocs = action(m_indexWriter, searchAnalyzer);
            shouldRecreateSearcher = changedDocs > 0;

            if (changedDocs > 0)
            {
              Flush(); // just make sure changes are flushed to disk
            }
          }
          finally
          {
            locker.Release();
          }
        }
        finally
        {
          m_currentlyIndexedDocuments.Clear();
          if (searchAnalyzer != null)
            searchAnalyzer.Close();
          foreach (var dispose in toDispose)
          {
            dispose();
          }
          m_waitReason = null;
          LastIndexTime = DateTime.UtcNow;
        }
        if (shouldRecreateSearcher)
          RecreateSearcher();
      }
    }

    protected void LogIndexedDocument(string key, Document luceneDoc)
    {
      if (!LogIndexing.IsDebugEnabled)
        return;

      var fieldsForLogging = luceneDoc.GetFields().Select(x => new
        {
          x.Name,
          Value = x.IsBinary ? "<binary>" : x.StringValue,
          Indexed = x.IsIndexed,
          Stored = x.IsStored,
        });
      var sb = new StringBuilder();
      foreach (var fieldForLogging in fieldsForLogging)
      {
        sb.Append("\t").Append(fieldForLogging.Name)
          .Append(" ")
          .Append(fieldForLogging.Indexed ? "I" : "-")
          .Append(fieldForLogging.Stored ? "S" : "-")
          .Append(": ")
          .Append(fieldForLogging.Value)
          .AppendLine();
      }

      LogIndexing.Debug("Indexing on {0} result in index {1} gave document: {2}", key, Name,
                        sb.ToString());
    }

    protected void AddDocumentToIndex(IndexWriter currentIndexWriter, Document luceneDoc, Analyzer analyzer)
    {
      var newAnalyzer = m_analyzerGenerators.Aggregate(analyzer,
                                                       (currentAnalyzer, generator) =>
                                                       {
                                                         var generateAnalyzer =
                                                           generator.GenerateAnalyzerForIndexing(Name, luceneDoc,
                                                                                                 currentAnalyzer);
                                                         if (generateAnalyzer != currentAnalyzer &&
                                                             currentAnalyzer != analyzer)
                                                           currentAnalyzer.Close();
                                                         return generateAnalyzer;
                                                       });

      try
      {
        currentIndexWriter.AddDocument(luceneDoc, newAnalyzer);
      }
      finally
      {
        if (newAnalyzer != analyzer)
          newAnalyzer.Close();
      }
    }
    #endregion

    public void Flush()
    {
      lock (m_writeLock)
      {
        if (m_disposed)
          return;
        if (m_indexWriter == null)
          return;

        try
        {
          m_waitReason = "Flush";
          m_indexWriter.Commit();
        }
        finally
        {
          m_waitReason = null;
        }
      }
    }

    public virtual void DeleteAll()
    {
      lock (m_writeLock)
      {
        if (m_disposed)
          return;

        if (m_indexWriter == null)
          return;

        try
        {
          m_waitReason = "Delete All";
          m_indexWriter.DeleteAll();
          m_indexWriter.Commit();

          RecreateSearcher();
        }
        finally
        {
          m_waitReason = null;
        }
      }
    }

    public void MarkQueried()
    {
      LastQueryTime = DateTime.UtcNow;
    }

    public void MarkQueried(DateTime time)
    {
      LastQueryTime = time;
    }

    public void MergeSegments()
    {
      lock (m_writeLock)
      {
        m_waitReason = "Merge / Optimize";
        try
        {
          LogIndexing.Info("Starting merge of {0}", Name);
          var sp = Stopwatch.StartNew();
          m_indexWriter.Optimize();
          LogIndexing.Info("Done merging {0} - took {1}", Name, sp.Elapsed);
        }
        finally
        {
          m_waitReason = null;
        }
      }
    }

    public abstract void IndexDocuments(IndexingBatch batch);

    public static JObject CreateDocumentFromFields(Document document, FieldsToFetch fieldsToFetch)
    {
      var documentFromFields = new JObject();
      var fields = fieldsToFetch.Fields;
      if (fieldsToFetch.FetchAllStoredFields)
        fields = fields.Concat(document.GetFields().Select(x => x.Name));

      var q = fields
        .Distinct()
        .SelectMany(name => document.GetFields(name) ?? new Field[0])
        .Where(x => x != null)
        .Where(
          x =>
          x.Name.EndsWith("_IsArray") == false &&
          x.Name.EndsWith("_Range") == false &&
          x.Name.EndsWith("_ConvertToJson") == false)
        .Select(fld => CreateProperty(fld, document))
        .GroupBy(x => x.Key)
        .Select(g =>
          {
            if (g.Count() == 1 && document.GetField(g.Key + "_IsArray") == null)
            {
              return g.First();
            }
            var jTokens = g.Select(x => x.Value).ToArray();
            return new KeyValuePair<string, JToken>(g.Key, new JArray((IEnumerable) jTokens));
          });
      foreach (var keyValuePair in q)
      {
        documentFromFields.Add(keyValuePair.Key, keyValuePair.Value);
      }
      return documentFromFields;
    }

    public BaristaPerFieldAnalyzerWrapper CreateAnalyzer(Analyzer defaultAnalyzer, ICollection<Action> toDispose,
                                                         bool forQuerying)
    {
      toDispose.Add(defaultAnalyzer.Close);

      string value;
      if (IndexDefinition.Analyzers.TryGetValue(Constants.AllFields, out value))
      {
        defaultAnalyzer = IndexingExtensions.CreateAnalyzerInstance(Constants.AllFields, value);
        toDispose.Add(defaultAnalyzer.Close);
      }
      var perFieldAnalyzerWrapper = new BaristaPerFieldAnalyzerWrapper(defaultAnalyzer);
      foreach (var analyzer in IndexDefinition.Analyzers)
      {
        var analyzerInstance = IndexingExtensions.CreateAnalyzerInstance(analyzer.Key, analyzer.Value);
        toDispose.Add(analyzerInstance.Close);

        if (forQuerying)
        {
          var customAttributes = analyzerInstance.GetType().GetCustomAttributes(typeof(NotForQueryingAttribute), false);
          if (customAttributes.Length > 0)
            continue;
        }

        perFieldAnalyzerWrapper.AddAnalyzer(analyzer.Key, analyzerInstance);
      }
      StandardAnalyzer standardAnalyzer = null;
      KeywordAnalyzer keywordAnalyzer = null;
      foreach (var fieldIndexing in IndexDefinition.Indexes)
      {
        switch (fieldIndexing.Value)
        {
          case FieldIndexing.NotAnalyzed:
            if (keywordAnalyzer == null)
            {
              keywordAnalyzer = new KeywordAnalyzer();
              toDispose.Add(keywordAnalyzer.Close);
            }
            perFieldAnalyzerWrapper.AddAnalyzer(fieldIndexing.Key, keywordAnalyzer);
            break;
          case FieldIndexing.Analyzed:
            if (IndexDefinition.Analyzers.ContainsKey(fieldIndexing.Key))
              continue;
            if (standardAnalyzer == null)
            {
              standardAnalyzer = new StandardAnalyzer(Version.LUCENE_29);
              toDispose.Add(standardAnalyzer.Close);
            }
            perFieldAnalyzerWrapper.AddAnalyzer(fieldIndexing.Key, standardAnalyzer);
            break;
        }
      }
      return perFieldAnalyzerWrapper;
    }

    public IndexingPerformanceStats[] GetIndexingPerformance()
    {
      return m_indexingPerformanceStats.ToArray();
    }

    public abstract void Remove(string[] keys);

    private static KeyValuePair<string, JToken> CreateProperty(IFieldable fld, Document document)
    {
      if (fld.IsBinary)
        return new KeyValuePair<string, JToken>(fld.Name, fld.GetBinaryValue());
      var stringValue = fld.StringValue;
      if (document.GetField(fld.Name + "_ConvertToJson") != null)
      {
        var val = JToken.Parse(fld.StringValue) as JObject;
        return new KeyValuePair<string, JToken>(fld.Name, val);
      }
      if (stringValue == Constants.NullValue)
        stringValue = null;
      if (stringValue == Constants.EmptyString)
        stringValue = string.Empty;
      return new KeyValuePair<string, JToken>(fld.Name, stringValue);
    }

    private void CreateIndexWriter()
    {
      m_snapshotter = new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy());
      m_indexWriter = new IndexWriter(m_directory, StopAnalyzer, m_snapshotter, IndexWriter.MaxFieldLength.UNLIMITED);
    }

    internal IDisposable GetSearcher(out IndexSearcher searcher)
    {
      return m_currentIndexSearcherHolder.GetSearcher(out searcher);
    }

    internal IDisposable GetSearcherAndTermsDocs(out IndexSearcher searcher, out JObject[] termsDocs)
    {
      return m_currentIndexSearcherHolder.GetSearcherAndTermDocs(out searcher, out termsDocs);
    }

    private void RecreateSearcher()
    {
      if (m_indexWriter == null)
      {
        m_currentIndexSearcherHolder.SetIndexSearcher(new IndexSearcher(m_directory, true), false);
      }
      else
      {
        var indexReader = m_indexWriter.GetReader();
        m_currentIndexSearcherHolder.SetIndexSearcher(new IndexSearcher(indexReader), false);
      }
    }

    protected static Document CloneDocument(Document luceneDoc)
    {
      var clonedDocument = new Document();
      foreach (AbstractField field in luceneDoc.GetFields())
      {
        var numericField = field as NumericField;
        if (numericField != null)
        {
          var clonedNumericField = new NumericField(numericField.Name,
                                                    numericField.IsStored ? Field.Store.YES : Field.Store.NO,
                                                    numericField.IsIndexed);
          var numericValue = numericField.NumericValue;
          if (numericValue is int)
          {
            clonedNumericField.SetIntValue((int) numericValue);
          }
          else if (numericValue is long)
          {
            clonedNumericField.SetLongValue((long) numericValue);
          }
          else if (numericValue is double)
          {
            clonedNumericField.SetDoubleValue((double) numericValue);
          }
          else if (numericValue is float)
          {
            clonedNumericField.SetFloatValue((float) numericValue);
          }
          clonedDocument.Add(clonedNumericField);
        }
        else
        {
          Field clonedField;
          if (field.IsBinary)
          {
            clonedField = new Field(field.Name, field.GetBinaryValue(),
                                    field.IsStored ? Field.Store.YES : Field.Store.NO);
          }
          else if (field.StringValue != null)
          {
            clonedField = new Field(field.Name, field.StringValue,
                                    field.IsStored ? Field.Store.YES : Field.Store.NO,
                                    field.IsIndexed ? Field.Index.ANALYZED_NO_NORMS : Field.Index.NOT_ANALYZED_NO_NORMS,
                                    field.IsTermVectorStored ? Field.TermVector.YES : Field.TermVector.NO);
          }
          else
          {
            //probably token stream, and we can't handle fields with token streams, so we skip this.
            continue;
          }
          clonedDocument.Add(clonedField);
        }
      }
      return clonedDocument;
    }
  }
}