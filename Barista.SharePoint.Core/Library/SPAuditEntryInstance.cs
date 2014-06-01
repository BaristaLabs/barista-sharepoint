namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPAuditEntryConstructor : ClrFunction
    {
        public SPAuditEntryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAuditEntry", new SPAuditEntryInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAuditEntryInstance Construct()
        {
            return new SPAuditEntryInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAuditEntryInstance : ObjectInstance
    {
        private readonly SPAuditEntry m_auditEntry;

        public SPAuditEntryInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAuditEntryInstance(ObjectInstance prototype, SPAuditEntry auditEntry)
            : this(prototype)
        {
            if (auditEntry == null)
                throw new ArgumentNullException("auditEntry");

            m_auditEntry = auditEntry;
        }

        public SPAuditEntry SPAuditEntry
        {
            get { return m_auditEntry; }
        }

        [JSProperty(Name = "docLocation")]
        public string DocLocation
        {
            get
            {
                return m_auditEntry.DocLocation;
            }
        }

        [JSProperty(Name = "event")]
        public string Event
        {
            get
            {
                return m_auditEntry.Event.ToString();
            }
        }

        [JSProperty(Name = "eventData")]
        public string EventData
        {
            get
            {
                return m_auditEntry.EventData;
            }
        }

        [JSProperty(Name = "eventName")]
        public string EventName
        {
            get
            {
                return m_auditEntry.EventName;
            }
        }

        [JSProperty(Name = "eventSource")]
        public string EventSource
        {
            get
            {
                return m_auditEntry.EventSource.ToString();
            }
        }

        [JSProperty(Name = "itemId")]
        public GuidInstance ItemId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_auditEntry.ItemId);
            }
        }

        [JSProperty(Name = "itemType")]
        public string ItemType
        {
            get
            {
                return m_auditEntry.ItemType.ToString();
            }
        }

        [JSProperty(Name = "locationType")]
        public string LocationType
        {
            get
            {
                return m_auditEntry.LocationType.ToString();
            }
        }

        [JSProperty(Name = "machineIp")]
        public string MachineIp
        {
            get
            {
                return m_auditEntry.MachineIP;
            }
        }

        [JSProperty(Name = "machineName")]
        public string MachineName
        {
            get
            {
                return m_auditEntry.MachineName;
            }
        }

        [JSProperty(Name = "occurred")]
        public DateInstance Occurred
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_auditEntry.Occurred);
            }
        }

        [JSProperty(Name = "siteId")]
        public GuidInstance SiteId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_auditEntry.SiteId);
            }
        }

        [JSProperty(Name = "sourceName")]
        public string SourceName
        {
            get
            {
                return m_auditEntry.SourceName;
            }
        }

        [JSProperty(Name = "userId")]
        public int UserId
        {
            get
            {
                return m_auditEntry.UserId;
            }
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_auditEntry.ToString();
        }
    }
}
