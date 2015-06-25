namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPAuditEntryCollectionConstructor : ClrFunction
    {
        public SPAuditEntryCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAuditEntryCollection", new SPAuditEntryCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAuditEntryCollectionInstance Construct()
        {
            return new SPAuditEntryCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAuditEntryCollectionInstance : ObjectInstance
    {
        private readonly SPAuditEntryCollection m_auditEntryCollection;

        public SPAuditEntryCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAuditEntryCollectionInstance(ObjectInstance prototype, SPAuditEntryCollection auditEntryCollection)
            : this(prototype)
        {
            if (auditEntryCollection == null)
                throw new ArgumentNullException("auditEntryCollection");

            m_auditEntryCollection = auditEntryCollection;
        }

        public SPAuditEntryCollection SPAuditEntryCollection
        {
            get { return m_auditEntryCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_auditEntryCollection.Count;
            }
        }

        [JSFunction(Name = "getEntryByIndex")]
        public SPAuditEntryInstance GetEntryByIndex(int index)
        {
            var result = m_auditEntryCollection[index];
            if (result == null)
                return null;

            return new SPAuditEntryInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (SPAuditEntry entry in this.m_auditEntryCollection)
                ArrayInstance.Push(result, new SPAuditEntryInstance(this.Engine.Object.InstancePrototype, entry));
            return result;
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_auditEntryCollection.ToString();
        }
    }
}
