namespace Barista.SharePoint.Library
{
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class SPServiceInstanceConstructor : ClrFunction
    {
        public SPServiceInstanceConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPServiceInstance", new SPServiceInstanceInstance(engine))
        {
        }

        [JSConstructorFunction]
        public SPServiceInstanceInstance Construct()
        {
            return new SPServiceInstanceInstance(this.Engine);
        }
    }

    [Serializable]
    public class SPServiceInstanceInstance : ObjectInstance
    {
        public SPServiceInstanceInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public SPServiceInstanceInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        public SPServiceInstanceInstance(ObjectInstance prototype, Microsoft.SharePoint.Administration.SPServiceInstance serviceInstance)
            : base(prototype)
        {
            if (serviceInstance == null)
                throw new ArgumentNullException("serviceInstance");

            SPServiceInstance = serviceInstance;
            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public Microsoft.SharePoint.Administration.SPServiceInstance SPServiceInstance
        {
            get;
            set;
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get { return SPServiceInstance.Description; }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return SPServiceInstance.DisplayName; }
        }

        [JSProperty(Name = "hidden")]
        public bool Hidden
        {
            get { return SPServiceInstance.Hidden; }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get { return new GuidInstance(this.Engine.Object.InstancePrototype, SPServiceInstance.Id); }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return SPServiceInstance.Name; }
        }

        [JSProperty(Name = "manageLinkUrl")]
        public string ManageLink
        {
            get
            {
                if (SPServiceInstance.ManageLink != null)
                    return SPServiceInstance.ManageLink.Url;
                return String.Empty;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public HashtableInstance PropertyBag
        {
            get
            {
                return SPServiceInstance.Properties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, SPServiceInstance.Properties);
            }
        }

        [JSProperty(Name = "provisionLinkUrl")]
        public string ProvisionLinkUrl
        {
            get
            {
                return SPServiceInstance.ProvisionLink != null
                    ? SPServiceInstance.ProvisionLink.Url
                    : string.Empty;
            }
        }

        [JSProperty(Name = "server")]
        public object Server
        {
            get
            {
                if (SPServiceInstance.Server == null)
                    return Null.Value;

                return new SPServerInstance(this.Engine.Object.InstancePrototype, SPServiceInstance.Server);
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get { return SPServiceInstance.Status.ToString(); }
        }

        [JSProperty(Name = "systemService")]
        public bool SystemService
        {
            get { return SPServiceInstance.SystemService; }
        }

        [JSProperty(Name = "typeName")]
        public string TypeName
        {
            get { return SPServiceInstance.TypeName; }
        }

        [JSProperty(Name = "unprovisionLinkUrl")]
        public string UnprovisionLinkUrl
        {
            get
            {
                return SPServiceInstance.UnprovisionLink != null
                    ? SPServiceInstance.UnprovisionLink.Url
                    : string.Empty;
            }
        }

        [JSProperty(Name = "version")]
        public double Version
        {
            get
            {
                return SPServiceInstance.Version;
            }
        }

        [JSFunction(Name = "getFarm")]
        public SPFarmInstance GetFarm()
        {
            return new SPFarmInstance(this.Engine.Object.InstancePrototype, SPServiceInstance.Farm);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            SPServiceInstance.Update();
        }
    }
}
