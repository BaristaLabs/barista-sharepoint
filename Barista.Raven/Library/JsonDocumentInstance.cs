using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using global::Raven.Json.Linq;

  [Serializable]
  public class JsonDocumentConstructor : ClrFunction
  {
    public JsonDocumentConstructor(ScriptEngine engine)
      : base(
        engine.Function.InstancePrototype, "JsonDocument", new JsonDocumentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public JsonDocumentInstance Construct()
    {
      return new JsonDocumentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class JsonDocumentInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Data.JsonDocument m_jsonDocument;

    public JsonDocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public JsonDocumentInstance(ObjectInstance prototype, RavenDB.Abstractions.Data.JsonDocument jsonDocument)
      : this(prototype)
    {
      if (jsonDocument == null)
        throw new ArgumentNullException("jsonDocument");

      m_jsonDocument = jsonDocument;
    }

    public RavenDB.Abstractions.Data.JsonDocument JsonDocument
    {
      get { return m_jsonDocument; }
    }

    //[JSProperty(Name = "eTag")]
    //public object ETag
    //{
    //  get
    //  {
    //    if (m_jsonDocument.Etag.HasValue)
    //      return new GuidInstance(this.Engine.Object, m_jsonDocument.Etag.Value);
    //    return Null.Value;
    //  }
    //  set
    //  {
    //    Guid tmpGuid;
    //    if (value == null || value == Null.Value || value == Undefined.Value)
    //      m_jsonDocument.Etag = null;
    //    else if (value is GuidInstance)
    //      m_jsonDocument.Etag = (value as GuidInstance).Value;
    //    else if (Guid.TryParse(TypeConverter.ToString(value), out tmpGuid))
    //      m_jsonDocument.Etag = tmpGuid;
    //    else
    //      m_jsonDocument.Etag = null;
    //  }
    //}

    [JSProperty(Name = "key")]
    public string Key
    {
      get { return m_jsonDocument.Key; }
      set { m_jsonDocument.Key = value; }
    }

    [JSProperty(Name = "lastModified")]
    public object LastModified
    {
      get
      {
        if (m_jsonDocument.LastModified.HasValue)
          return JurassicHelper.ToDateInstance(this.Engine, m_jsonDocument.LastModified.Value);
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocument.LastModified = null;
        else if (value is DateInstance)
          m_jsonDocument.LastModified = DateTime.Parse((value as DateInstance).ToIsoString());
        else
          m_jsonDocument.LastModified = null;
      }
    }

    [JSProperty(Name = "metaData")]
    public object Metadata
    {
      get
      {
        return m_jsonDocument.Metadata == null
                 ? Null.Value
                 : JSONObject.Parse(this.Engine, m_jsonDocument.Metadata.ToString(), null);
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocument.Metadata = null;

        m_jsonDocument.Metadata = RavenJObject.Parse(JSONObject.Stringify(this.Engine, value, null, null));
      }
    }

    [JSProperty(Name = "data")]
    public object Data
    {
      get
      {
        return m_jsonDocument.DataAsJson == null
                 ? Null.Value
                 : JSONObject.Parse(this.Engine, m_jsonDocument.DataAsJson.ToString(), null);
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocument.DataAsJson = null;

        m_jsonDocument.DataAsJson = RavenJObject.Parse(JSONObject.Stringify(this.Engine, value, null, null));
      }
    }

    [JSProperty(Name = "nonAuthoritativeInformation")]
    public object NonAuthoritativeInformation
    {
      get
      {
        if (m_jsonDocument.NonAuthoritativeInformation.HasValue)
          return m_jsonDocument.NonAuthoritativeInformation.Value;
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocument.NonAuthoritativeInformation = null;
        else
          m_jsonDocument.NonAuthoritativeInformation = TypeConverter.ToBoolean(value);
      }
    }

    [JSProperty(Name = "tempIndexScore")]
    public object TempIndexScore
    {
      get
      {
        if (m_jsonDocument.TempIndexScore.HasValue)
          return (double) m_jsonDocument.TempIndexScore;
        return Null.Value;
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_jsonDocument.TempIndexScore = null;
        else
          m_jsonDocument.TempIndexScore = (float) TypeConverter.ToNumber(value);
      }
    }
  }
}
