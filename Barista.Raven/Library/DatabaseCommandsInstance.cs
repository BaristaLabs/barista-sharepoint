using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using System.IO;
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using System.Text;

  [Serializable]
  public class DatabaseCommandsConstructor : ClrFunction
  {
    public DatabaseCommandsConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "DatabaseCommands", new DatabaseCommandsInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public DatabaseCommandsInstance Construct()
    {
      return new DatabaseCommandsInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class DatabaseCommandsInstance : ObjectInstance
  {
    private readonly RavenDB.Client.Connection.IDatabaseCommands m_databaseCommands;

    public DatabaseCommandsInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public DatabaseCommandsInstance(ObjectInstance prototype, RavenDB.Client.Connection.IDatabaseCommands databaseCommands)
      : this(prototype)
    {
      if (databaseCommands == null)
        throw new ArgumentNullException("databaseCommands");

      m_databaseCommands = databaseCommands;
    }

    public RavenDB.Client.Connection.IDatabaseCommands DatabaseCommands
    {
      get { return m_databaseCommands; }
    }

    [JSFunction(Name = "delete")]
    [JSDoc("Deletes the document with the specified key.")]
    public void Delete(object key, object eTag)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag); 
      m_databaseCommands.Delete(strKey, guidETag);
    }

    [JSFunction(Name = "deleteAttachment")]
    [JSDoc("Deletes the attachment with the specified key.")]
    public void DeleteAttachment(object key, object eTag)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag);
      m_databaseCommands.DeleteAttachment(strKey, guidETag);
    }

    [JSFunction(Name = "deleteByIndex")]
    [JSDoc("Performs a set-based delete using the specified index.")]
    public void DeleteByIndex(string indexName, IndexQueryInstance queryToDelete, object allowStale)
    {
      if (queryToDelete == null)
        throw new JavaScriptException(this.Engine, "Error", "A delete query must be specified as the second argument.");

      if (allowStale != null && allowStale != Null.Value && allowStale != Undefined.Value)
        m_databaseCommands.DeleteByIndex(indexName, queryToDelete.IndexQuery, TypeConverter.ToBoolean(allowStale));
      else
        m_databaseCommands.DeleteByIndex(indexName, queryToDelete.IndexQuery);
    }

    [JSFunction(Name = "deleteIndex")]
    [JSDoc("Deletes the specified index.")]
    public void DeleteIndex(string name)
    {
      m_databaseCommands.DeleteIndex(name);
    }

    [JSFunction(Name = "forDatabase")]
    [JSDoc("Returns a new databasecommands object that will interact with the specified database.")]
    public DatabaseCommandsInstance ForDatabase(string databaseName)
    {
      return new DatabaseCommandsInstance(this.Engine.Object.InstancePrototype, m_databaseCommands.ForDatabase(databaseName));
    }

    [JSFunction(Name = "forSystemDatabase")]
    [JSDoc("Returns a new databasecommands object that will interact with the system database.")]
    public DatabaseCommandsInstance ForSystemDatabase()
    {
      return new DatabaseCommandsInstance(this.Engine.Object.InstancePrototype, m_databaseCommands.ForSystemDatabase());
    }

    [JSFunction(Name = "get")]
    [JSDoc("Retrieves the document with the specified key.")]
    public JsonDocumentInstance Get(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var doc = m_databaseCommands.Get(strKey);
      return new JsonDocumentInstance(this.Engine.Object.InstancePrototype, doc);
    }

    [JSFunction(Name = "getAttachment")]
    [JSDoc("Retrieves the attachment with the specified key.")]
    public AttachmentInstance GetAttachment(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var attachment = m_databaseCommands.GetAttachment(strKey);
      return new AttachmentInstance(this.Engine.Object.InstancePrototype, attachment);
    }

    [JSFunction(Name = "getAttachmentHeadersStartingWith")]
    [JSDoc("Returns attachments starting with the specified prefix.")]
    public ArrayInstance GetAttachmentHeadersStartingWith(string idPrefix, int start, int pageSize)
    {
      var results = m_databaseCommands.GetAttachmentHeadersStartingWith(idPrefix, start, pageSize);

      return
        this.Engine.Array.Construct(
          // ReSharper disable CoVariantArrayConversion
          results.Select(a => new AttachmentInstance(this.Engine.Object.InstancePrototype, a)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "getDatabaseNames")]
    [JSDoc("Retrieves the names of all tenant databases on the RavenDB server.")]
    public ArrayInstance GetDatabaseNames(int pageSize, object start)
    {
      string[] databaseNames;
      if (start != null && start != Null.Value && start != Undefined.Value)
      {
        var intStart = TypeConverter.ToInteger(start);
        databaseNames = m_databaseCommands.GetDatabaseNames(pageSize, intStart);
      }
      else
      {
        databaseNames = m_databaseCommands.GetDatabaseNames(pageSize);
      }

// ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(databaseNames);
// ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "getIndex")]
    [JSDoc("Retrieves the index with the specified key.")]
    public IndexDefinitionInstance GetIndex(object name)
    {
      if (name == null || name == Undefined.Value || name == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain the name of the index.");

      var strName = TypeConverter.ToString(name);

      var index = m_databaseCommands.GetIndex(strName);
      return new IndexDefinitionInstance(this.Engine.Object.InstancePrototype, index);
    }

    [JSFunction(Name = "getIndexNames")]
    [JSDoc("Retrieves the names of indexes that exist on the RavenDB server.")]
    public ArrayInstance GetIndexNames(int start, int pageSize)
    {
      var indexNames = m_databaseCommands.GetIndexNames(start, pageSize);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(indexNames);
      // ReSharper restore CoVariantArrayConversion
    }

    //TODO: Get Statistics

    [JSFunction(Name = "getTerms")]
    [JSDoc("Retrieves all terms stored in the index for the specified field using the fromValue as the starting point and the page size as the number of results to return.")]
    public ArrayInstance GetTerms(string index, string field, string fromValue, int pageSize)
    {
      var terms = m_databaseCommands.GetTerms(index, field, fromValue, pageSize);

      // ReSharper disable CoVariantArrayConversion
      return this.Engine.Array.Construct(terms.ToArray());
      // ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "head")]
    [JSDoc("Returns the document metadata for the specified document key")]
    public object Head(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var head = m_databaseCommands.Head(strKey);
      return head == null
        ? null
        : new JsonDocumentMetadataInstance(this.Engine.Object.InstancePrototype, head);
    }

    [JSFunction(Name = "headAttachment")]
    [JSDoc("Returns the attachment metadata for the specified key.")]
    public object HeadAttachment(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var attachment = m_databaseCommands.HeadAttachment(strKey);
      return new AttachmentInstance(this.Engine.Object.InstancePrototype, attachment);
    }

    [JSFunction(Name = "nextIdentityFor")]
    [JSDoc("Generate the next identity value from the server.")]
    public long NextIdentityFor(string name)
    {
      return m_databaseCommands.NextIdentityFor(name);
    }

    //TODO: Patch

    [JSFunction(Name = "put")]
    [JSDoc("Puts the document in the database with the specified key.")]
    public PutResultInstance Put(object key, object eTag, object document, object metadata)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      if (document == null || document == Undefined.Value || document == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The third argument must contain a document.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag);

      var roDocument = RavenDB.Json.Linq.RavenJObject.Parse(JSONObject.Stringify(this.Engine, document, null, null));

      RavenDB.Json.Linq.RavenJObject roMetadata = null;
      if (metadata != null && metadata != Null.Value && metadata != Undefined.Value)
        roMetadata = RavenDB.Json.Linq.RavenJObject.Parse(JSONObject.Stringify(this.Engine, metadata, null, null));

      var result = m_databaseCommands.Put(strKey, guidETag, roDocument, roMetadata);
      return new PutResultInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "putAttachment")]
    [JSDoc("Puts a byte array as an attachment with the specified key.")]
    public void PutAttachment(object key, object eTag, object attachment, object metadata)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      if (attachment == null || attachment == Undefined.Value || attachment == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The third argument must contain an attachment.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag);

      byte[] attachmentData;
      if (attachment is Base64EncodedByteArrayInstance)
      {
        attachmentData = (attachment as Base64EncodedByteArrayInstance).Data;
      }
      else
      {
        attachmentData = Encoding.UTF8.GetBytes(TypeConverter.ToString(attachment));
      }

      RavenDB.Json.Linq.RavenJObject roMetadata = null;
      if (metadata != null && metadata != Null.Value && metadata != Undefined.Value)
        roMetadata = RavenDB.Json.Linq.RavenJObject.Parse(JSONObject.Stringify(this.Engine, metadata, null, null));


      using(var dataStream = new MemoryStream(attachmentData))
      {
        m_databaseCommands.PutAttachment(strKey, guidETag, dataStream, roMetadata);
      }
    }

    [JSFunction(Name = "putIndex")]
    [JSDoc("Creates an index with the specified name, based on an index definition.")]
    public string PutIndex(object name, object indexDefinition, object overwrite)
    {
      if (name == null || name == Undefined.Value || name == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain the index name.");

      if (indexDefinition == null || indexDefinition == Undefined.Value || indexDefinition == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The second argument must contain the index definition.");

      var strName = TypeConverter.ToString(name);
      RavenDB.Abstractions.Indexing.IndexDefinition objIndexDefinition;

      if (indexDefinition is IndexDefinitionInstance)
        objIndexDefinition = (indexDefinition as IndexDefinitionInstance).IndexDefinition;
      else if (indexDefinition is ObjectInstance)
        objIndexDefinition = JurassicHelper.Coerce<IndexDefinitionInstance>(this.Engine, indexDefinition).IndexDefinition;
      else
        throw new JavaScriptException(this.Engine, "Error", "The second argument must contain either an instance of IndexDefinition or a object that can be converted");

      string result;
      if (overwrite != null && overwrite != Null.Value && overwrite != Undefined.Value)
        result = m_databaseCommands.PutIndex(strName, objIndexDefinition, TypeConverter.ToBoolean(overwrite));
      else
        result = m_databaseCommands.PutIndex(strName, objIndexDefinition);

      return result;
    }

    [JSFunction(Name = "query")]
    [JSDoc("Queries the specified index in the Raven flavored Lucene query syntax.")]
    public QueryResultInstance Query(string index, IndexQueryInstance indexQuery, ArrayInstance includes, object metadataOnly, object indexEntriesOnly)
    {
      if (indexQuery == null)
        throw new JavaScriptException(this.Engine, "Error", "The index query must be specified as the second argument.");

      var arrIncludes = includes.ElementValues.Select(TypeConverter.ToString).ToArray();

      var bMetadataOnly = false;
      if (metadataOnly != null && metadataOnly != Null.Value && metadataOnly != Undefined.Value)
        bMetadataOnly = TypeConverter.ToBoolean(metadataOnly);

      var bIndexEntriesOnly = false;
      if (indexEntriesOnly != null && indexEntriesOnly != Null.Value && indexEntriesOnly != Undefined.Value)
        bIndexEntriesOnly = TypeConverter.ToBoolean(indexEntriesOnly);

      var result = m_databaseCommands.Query(index, indexQuery.IndexQuery, arrIncludes, bMetadataOnly, bIndexEntriesOnly);
      return new QueryResultInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSFunction(Name = "resetIndex")]
    [JSDoc("Resets the specified index.")]
    public void ResetIndex(string name)
    {
      m_databaseCommands.ResetIndex(name);
    }

    [JSFunction(Name = "startsWith")]
    [JSDoc("Retrieves documents for the specified key prefix.")]
    public ArrayInstance StartsWith(string keyPrefix, string matches, int start, int pageSize, object metadataOnly)
    {
      var bMetadataOnly = false;

      if (metadataOnly == null || metadataOnly == Null.Value || metadataOnly == Undefined.Value)
        bMetadataOnly = TypeConverter.ToBoolean(metadataOnly);

      var results = m_databaseCommands.StartsWith(keyPrefix, matches, start, pageSize, bMetadataOnly);
      return
        this.Engine.Array.Construct(
        // ReSharper disable CoVariantArrayConversion
          results.Select(r => new JsonDocumentInstance(this.Engine.Object.InstancePrototype, r)).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "urlFor")]
    [JSDoc("Returns the full url for the specified document.")]
    public string UrlFor(object documentKey)
    {
      if (documentKey == null || documentKey == Undefined.Value || documentKey == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(documentKey);

      return m_databaseCommands.UrlFor(strKey);
    }

    private static Guid? GetETagValue(object eTag)
    {
      Guid tmpGuid;
      Guid? guidETag = null;
      if (eTag is GuidInstance)
        guidETag = (eTag as GuidInstance).Value;
      else if (eTag != null && eTag != Null.Value && eTag != Undefined.Value &&
               Guid.TryParse(TypeConverter.ToString(eTag), out tmpGuid))
        guidETag = tmpGuid;

      return guidETag;
    }
  }
}
