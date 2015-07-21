namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPAlertConstructor : ClrFunction
    {
        public SPAlertConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAlert", new SPAlertInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAlertInstance Construct()
        {
            return new SPAlertInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAlertInstance : ObjectInstance
    {
        private readonly SPAlert m_alert;

        public SPAlertInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAlertInstance(ObjectInstance prototype, SPAlert alert)
            : this(prototype)
        {
            if (alert == null)
                throw new ArgumentNullException("alert");

            m_alert = alert;
        }

        public SPAlert SPAlert
        {
            get
            {
                return m_alert;
            }
        }

        [JSProperty(Name = "alertFrequency")]
        public string AlertFrequency
        {
            get
            {
                return m_alert.AlertFrequency.ToString();
            }
            set
            {
                SPAlertFrequency freq;
                if (!value.TryParseEnum(true, out freq))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPAlertFrequency must be specified.");

                m_alert.AlertFrequency = freq;
            }
        }

        [JSProperty(Name = "alertTemplate")]
        public SPAlertTemplateInstance AlertTemplate
        {
            get
            {
                return m_alert.AlertTemplate == null
                    ? null
                    : new SPAlertTemplateInstance(this.Engine.Object.InstancePrototype, m_alert.AlertTemplate);
            }
        }

        [JSProperty(Name = "alertTemplateName")]
        public string AlertTemplateName
        {
            get
            {
                return m_alert.AlertTemplateName;
            }
        }

        [JSProperty(Name = "alertTime")]
        public DateInstance AlertTime
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_alert.AlertTime);
            }
            set
            {
                if (value != null)
                    m_alert.AlertTime = value.Value;
            }
        }

        [JSProperty(Name = "alertType")]
        public string AlertType
        {
            get
            {
                return m_alert.AlertType.ToString();
            }
            set
            {
                SPAlertType type;
                if (!value.TryParseEnum(true, out type))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPAlertType must be specified.");

                m_alert.AlertType = type;
            }
        }

        [JSProperty(Name = "alwaysNotify")]
        public bool AlwaysNotify
        {
            get
            {
                return m_alert.AlwaysNotify;
            }
            set
            {
                m_alert.AlwaysNotify = value;
            }
        }

        [JSProperty(Name = "deliveryChannels")]
        public string DeliveryChannels
        {
            get
            {
                return m_alert.DeliveryChannels.ToString();
            }
            set
            {
                SPAlertDeliveryChannels channels;
                if (!value.TryParseEnum(true, out channels))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPAlertType must be specified.");

                m_alert.DeliveryChannels = channels;
            }
        }

        [JSProperty(Name = "dynamicRecipient")]
        public string DynamicRecipient
        {
            get
            {
                return m_alert.DynamicRecipient;
            }
            set
            {
                m_alert.DynamicRecipient = value;
            }
        }

        [JSProperty(Name = "eventType")]
        public string EventType
        {
            get
            {
                return m_alert.EventType.ToString();
            }
            set
            {
                SPEventType type;
                if (!value.TryParseEnum(true, out type))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPEventType must be specified.");

                m_alert.EventType = type;
            }
        }

        [JSProperty(Name = "eventTypeBitmask")]
        public int EventTypeBitmask
        {
            get
            {
                return m_alert.EventTypeBitmask;
            }
            set
            {
                m_alert.EventTypeBitmask = value;
            }
        }

        [JSProperty(Name = "filter")]
        public string Filter
        {
            get
            {
                return m_alert.Filter;
            }
            set
            {
                m_alert.Filter = value;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_alert.ID);
            }
        }

        [JSProperty(Name = "itemId")]
        public int ItemId
        {
            get
            {
                return m_alert.ItemID;
            }
        }

        [JSProperty(Name = "listId")]
        public GuidInstance ListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_alert.ListID);
            }
        }

        [JSProperty(Name = "listUrl")]
        public string ListUrl
        {
            get
            {
                return m_alert.ListUrl;
            }
        }

        [JSProperty(Name = "matchId")]
        public GuidInstance MatchId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_alert.MatchId);
            }
            set
            {
                m_alert.MatchId = value == null
                    ? Guid.Empty
                    : value.Value;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public SPPropertyBagInstance PropertyBag
        {
            get
            {
                return m_alert.Properties == null
                    ? null
                    : new SPPropertyBagInstance(this.Engine.Object.InstancePrototype, m_alert.Properties);
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get
            {
                return m_alert.Status.ToString();
            }
            set
            {
                SPAlertStatus status;
                if (!value.TryParseEnum(true, out status))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPAlertStatus must be specified.");

                m_alert.Status = status;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_alert.Title;
            }
            set
            {
                m_alert.Title = value;
            }
        }

        [JSProperty(Name = "user")]
        public SPUserInstance User
        {
            get
            {
                return m_alert.User == null
                    ? null
                    : new SPUserInstance(Engine, m_alert.User);
            }
            set
            {
                m_alert.User = value == null
                    ? null
                    : value.User;
            }
        }

        [JSProperty(Name = "userId")]
        public int UserId
        {
            get
            {
                return m_alert.UserId;
            }
        }

        [JSFunction(Name = "getListItem")]
        public SPListItemInstance GetListItem()
        {
            return m_alert.Item == null
                ? null
                : new SPListItemInstance(this.Engine, m_alert.Item);
        }

        [JSFunction(Name = "getList")]
        public SPListInstance GetList()
        {
            return m_alert.List == null
                ? null
                : new SPListInstance(this.Engine, m_alert.List.ParentWeb.Site, m_alert.List.ParentWeb, m_alert.List);
        }

        [JSFunction(Name = "setListItem")]
        public void SetListItem(SPListItemInstance listItem)
        {
            m_alert.Item = listItem == null
                ? null
                : listItem.ListItem;
        }

        [JSFunction(Name = "setList")]
        public void SetList(SPListInstance list)
        {
            m_alert.List = list == null
                ? null
                : list.List;
        }

        [JSFunction(Name = "update")]
        public void Update(object bSendMail)
        {
            if (bSendMail == Undefined.Value)
                m_alert.Update();
            else
                m_alert.Update(TypeConverter.ToBoolean(bSendMail));
        }
    }
}
