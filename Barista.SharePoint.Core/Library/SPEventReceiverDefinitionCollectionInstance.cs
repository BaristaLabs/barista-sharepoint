namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;
    using System.Linq;
    using Undefined = Barista.Jurassic.Undefined;

    [Serializable]
    public class SPEventReceiverDefinitionCollectionConstructor : ClrFunction
    {
        public SPEventReceiverDefinitionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPEventReceiverDefinitionCollection", new SPEventReceiverDefinitionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPEventReceiverDefinitionCollectionInstance Construct()
        {
            return new SPEventReceiverDefinitionCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPEventReceiverDefinitionCollectionInstance : ObjectInstance
    {
        private readonly SPEventReceiverDefinitionCollection m_eventReceiverDefinitionCollection;

        public SPEventReceiverDefinitionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPEventReceiverDefinitionCollectionInstance(ObjectInstance prototype, SPEventReceiverDefinitionCollection eventReceiverDefinitionCollection)
            : this(prototype)
        {
            if (eventReceiverDefinitionCollection == null)
                throw new ArgumentNullException("eventReceiverDefinitionCollection");

            m_eventReceiverDefinitionCollection = eventReceiverDefinitionCollection;
        }

        public SPEventReceiverDefinitionCollection SPEventReceiverDefinitionCollection
        {
            get
            {
                return m_eventReceiverDefinitionCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_eventReceiverDefinitionCollection.Count;
            }
        }

        [JSProperty(Name = "hostId")]
        public GuidInstance HostId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinitionCollection.HostId);
            }
        }

        [JSProperty(Name = "hostType")]
        public string HostType
        {
            get
            {
                return m_eventReceiverDefinitionCollection.HostType.ToString();
            }
        }

        [JSProperty(Name = "itemId")]
        public int ItemId
        {
            get
            {
                return m_eventReceiverDefinitionCollection.ItemId;
            }
        }

        [JSFunction(Name = "add")]
        public SPEventReceiverDefinitionInstance Add(object id, object contextList)
        {
            SPEventReceiverDefinition result;
            if (id == Undefined.Value && contextList == Undefined.Value)
                result = m_eventReceiverDefinitionCollection.Add();
            else if (id != Undefined.Value && contextList == Undefined.Value)
            {
                var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
                result = m_eventReceiverDefinitionCollection.Add(guid);
            }
            else
            {
                var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

                var listContextList = contextList as SPListInstance;
                if (listContextList == null)
                    throw new JavaScriptException(this.Engine, "Error", "Context list must be specified and an instance of SPList.");

                result = m_eventReceiverDefinitionCollection.Add(guid, listContextList.List);
            }

            return result == null
                ? null
                : new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "add2")]
        public void Add2(string receiverType, string assembly, string className)
        {
            SPEventReceiverType erType;
            if (!receiverType.TryParseEnum(true, out erType))
                throw new JavaScriptException(this.Engine, "Error", "Receiver Type must be specified.");

            m_eventReceiverDefinitionCollection.Add(erType, assembly, className);
        }

        [JSFunction(Name = "doesEventReceiverDefinitionExist")]
        public bool EventReceiverDefinitionExist(object eventReceiverId)
        {
            var guidEventReceiverId = GuidInstance.ConvertFromJsObjectToGuid(eventReceiverId);

            return m_eventReceiverDefinitionCollection.EventReceiverDefinitionExist(guidEventReceiverId);
        }

        [JSFunction(Name = "getEventReceiverById")]
        public SPEventReceiverDefinitionInstance GetEventReceiverById(object eventReceiverId)
        {
            var guidEventReceiverId = GuidInstance.ConvertFromJsObjectToGuid(eventReceiverId);

            var result = m_eventReceiverDefinitionCollection[guidEventReceiverId];
            return result == null
                ? null
                : new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getEventReceiverByIndex")]
        public SPEventReceiverDefinitionInstance GetEventReceiverByIndex(int index)
        {
            var result = m_eventReceiverDefinitionCollection[index];
            return result == null
                ? null
                : new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getSite")]
        public SPSiteInstance GetSite()
        {
            return m_eventReceiverDefinitionCollection.Site == null
                ? null
                : new SPSiteInstance(this.Engine.Object.InstancePrototype, m_eventReceiverDefinitionCollection.Site);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            return m_eventReceiverDefinitionCollection.Web == null
                ? null
                : new SPWebInstance(this.Engine, m_eventReceiverDefinitionCollection.Web);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var iCal in m_eventReceiverDefinitionCollection.OfType<SPEventReceiverDefinition>().Select(def => new SPEventReceiverDefinitionInstance(this.Engine.Object.InstancePrototype, def)))
            {
                ArrayInstance.Push(result, iCal);
            }

            return result;
        }
    }
}
