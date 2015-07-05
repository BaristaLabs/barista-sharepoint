using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using global::Raven.Json.Linq;

  [Serializable]
  public class JsonDocumentMetadataConstructor : ClrFunction
  {
    public JsonDocumentMetadataConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "JsonDocumentMetadata", new JsonDocumentMetadataInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public JsonDocumentMetadataInstance Construct()
    {
      return new JsonDocumentMetadataInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class JsonDocumentMetadataInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Data.JsonDocumentMetadata m_jsonDocumentMetadata;

    public JsonDocumentMetadataInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public JsonDocumentMetadataInstance(ObjectInstance prototype, RavenDB.Abstractions.Data.JsonDocumentMetadata jsonDocumentMetadata)
      : this(prototype)
    {
      if (jsonDocumentMetadata == null)
        throw new ArgumentNullException("jsonDocumentMetadata");

      m_jsonDocumentMetadata = jsonDocumentMetadata;
    }

    public RavenDB.Abstractions.Data.JsonDocumentMetadata JsonDocumentMetadata
    {
      get { return m_jsonDocumentMetadata; }
    }

    //[JSProperty(Name = "eTag")]
    //public object ETag
    //{
    //  get
    //  {
    //    if (m_jsonDocumentMetadata.Etag)
    //      return new GuidInstance(this.Engine.Object, m_jsonDocumentMetadata.Etag);
    //    return Null.Value;
    //  }
    //  set
    //  {
    //    Guid tmpGuid;
    //    if (value == null || value == Null.Value || value == Undefined.Value)
    //      m_jsonDocumentMetadata.Etag = null;
    //    else if (value is GuidInstance)
    //      m_jsonDocumentMetadata.Etag = (value as GuidInstance).Value;
    //    else if (Guid.TryParse(TypeConverter.ToString(value), out tmpGuid))
    //      m_jsonDocumentMetadata.Etag = tmpGuid;
    //    else
    //      m_jsonDocumentMetadata.Etag = null;
    //  }
    //}

    [JSProperty(Name = "key")]
    public string Key
    {
      get { return m_jsonDocumentMetadata.Key; }
      set { m_jsonDocumentMetadata.Key = value; }
    }

    [JSProperty(Name = "lastModified")]
    public object LastModified
    {
      get
      {
        if (m_jsonDocumentMetadata.LastModified.HasValue)
          return JurassicHelper.ToDateInstance(this.Engine, m_jsonDocumentMetadata.LastModified.Value);
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocumentMetadata.LastModified = null;
        else if (value is DateInstance)
          m_jsonDocumentMetadata.LastModified = DateTime.Parse((value as DateInstance).ToIsoString());
        else
          m_jsonDocumentMetadata.LastModified = null;
      }
    }

    [JSProperty(Name = "metaData")]
    public object Metadata
    {
      get
      {
        return m_jsonDocumentMetadata.Metadata == null
                 ? Null.Value
                 : JSONObject.Parse(this.Engine, m_jsonDocumentMetadata.Metadata.ToString(), null);
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocumentMetadata.Metadata = null;

        m_jsonDocumentMetadata.Metadata = RavenJObject.Parse(JSONObject.Stringify(this.Engine, value, null, null));
      }
    }

    [JSProperty(Name = "nonAuthoritativeInformation")]
    public object NonAuthoritativeInformation
    {
      get
      {
        if (m_jsonDocumentMetadata.NonAuthoritativeInformation.HasValue)
          return m_jsonDocumentMetadata.NonAuthoritativeInformation.Value;
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocumentMetadata.NonAuthoritativeInformation = null;
        else
          m_jsonDocumentMetadata.NonAuthoritativeInformation = TypeConverter.ToBoolean(value);
      }
    }
  }
}
