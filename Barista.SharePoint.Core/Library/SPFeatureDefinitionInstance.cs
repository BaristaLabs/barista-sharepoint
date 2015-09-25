namespace Barista.SharePoint.Library
{
    using System;
    using System.Linq;
    using Barista.Library;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    [Serializable]
    public class SPFeatureDefinitionConstructor : ClrFunction
    {
        public SPFeatureDefinitionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFeatureDefinition", new SPFeatureDefinitionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFeatureDefinitionInstance Construct(object featureId)
        {
            var featureGuid = GuidInstance.ConvertFromJsObjectToGuid(featureId);
            var featureDefinition = SPFarm.Local.FeatureDefinitions.FirstOrDefault(fd => fd.Id == featureGuid);

            if (featureDefinition == null)
                throw new JavaScriptException(this.Engine, "Error", "A feature with the specified id is not installed in the farm.");

            return new SPFeatureDefinitionInstance(this.InstancePrototype, featureDefinition);
        }

        public SPFeatureDefinitionInstance Construct(SPFeatureDefinition featureDefinition)
        {
            if (featureDefinition == null)
                throw new ArgumentNullException("featureDefinition");

            return new SPFeatureDefinitionInstance(this.InstancePrototype, featureDefinition);
        }
    }

    [Serializable]
    public class SPFeatureDefinitionInstance : ObjectInstance
    {
        private readonly SPFeatureDefinition m_featureDefinition;

        public SPFeatureDefinitionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPFeatureDefinitionInstance(ObjectInstance prototype, SPFeatureDefinition featureDefinition)
            : this(prototype)
        {
            this.m_featureDefinition = featureDefinition;
        }

        #region Properties
        internal SPFeatureDefinition FeatureDefinition
        {
            get { return m_featureDefinition; }
        }

        [JSProperty(Name = "activateOnDefault")]
        public bool ActivateOnDefault
        {
            get { return m_featureDefinition.ActivateOnDefault; }
        }

        [JSProperty(Name = "activationDependencies")]
        public ArrayInstance ActivationDependencies
        {
            get
            {
                var result = this.Engine.Array.Construct();
                foreach (var featureDependency in m_featureDefinition.ActivationDependencies.OfType<SPFeatureDependency>())
                {
                    ArrayInstance.Push(result, new SPFeatureDependencyInstance(this.Engine.Object.InstancePrototype, featureDependency));
                }

                return result;
            }
        }

        [JSProperty(Name = "alwaysForceInstall")]
        public bool AlwaysForceInstall
        {
            get { return m_featureDefinition.AlwaysForceInstall; }
        }

        [JSProperty(Name = "autoActivateInCentralAdmin")]
        public bool AutoActivateInCentralAdmin
        {
            get { return m_featureDefinition.AutoActivateInCentralAdmin; }
        }

        [JSProperty(Name = "compatibilityLevel")]
        public int CompatibilityLevel
        {
            get { return m_featureDefinition.CompatibilityLevel; }
        }

        [JSProperty(Name = "defaultResourceFile")]
        public string DefaultResourceFile
        {
            get { return m_featureDefinition.DefaultResourceFile; }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return m_featureDefinition.DisplayName; }
        }

        [JSProperty(Name = "hidden")]
        public bool Hidden
        {
            get { return m_featureDefinition.Hidden; }
        }

        [JSProperty(Name = "id")]
        public string Id
        {
            get { return m_featureDefinition.Id.ToString(); }
        }

        [JSProperty(Name = "imageUrl")]
        public string ImageUrl
        {
            get { return m_featureDefinition.GetImageUrl(System.Threading.Thread.CurrentThread.CurrentCulture); }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_featureDefinition.Name; }
        }

        //TODO: Properties;

        [JSProperty(Name = "receiverAssembly")]
        public string ReceiverAssembly
        {
            get { return m_featureDefinition.ReceiverAssembly; }
        }

        [JSProperty(Name = "receiverClass")]
        public string ReceiverClass
        {
            get { return m_featureDefinition.ReceiverClass; }
        }

        [JSProperty(Name = "requireResources")]
        public bool RequireResources
        {
            get { return m_featureDefinition.RequireResources; }
        }

        [JSProperty(Name = "rootDirectory")]
        public string RootDirectory
        {
            get { return m_featureDefinition.RootDirectory; }
        }

        [JSProperty(Name = "scope")]
        public string Scope
        {
            get { return m_featureDefinition.Scope.ToString(); }
        }

        [JSProperty(Name = "solutionId")]
        public string SolutionId
        {
            get { return m_featureDefinition.SolutionId.ToString(); }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get { return m_featureDefinition.Status.ToString(); }
        }

        [JSProperty(Name = "typeName")]
        public string TypeName
        {
            get { return m_featureDefinition.TypeName; }
        }

        [JSProperty(Name = "uiVersion")]
        public string UIVersion
        {
            get { return m_featureDefinition.UIVersion; }
        }

        [JSProperty(Name = "version")]
        public string Version
        {
            get { return m_featureDefinition.Version.ToString(); }
        }
        #endregion

        #region Functions
        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_featureDefinition.Delete();
        }

        [JSFunction(Name = "getTitle")]
        public string GetTitle()
        {
            return m_featureDefinition.GetTitle(System.Threading.Thread.CurrentThread.CurrentCulture);
        }

        [JSFunction(Name = "getXmlDefinition")]
        public string GetXmlDefinition()
        {
            var result = m_featureDefinition.GetXmlDefinition(System.Threading.Thread.CurrentThread.CurrentCulture);
            return result.ToString();
        }

        [JSFunction(Name = "provision")]
        public void Provision()
        {
            m_featureDefinition.Provision();
        }

        [JSFunction(Name = "uncache")]
        public void Uncache()
        {
            m_featureDefinition.Uncache();
        }

        [JSFunction(Name = "unprovision")]
        public void Unprovision()
        {
            m_featureDefinition.Unprovision();
        }

        [JSFunction(Name = "update")]
        public void Update(object ensure)
        {
            if (ensure == Null.Value || ensure == Undefined.Value || ensure == null)
                m_featureDefinition.Update();
            else
                m_featureDefinition.Update(TypeConverter.ToBoolean(ensure));
        }
        #endregion
    }
}
