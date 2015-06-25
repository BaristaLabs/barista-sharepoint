namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPAttachmentCollectionConstructor : ClrFunction
    {
        public SPAttachmentCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPAttachmentCollection", new SPAttachmentCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPAttachmentCollectionInstance Construct()
        {
            return new SPAttachmentCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPAttachmentCollectionInstance : ObjectInstance
    {
        private readonly SPAttachmentCollection m_attachmentCollection;

        public SPAttachmentCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPAttachmentCollectionInstance(ObjectInstance prototype, SPAttachmentCollection attachmentCollection)
            : this(prototype)
        {
            if (attachmentCollection == null)
                throw new ArgumentNullException("attachmentCollection");

            m_attachmentCollection = attachmentCollection;
            
        }

        public SPAttachmentCollection SPAttachmentCollection
        {
            get { return m_attachmentCollection; }
        }

        [JSProperty(Name="count")]
        public int Count
        {
            get
            {
                return m_attachmentCollection.Count;
            }
        }

        [JSProperty(Name = "isSynchronized")]
        public bool IsSynchronized
        {
            get
            {
                return m_attachmentCollection.IsSynchronized;
            }
        }

        [JSProperty(Name = "urlPrefix")]
        public string UrlPrefix
        {
            get
            {
                return m_attachmentCollection.UrlPrefix;
            }
        }

        [JSFunction(Name = "add")]
        public void Add(string leafName, Base64EncodedByteArrayInstance data)
        {
            if (data == null)
                throw new JavaScriptException(this.Engine, "Error", "A Base64EncodedByteArrayInstance must be supplied as the attachment data to add.");

            m_attachmentCollection.Add(leafName, data.Data);
        }

        [JSFunction(Name = "addNow")]
        public void AddNow(string leafName, Base64EncodedByteArrayInstance data)
        {
            if (data == null)
                throw new JavaScriptException(this.Engine, "Error", "A Base64EncodedByteArrayInstance must be supplied as the attachment data to add.");

            m_attachmentCollection.AddNow(leafName, data.Data);
        }

        [JSFunction(Name = "getLeafItemNameAtIndex")]
        public string GetLeafItemNameAtIndex(int index)
        {
            return m_attachmentCollection[index];
        }

        [JSFunction(Name = "delete")]
        public void Delete(string leafName)
        {
            m_attachmentCollection.Delete(leafName);
        }

        [JSFunction(Name = "deleteNow")]
        public void DeleteNow(string leafName)
        {
            m_attachmentCollection.DeleteNow(leafName);
        }

        [JSFunction(Name = "recycle")]
        public void Recycle(string leafName)
        {
            m_attachmentCollection.Recycle(leafName);
        }

        [JSFunction(Name = "recycleNow")]
        public void RecycleNow(string leafName)
        {
            m_attachmentCollection.RecycleNow(leafName);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var attachment in m_attachmentCollection)
                ArrayInstance.Push(result, attachment);

            return result;
        }

    }
}
