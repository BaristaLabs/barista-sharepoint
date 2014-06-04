namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;
    using System.Linq;

    [Serializable]
    public class SPAlertCollectionConstructor : ClrFunction
    {
        public SPAlertCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAlertCollection", new SPAlertCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAlertCollectionInstance Construct()
        {
            return new SPAlertCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAlertCollectionInstance : ObjectInstance
    {
        private readonly SPAlertCollection m_alertCollection;

        public SPAlertCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAlertCollectionInstance(ObjectInstance prototype, SPAlertCollection alertCollection)
            : this(prototype)
        {
            if (alertCollection == null)
                throw new ArgumentNullException("alertCollection");

            m_alertCollection = alertCollection;
        }

        public SPAlertCollection SPAlertCollection
        {
            get
            {
                return m_alertCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_alertCollection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public SPAlertInstance Add()
        {
            var result = m_alertCollection.Add();
            return result == null
                ? null
                : new SPAlertInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "deleteById")]
        public void DeleteById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            m_alertCollection.Delete(guidId);
        }

        [JSFunction(Name = "deleteByIndex")]
        public void DeleteByIndex(int index)
        {
            m_alertCollection.Delete(index);
        }

        [JSFunction(Name = "getAlertById")]
        public SPAlertInstance GetAlertById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_alertCollection[guidId];
            return result == null
                ? null
                : new SPAlertInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getAlertByIndex")]
        public SPAlertInstance GetAlertByIndex(int index)
        {
            var result = m_alertCollection[index];
            return result == null
                ? null
                : new SPAlertInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getSite")]
        public SPSiteInstance GetSite()
        {
            var result = m_alertCollection.Site;
            return result == null
                ? null
                : new SPSiteInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            var result = m_alertCollection.Web;
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var alert in m_alertCollection
                .OfType<SPAlert>()
                .Select(a => new SPAlertInstance(this.Engine.Object.InstancePrototype, a)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, alert);
            }

            return result;
        }
    }
}
