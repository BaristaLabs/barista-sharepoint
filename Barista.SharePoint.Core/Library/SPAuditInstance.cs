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
        public object AuditFlags
        {
            get
            {
                try
                {
                    return m_audit.AuditFlags.ToString();
                }
                catch(Exception)
                {
                    return Undefined.Value;
                }
            }
            set
            {
                var str = TypeConverter.ToString(value);

                SPAuditMaskType mask;
                if (str.TryParseEnum(true, out mask))
                    m_audit.AuditFlags = mask;
            }
        }

        [JSProperty(Name = "effectiveAuditMask")]
        public object EffectiveAuditMask
        {
            get
            {
                try
                {
                    return m_audit.EffectiveAuditMask.ToString();
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
        }

        [JSProperty(Name = "useAuditFlagCache")]
        public object UseAuditFlagCache
        {
            get
            {
                try
                {
                    return m_audit.UseAuditFlagCache;
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
            set
            {
                var b = TypeConverter.ToBoolean(value);
                m_audit.UseAuditFlagCache = b;
            }
        }

        [JSFunction(Name = "getEntries")]
        public SPAuditEntryCollectionInstance GetEntries(object query)
        {
            SPAuditEntryCollection result;

            if (query is SPAuditQueryInstance)
                result = m_audit.GetEntries((query as SPAuditQueryInstance).SPAuditQuery);
            else
                result = m_audit.GetEntries();

            return result == null
                ? null
                : new SPAuditEntryCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

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

        [JSFunction(Name = "writeAuditEvent")]
        public bool WriteAuditEvent(string eventName, string eventSource, object arg3, object arg4)
        {
            if (arg4 == Undefined.Value)
            {
                return m_audit.WriteAuditEvent(eventName, eventSource, TypeConverter.ToString(arg3));
            }
            return m_audit.WriteAuditEvent(eventName, eventSource, TypeConverter.ToInteger(arg3), TypeConverter.ToString(arg4));
        }

        [JSFunction(Name = "writeAuditEvent2")]
        public bool WriteAuditEvent2(string eventType, string eventSource, string xmlData)
        {
            SPAuditEventType auditEventType;

            if (!eventType.TryParseEnum(true, out auditEventType))
                throw new JavaScriptException(this.Engine, "Error", "EventType parameter must be of the possible SPAuditEventTypes");

            return m_audit.WriteAuditEvent(auditEventType, eventSource, xmlData);
        }


        [JSFunction(Name = "writeAuditEventUnlimitedData")]
        public bool WriteAuditEventUnlimitedData(string eventName, string eventSource, string xmlData)
        {
            return m_audit.WriteAuditEventUnlimitedData(eventName, eventSource, xmlData);
        }

        [JSFunction(Name = "writeAuditEventUnlimitedData2")]
        public bool WriteAuditEventUnlimitedData2(string eventType, string eventSource, string xmlData)
        {
            SPAuditEventType auditEventType;

            if (!eventType.TryParseEnum(true, out auditEventType))
                throw new JavaScriptException(this.Engine, "Error", "EventType parameter must be of the possible SPAuditEventTypes");

            return m_audit.WriteAuditEventUnlimitedData(auditEventType, eventSource, xmlData);
        }
    }
}
