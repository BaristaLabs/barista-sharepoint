namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.Newtonsoft.Json.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class JsonData
    {
        [Serializable]
        public sealed class MergeFunctionInstance : FunctionInstance
        {
            public MergeFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
                : base(engine, prototype)
            {
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var contentObject = argumentValues.ElementAtOrDefault(0);
                if (contentObject == Undefined.Value ||
                    contentObject == Null.Value || contentObject == null)
                    return thisObject;

                var mergeSettings = argumentValues.ElementAtOrDefault(1);
                JsonMergeSettingsInstance jms = null;
                if (mergeSettings != Undefined.Value && mergeSettings != Null.Value && mergeSettings != null)
                {
                    if (mergeSettings is JsonMergeSettingsInstance)
                        jms = (mergeSettings as JsonMergeSettingsInstance);
                    else
                        jms = JurassicHelper.Coerce<JsonMergeSettingsInstance>(this.Engine, mergeSettings);
                }
                
                var s1 = JSONObject.Stringify(this.Engine, thisObject, null, null);
                var s2 = JSONObject.Stringify(this.Engine, contentObject, null, null);

                if (TypeUtilities.IsObject(thisObject) == false && TypeUtilities.IsArray(thisObject) == false)
                    throw new JavaScriptException(this.Engine, "Error", "This object must either be an object or an array.");

                if (TypeUtilities.IsObject(contentObject) == false && TypeUtilities.IsArray(contentObject) == false)
                    throw new JavaScriptException(this.Engine, "Error", "Content object must either be an object or an array.");

                var o1 = (JContainer)JToken.Parse(s1);
                var o2 = (JContainer)JToken.Parse(s2);
                
                if (jms != null)
                    o1.Merge(o2, jms.JsonMergeSettings);
                else
                    o1.Merge(o2);

                var result = JSONObject.Parse(this.Engine, o1.ToString(), null);
                return result;
            }
        }

        [Serializable]
        public sealed class SelectTokenFunctionInstance : FunctionInstance
        {
            public SelectTokenFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
                : base(engine, prototype)
            {
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var path = argumentValues.ElementAtOrDefault(0);
                if (path == Undefined.Value ||
                    path == Null.Value || path == null)
                    return thisObject;

                var sPath = TypeConverter.ToString(path);

                var s1 = JSONObject.Stringify(this.Engine, thisObject, null, null);
                var o1 = JToken.Parse(s1);

                var token = o1.SelectToken(sPath, false);
                var result = JSONObject.Parse(this.Engine, token.ToString(), null);
                return result;
            }
        }

        [Serializable]
        public sealed class SelectTokensFunctionInstance : FunctionInstance
        {
            public SelectTokensFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
                : base(engine, prototype)
            {
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var path = argumentValues.ElementAtOrDefault(0);
                if (path == Undefined.Value ||
                    path == Null.Value || path == null)
                    return thisObject;

                var sPath = TypeConverter.ToString(path);

                var s1 = JSONObject.Stringify(this.Engine, thisObject, null, null);
                var o1 = JToken.Parse(s1);
                
                var tokens = o1.SelectTokens(sPath, false);
                var result = this.Engine.Array.Construct();
                foreach (var token in tokens)
                    ArrayInstance.Push(result, JSONObject.Parse(this.Engine, token.ToString(), null));

                return result;
            }
        }

        [Serializable]
        public sealed class IsValidFunctionInstance : FunctionInstance
        {
            public IsValidFunctionInstance(ScriptEngine engine, ObjectInstance prototype)
                : base(engine, prototype)
            {
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var schema = argumentValues.ElementAtOrDefault(0);
                if (schema == Undefined.Value ||
                    schema == Null.Value || schema == null)
                    return true;

                JsonSchema jsSchema;
                if (schema is JsonSchemaInstance)
                    jsSchema = (schema as JsonSchemaInstance).JsonSchema;
                else
                    jsSchema = JsonSchema.Parse(JSONObject.Stringify(this.Engine, schema, null, null));
                
                var s1 = JSONObject.Stringify(this.Engine, thisObject, null, null);
                var o1 = JToken.Parse(s1);

                return o1.IsValid(jsSchema);
            }
        }

        [Serializable]
        public sealed class IsValid2FunctionInstance : FunctionInstance
        {
            public IsValid2FunctionInstance(ScriptEngine engine, ObjectInstance prototype)
                : base(engine, prototype)
            {
            }

            public override object CallLateBound(object thisObject, params object[] argumentValues)
            {
                var schema = argumentValues.ElementAtOrDefault(0);
                if (schema == Undefined.Value ||
                    schema == Null.Value || schema == null)
                    return true;

                JsonSchema jsSchema;
                if (schema is JsonSchemaInstance)
                    jsSchema = (schema as JsonSchemaInstance).JsonSchema;
                else
                {
                    var schemaString = TypeUtilities.IsString(schema)
                        ? TypeConverter.ToString(schema)
                        : JSONObject.Stringify(this.Engine, schema, null, null);
                    jsSchema = JsonSchema.Parse(schemaString);
                }

                var s1 = JSONObject.Stringify(this.Engine, thisObject, null, null);
                var o1 = JToken.Parse(s1);

                IList<string> errors;
                var result = o1.IsValid(jsSchema, out errors);

                var arrErrors = this.Engine.Array.Construct();
                foreach (var error in errors)
                    ArrayInstance.Push(arrErrors, error);

                var objResult = this.Engine.Object.Construct();
                objResult.SetPropertyValue("isValid", result, false);
                objResult.SetPropertyValue("errors", arrErrors, false);
                return objResult;
            }
        }
    }
}
