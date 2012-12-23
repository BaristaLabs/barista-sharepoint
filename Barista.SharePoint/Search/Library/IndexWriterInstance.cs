using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Microsoft.SharePoint;
using Newtonsoft.Json.Linq;

namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Index;
  using System;

  [Serializable]
  public class IndexWriterConstructor : ClrFunction
  {
    public IndexWriterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexWriter", new IndexWriterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexWriterInstance Construct()
    {
      return new IndexWriterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexWriterInstance : ObjectInstance
  {
    private readonly IndexWriter m_indexWriter;
    private readonly SPFolder m_targetFolder;

    public IndexWriterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexWriterInstance(ObjectInstance prototype, IndexWriter indexWriter, SPFolder targetFolder)
      : this(prototype)
    {
      if (indexWriter == null)
        throw new ArgumentNullException("indexWriter");

      if (targetFolder == null)
        throw new ArgumentNullException("targetFolder");

      m_indexWriter = indexWriter;
      m_targetFolder = targetFolder;
    }

    public IndexWriter IndexWriter
    {
      get { return m_indexWriter; }
    }

    [JSFunction(Name = "addObjectToIndex")]
    public void AddObjectToIndex(object obj)
    {
      //TODO: if the obj is a SPListItemInstance, create a document from the list item.
      var objString = JSONObject.Stringify(this.Engine, obj, null, null);
      var objectToAdd = JObject.Parse(objString);

      var doc = LuceneHelper.ConvertObjectToDocument(objectToAdd);

      try
      {
        m_indexWriter.AddDocument(doc);
      }
      catch (OutOfMemoryException)
      {
        LuceneHelper.CloseIndexWriterSingleton(m_targetFolder);
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

      try
      {
        m_indexWriter.DeleteDocuments(lQuery);
      }
      catch (OutOfMemoryException)
      {
        LuceneHelper.CloseIndexWriterSingleton(m_targetFolder);
      }
    }
  }
}
