namespace Barista.Search.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json.Linq;

    [Serializable]
    public class JsonDocumentConstructor : ClrFunction
    {
        public JsonDocumentConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "JsonDocument", new JsonDocumentInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public JsonDocumentInstance Construct()
        {
            return new JsonDocumentInstance(this.Engine, new JsonDocumentDto());
        }
    }

    [Serializable]
    public sealed class JsonDocumentInstance : ObjectInstance
    {
        private readonly JsonDocumentDto m_jsonDocument;
        private object m_jsMetadataObject;
        private object m_jsObject;

        public JsonDocumentInstance(ScriptEngine engine, JsonDocumentDto jsonDocument)
            : base(engine)
        {
            m_jsonDocument = jsonDocument;
            this.PopulateFunctions();
        }

        public JsonDocumentDto JsonDocument
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
            get
            {
                return m_jsMetadataObject ??
                       (m_jsMetadataObject = JSONObject.Parse(this.Engine, m_jsonDocument.MetadataAsJson, null));
            }
            set
            {
                m_jsMetadataObject = value;
                m_jsonDocument.MetadataAsJson = JSONObject.Stringify(this.Engine, value, null, null);
            }
        }

        [JSProperty(Name = "data")]
        public object Data
        {
            get
            {
                return m_jsObject ??
                    (m_jsObject = JSONObject.Parse(this.Engine, m_jsonDocument.DataAsJson, null));
            }
            set
            {
                m_jsObject = value;
                m_jsonDocument.DataAsJson = JSONObject.Stringify(this.Engine, value, null, null);
            }
        }

        /// <summary>
        /// Used by the JSON.stringify to transform objects prior to serialization.
        /// </summary>
        /// <param name="thisObject"></param>
        /// <param name="key"> Unused. </param>
        /// <returns> The date as a serializable string. </returns>
        [JSFunction(Name = "toJSON")]
        public object ToJson(ObjectInstance thisObject, string key)
        {
            var obj = new JObject
            {
                {"documentId", m_jsonDocument.DocumentId},
                {"metadata", JObject.Parse(m_jsonDocument.MetadataAsJson)},
                {"data", JObject.Parse(m_jsonDocument.DataAsJson)}
            };

            return obj;
        }
    }
}
