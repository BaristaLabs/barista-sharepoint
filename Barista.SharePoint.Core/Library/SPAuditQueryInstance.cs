namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPAuditQueryConstructor : ClrFunction
    {
        public SPAuditQueryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAuditQuery", new SPAuditQueryInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAuditQueryInstance Construct(SPSiteInstance site)
        {
            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "A site must be specified.");

            return new SPAuditQueryInstance(this.InstancePrototype, new SPAuditQuery(site.Site));
        }
    }

    [Serializable]
    public class SPAuditQueryInstance : ObjectInstance
    {
        private readonly SPAuditQuery m_auditQuery;

        public SPAuditQueryInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAuditQueryInstance(ObjectInstance prototype, SPAuditQuery auditQuery)
            : this(prototype)
        {
            if (auditQuery == null)
                throw new ArgumentNullException("auditQuery");

            m_auditQuery = auditQuery;
        }

        public SPAuditQuery SPAuditQuery
        {
            get { return m_auditQuery; }
        }

        [JSProperty(Name = "hasMoreItems")]
        public object HasMoreItems
        {
            get
            {
                if (m_auditQuery.HasMoreItems.HasValue == false)
                    return null;

                return m_auditQuery.HasMoreItems.Value;
            }
        }

        [JSProperty(Name = "rowLimit")]
        public double RowLimit
        {
            get
            {
                return m_auditQuery.RowLimit;
            }
            set
            {
                m_auditQuery.RowLimit = (uint)value;
            }
        }

        [JSFunction(Name = "addEventRestriction")]
        public void AddEventRestriction(string eventId)
        {
            SPAuditEventType auditEventType;

            if (!eventId.TryParseEnum(true, out auditEventType))
                throw new JavaScriptException(this.Engine, "Error", "EventType parameter must be of the possible SPAuditEventTypes");

            m_auditQuery.AddEventRestriction(auditEventType);
        }

        [JSFunction(Name = "restrictToList")]
        public void RestrictToList(SPListInstance list)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "List must be specified.");

            m_auditQuery.RestrictToList(list.List);
        }

        [JSFunction(Name = "restrictToListItem")]
        public void RestrictToListItem(SPListItemInstance listItem)
        {
            if (listItem == null)
                throw new JavaScriptException(this.Engine, "Error", "ListItem must be specified.");

            m_auditQuery.RestrictToListItem(listItem.ListItem);
        }

        [JSFunction(Name = "restrictToUser")]
        public void RestrictToUser(int userId)
        {
            m_auditQuery.RestrictToUser(userId);
        }

        [JSFunction(Name = "setRangeEnd")]
        public void SetRangeEnd(DateInstance end)
        {
            if (end == null)
                throw new JavaScriptException(this.Engine, "Error", "End date must be specified.");

            m_auditQuery.SetRangeEnd(end.Value);
        }

        [JSFunction(Name = "setRangeStart")]
        public void SetRangeStart(DateInstance start)
        {
            if (start == null)
                throw new JavaScriptException(this.Engine, "Error", "Start date must be specified.");

            m_auditQuery.SetRangeStart(start.Value);
        }
    }
}
