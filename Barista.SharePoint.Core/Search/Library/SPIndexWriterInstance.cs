namespace Barista.SharePoint.Search.Library
{
  using Barista.Extensions;
  using Barista.Search.Library;
  using Barista.SharePoint.Search;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Lucene.Net.Index;
  using Lucene.Net.Search;
  using Microsoft.SharePoint;
  using Barista.Newtonsoft.Json.Linq;
  using System;
  using System.Reflection;

  [Serializable]
  public class IndexWriterConstructor : ClrFunction
  {
    public IndexWriterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexWriter", new SPIndexWriterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPIndexWriterInstance Construct()
    {
      return new SPIndexWriterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPIndexWriterInstance : ObjectInstance
  {
    private readonly IndexWriter m_indexWriter;
    private readonly SPFolder m_targetFolder;

    public SPIndexWriterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }
    
    public SPIndexWriterInstance(ObjectInstance prototype, IndexWriter indexWriter, SPFolder targetFolder)
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
      var queryType = query.GetType();

      if (queryString == "*")
        lQuery = new MatchAllDocsQuery();
      else if (queryType.IsSubclassOfRawGeneric(typeof(QueryInstance<>)))
      {
        var queryProperty = queryType.GetProperty("Query", BindingFlags.Instance | BindingFlags.Public);
        lQuery = queryProperty.GetValue(query, null) as Query;
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "The query parameter was not a query.");


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
