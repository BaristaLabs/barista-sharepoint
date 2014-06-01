namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.SuperSocket.Common;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPAuditConstructor : ClrFunction
    {
        public SPAuditConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAudit", new SPAuditInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAuditInstance Construct()
        {
            return new SPAuditInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAuditInstance : ObjectInstance
    {
        private readonly SPAudit m_audit;

        public SPAuditInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAuditInstance(ObjectInstance prototype, SPAudit audit)
            : this(prototype)
        {
            if (audit == null)
                throw new ArgumentNullException("audit");

            m_audit = audit;
        }

        public SPAudit SPAudit
        {
            get { return m_audit; }
        }

        [JSProperty(Name = "auditFlags")]
        public string AuditFlags
        {
            get
            {
                return m_audit.AuditFlags.ToString();
            }
            set
            {
                SPAuditMaskType mask;
                if (value.TryParseEnum(true, out mask))
                    m_audit.AuditFlags = mask;
            }
        }

        [JSProperty(Name = "effectiveAuditMask")]
        public string EffectiveAuditMask
        {
            get
            {
                return m_audit.EffectiveAuditMask.ToString();
            }
        }

        [JSProperty(Name = "useAuditFlagCache")]
        public bool UseAuditFlagCache
        {
            get
            {
                return m_audit.UseAuditFlagCache;
            }
            set
            {
                m_audit.UseAuditFlagCache = value;
            }
        }

        //[JSFunction(Name = "getEntries")]
        //public SPAuditEntryCollectionInstance GetEntries(object query)
        //{
        //}

        [JSFunction(Name = "deleteEntries")]
        public int DeleteEntries(DateInstance deleteEndDate)
        {
            if (deleteEndDate == null)
                throw new JavaScriptException(this.Engine, "Error", "deleteEndDate must be specified.");
            return m_audit.DeleteEntries(deleteEndDate.Value);
        }

        [JSFunction(Name = "trimAuditLog")]
        public void TrimAuditLog(DateInstance deleteEndDate)
        {
            if (deleteEndDate == null)
                throw new JavaScriptException(this.Engine, "Error", "deleteEndDate must be specified.");
            m_audit.TrimAuditLog(deleteEndDate.Value);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_audit.Update();
        }

        //WriteAuditEvent
        //WriteAuditEventUnlimitedData
    }
}
