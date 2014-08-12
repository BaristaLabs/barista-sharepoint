namespace Barista.Bundles
{
    using Barista.Library;
    using Jurassic;
    using System;

    [Serializable]
    public class JsonDataBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "Json Data"; }
        }

        public string BundleDescription
        {
            get
            {
                return "Json Data Bundle. Provides behavior to assist with manipulating json. Currently adds:\n" +
                       "jsonDataHandler: a mechanism to Diff/Merge Json objects.\n" +
                       "automapper: a mechanism to perform json object-to-object mapping.\n" +
                       "JsonSchema: a top-level native JsonSchema validator.\n" +
                       "JsonMergeSettings: Supports object.merge().\n" + 
                       "object.merge(): a native mechanism to merge json objects.\n" +
                       "object.selectToken(): a native mechnism for XPath like quering.\n" +
                       "object.selectTokens(): a native mechnism for XPath like quering.\n" +
                       "object.isValid(schema): validates that the object is valid according to the instance of the JsonSchema.\n" +
                        "object.isValid2(schema): validates that the object is valid according to the instance of the JsonSchema. Returns a collection of errors\n";
            }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.Execute(Barista.Properties.Resources.jsonDataHandler);
            engine.Execute(Barista.Properties.Resources.Automapper);

            engine.SetGlobalValue("JsonMergeSettings", new JsonMergeSettingsConstructor(engine));
            engine.SetGlobalValue("JsonSchema", new JsonSchemaConstructor(engine));
            engine.Object.InstancePrototype.SetPropertyValue("merge", new JsonData.MergeFunctionInstance(engine, engine.Object.InstancePrototype), false);
            engine.Object.InstancePrototype.SetPropertyValue("selectToken", new JsonData.SelectTokenFunctionInstance(engine, engine.Object.InstancePrototype), false);
            engine.Object.InstancePrototype.SetPropertyValue("selectTokens", new JsonData.SelectTokensFunctionInstance(engine, engine.Object.InstancePrototype), false);
            engine.Object.InstancePrototype.SetPropertyValue("isValid", new JsonData.IsValidFunctionInstance(engine, engine.Object.InstancePrototype), false);
            engine.Object.InstancePrototype.SetPropertyValue("isValid2", new JsonData.IsValid2FunctionInstance(engine, engine.Object.InstancePrototype), false);
            return Null.Value;
        }
    }
}
