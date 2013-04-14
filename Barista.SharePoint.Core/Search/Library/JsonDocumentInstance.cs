namespace Barista.SharePoint.Search.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.SharePoint.SPBaristaSearchService;
  using System;

  [Serializable]
  public class JsonDocumentConstructor : ClrFunction
  {
    public JsonDocumentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "JsonDocument", new JsonDocumentInstance(engine.Object.InstancePrototype))
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
    private readonly JsonDocument m_jsonDocument;

    public JsonDocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public JsonDocumentInstance(ObjectInstance prototype, JsonDocument jsonDocument)
      : this(prototype)
    {
      if (jsonDocument == null)
        throw new ArgumentNullException("jsonDocument");

      m_jsonDocument = jsonDocument;
    }

    public JsonDocument JsonDocument
    {
      get { return m_jsonDocument; }
    }

    [JSProperty(Name = "documentId")]
    public string DocumentId
    {
      get { return m_jsonDocument.DocumentId; }
      set { m_jsonDocument.DocumentId = value; }
    }

    [JSProperty(Name = "metadata")]
    public object Metadata
    {
      get { return JSONObject.Parse(this.Engine, m_jsonDocument.MetadataAsJson, null); }
      set { m_jsonDocument.MetadataAsJson = JSONObject.Stringify(this.Engine, value, null, null); }
    }

    [JSProperty(Name = "data")]
    public object Data
    {
      get { return JSONObject.Parse(this.Engine, m_jsonDocument.DataAsJson, null); }
      set { m_jsonDocument.DataAsJson = JSONObject.Stringify(this.Engine, value, null, null); }
    }
  }
}
