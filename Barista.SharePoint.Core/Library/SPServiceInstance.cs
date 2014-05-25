namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint.Administration;
    using System;

    [Serializable]
    public class SPServiceConstructor : ClrFunction
    {
        public SPServiceConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPService", new SPServiceInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPServiceInstance Construct()
        {
            return new SPServiceInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPServiceInstance : ObjectInstance
    {
        private readonly SPService m_service;

        public SPServiceInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPServiceInstance(ObjectInstance prototype, SPService service)
            : this(prototype)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            m_service = service;
        }

        public SPService SPService
        {
            get { return m_service; }
        }

        [JSProperty(Name = "applications")]
        public SPServiceApplicationCollectionInstance Applications
        {
            get
            {
                return new SPServiceApplicationCollectionInstance(this.Engine.Object.InstancePrototype, m_service.Applications);
            }
        }

        [JSProperty(Name = "instances")]
        public SPServiceInstanceDependencyCollectionInstance Instances
        {
            get
            {
                return new SPServiceInstanceDependencyCollectionInstance(this.Engine.Object.InstancePrototype, m_service.Instances);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_service.Name; }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return m_service.DisplayName; }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get { return new GuidInstance(this.Engine.Object, m_service.Id); }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get { return m_service.Status.ToString(); }
        }

        [JSProperty(Name = "typeName")]
        public string TypeName
        {
            get { return m_service.TypeName; }
        }
    }
}
