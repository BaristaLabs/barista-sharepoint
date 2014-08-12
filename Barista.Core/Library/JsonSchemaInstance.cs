namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.Newtonsoft.Json.Schema;

    [Serializable]
    public class JsonSchemaConstructor : ClrFunction
    {
        public JsonSchemaConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "JsonSchema", new JsonSchemaInstance(engine))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public JsonSchemaInstance Construct()
        {
            return new JsonSchemaInstance(this.Engine, new JsonSchema());
        }

        [JSFunction(Name = "parse")]
        public JsonSchemaInstance Parse(object schema)
        {
            if (schema == null || schema == Undefined.Value || schema == Null.Value)
                throw new JavaScriptException(this.Engine, "Error", "Schema must be specified as the first object.");

            var schemaString = TypeUtilities.IsString(schema)
                ? TypeConverter.ToString(schema)
                : JSONObject.Stringify(this.Engine, schema, null, null);

            return new JsonSchemaInstance(this.Engine, JsonSchema.Parse(schemaString));
        }
    }

    [Serializable]
    public class JsonSchemaInstance : ObjectInstance
    {
        private readonly JsonSchema m_jsonSchema;

        internal JsonSchemaInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public JsonSchemaInstance(ScriptEngine engine, JsonSchema jsonSchema)
            : this(engine)
        {
            if (jsonSchema == null)
                throw new JavaScriptException(engine, "Error", "$camelCasedName must be specified.");

            m_jsonSchema = jsonSchema;
        }

        protected JsonSchemaInstance(ObjectInstance prototype, JsonSchema jsonSchema)
            : base(prototype)
        {
            if (jsonSchema == null)
                throw new ArgumentNullException("jsonSchema");

            m_jsonSchema = jsonSchema;
        }

        public JsonSchema JsonSchema
        {
            get
            {
                return m_jsonSchema;
            }
        }

        [JSProperty(Name = "default")]
        public object Default
        {
            get
            {
                if (m_jsonSchema.Default == null)
                    return null;

                var result = m_jsonSchema.Default.ToString();
                return JSONObject.Parse(this.Engine, result, null);
            }
            set
            {
                if (value == Undefined.Value || value == Null.Value || value == null)
                    m_jsonSchema.Default = null;

                m_jsonSchema.Default = JToken.Parse(JSONObject.Stringify(this.Engine, value, null, null));
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_jsonSchema.Description;
            }
            set
            {
                m_jsonSchema.Description = value;
            }
        }

        [JSProperty(Name = "format")]
        public string Format
        {
            get
            {
                return m_jsonSchema.Format;
            }
            set
            {
                m_jsonSchema.Format = value;
            }
        }

        [JSProperty(Name = "id")]
        public string Id
        {
            get
            {
                return m_jsonSchema.Id;
            }
            set
            {
                m_jsonSchema.Id = value;
            }
        }

        [JSProperty(Name = "location")]
        public string Location
        {
            get
            {
                return m_jsonSchema.Location;
            }
            set
            {
                m_jsonSchema.Location = value;
            }
        }

        [JSProperty(Name = "pattern")]
        public string Pattern
        {
            get
            {
                return m_jsonSchema.Pattern;
            }
            set
            {
                m_jsonSchema.Pattern = value;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_jsonSchema.Title;
            }
            set
            {
                m_jsonSchema.Title = value;
            }
        }

        [JSFunction(Name = "toJSON")]
        public object ToJson()
        {
            return JToken.Parse(m_jsonSchema.ToString());
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_jsonSchema.ToString();
        }
    }
}
