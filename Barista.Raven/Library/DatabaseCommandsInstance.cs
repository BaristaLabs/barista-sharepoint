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
    public void Delete(object key, object eTag)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag); 
      m_databaseCommands.Delete(strKey, guidETag);
    }

    [JSFunction(Name = "deleteAttachment")]
    public void DeleteAttachment(object key, object eTag)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);
      var guidETag = GetETagValue(eTag);
      m_databaseCommands.DeleteAttachment(strKey, guidETag);
    }

    [JSFunction(Name = "deleteByIndex")]
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
    public void DeleteIndex(string name)
    {
      m_databaseCommands.DeleteIndex(name);
    }

    [JSFunction(Name = "get")]
    public JsonDocumentInstance Get(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var doc = m_databaseCommands.Get(strKey);
      return new JsonDocumentInstance(this.Engine.Object.InstancePrototype, doc);
    }

    [JSFunction(Name = "getAttachment")]
    public AttachmentInstance GetAttachment(object key)
    {
      if (key == null || key == Undefined.Value || key == Null.Value)
        throw new JavaScriptException(this.Engine, "Error", "The first argument must contain a key.");

      var strKey = TypeConverter.ToString(key);

      var attachment = m_databaseCommands.GetAttachment(strKey);
      return new AttachmentInstance(this.Engine.Object.InstancePrototype, attachment);
    }

    [JSFunction(Name = "put")]
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

    [JSFunction(Name = "query")]
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
