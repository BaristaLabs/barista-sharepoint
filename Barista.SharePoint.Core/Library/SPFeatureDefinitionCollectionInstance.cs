namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.Administration;

    [Serializable]
    public class SPFeatureDefinitionCollectionConstructor : ClrFunction
    {
        public SPFeatureDefinitionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFeatureDefinitionCollection", new SPFeatureDefinitionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFeatureDefinitionCollectionInstance Construct()
        {
            return new SPFeatureDefinitionCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPFeatureDefinitionCollectionInstance : ObjectInstance
    {
        private readonly SPFeatureDefinitionCollection m_featureDefinitionCollection;

        public SPFeatureDefinitionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPFeatureDefinitionCollectionInstance(ObjectInstance prototype, SPFeatureDefinitionCollection featureDefinitionCollection)
            : this(prototype)
        {
            if (featureDefinitionCollection == null)
                throw new ArgumentNullException("featureDefinitionCollection");

            m_featureDefinitionCollection = featureDefinitionCollection;
        }

        public SPFeatureDefinitionCollection SPFeatureDefinitionCollection
        {
            get
            {
                return m_featureDefinitionCollection;
            }
        }

        [JSFunction(Name = "add")]
        public SPFeatureDefinitionInstance Add(string relativePathToFeatureManifest, object solutionId, object force)
        {
            var guidSolutionId = GuidInstance.ConvertFromJsObjectToGuid(solutionId);
            SPFeatureDefinition result;
            if (force == Undefined.Value)
                result = m_featureDefinitionCollection.Add(relativePathToFeatureManifest, guidSolutionId);
            else
                result = m_featureDefinitionCollection.Add(relativePathToFeatureManifest, guidSolutionId,
                    TypeConverter.ToBoolean(force));

            return result == null
                ? null
                : new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "add2")]
        public SPFeatureDefinitionInstance Add(SPFeatureDefinitionInstance featureDefinition)
        {
            if (featureDefinition == null)
                throw new JavaScriptException(this.Engine, "Error", "feature definition must be specified.");

            var def = m_featureDefinitionCollection.Add(featureDefinition.FeatureDefinition);
            return def == null
                ? null
                : new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, def);
        }

        [JSFunction(Name = "ensure")]
        public SPFeatureDefinitionInstance Ensure(SPFeatureDefinitionInstance featureDefinition)
        {
            if (featureDefinition == null)
                throw new JavaScriptException(this.Engine, "Error", "feature definition must be specified.");

            var def = m_featureDefinitionCollection.Ensure(featureDefinition.FeatureDefinition);
            return def == null
                ? null
                : new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, def);
        }

        [JSFunction(Name = "getByFeatureName")]
        public SPFeatureDefinitionInstance GetByFeatureName(string featureName)
        {
            var result = m_featureDefinitionCollection[featureName];
            return result == null
                ? null
                : new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getByFeatureId")]
        public SPFeatureDefinitionInstance GetByFeatureId(object featureId)
        {
            var guidFeatureId = GuidInstance.ConvertFromJsObjectToGuid(featureId);
            var result = m_featureDefinitionCollection[guidFeatureId];
            return result == null
                ? null
                : new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "removeById")]
        public void RemoveById(object id, object force)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            if (force == Undefined.Value)
                m_featureDefinitionCollection.Remove(guidId);
            else
                m_featureDefinitionCollection.Remove(guidId, TypeConverter.ToBoolean(force));
        }

        [JSFunction(Name = "removeByPath")]
        public void RemoveByPath(string pathToFeatureManifest, object force)
        {
            if (force == Undefined.Value)
                m_featureDefinitionCollection.Remove(pathToFeatureManifest);
            else
                m_featureDefinitionCollection.Remove(pathToFeatureManifest, TypeConverter.ToBoolean(force));
        }

        [JSFunction(Name = "scanForFeatures")]
        public ArrayInstance ScanForFeatures(object solutionId, bool scanOnly, bool force)
        {
            var guidSolutionId = GuidInstance.ConvertFromJsObjectToGuid(solutionId);
            var result = m_featureDefinitionCollection.ScanForFeatures(guidSolutionId, scanOnly, force);

            var arrResult = this.Engine.Array.Construct();
            foreach (var item in result.OfType<SPFeatureDefinition>())
                ArrayInstance.Push(arrResult,
                    new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, item));

            return arrResult;
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();

            //Mmmm, feet....
            foreach (var feat in m_featureDefinitionCollection.Select(def => new SPFeatureDefinitionInstance(this.Engine.Object.InstancePrototype, def)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, feat);
            }

            return result;
        }
    }
}
