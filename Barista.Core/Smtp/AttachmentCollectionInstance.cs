namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class AttachmentCollectionConstructor : ClrFunction
    {
        public AttachmentCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "AttachmentCollection", new AttachmentCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public AttachmentCollectionInstance Construct()
        {
            return new AttachmentCollectionInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class AttachmentCollectionInstance : CollectionInstance<AttachmentCollection, AttachmentInstance, Attachment>
    {
        private readonly AttachmentCollection m_attachmentCollection;

        public AttachmentCollectionInstance(ScriptEngine engine, AttachmentCollection attachmentCollection)
            : base(new CollectionInstance<AttachmentCollection, AttachmentCollectionInstance, Attachment>(engine))
        {
            this.m_attachmentCollection = attachmentCollection;
            Collection = attachmentCollection;

            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override AttachmentInstance Wrap(Attachment attachment)
        {
            return attachment == null
                ? null
                : new AttachmentInstance(this.Engine, attachment);
        }

        protected override Attachment Unwrap(AttachmentInstance attachmentInstance)
        {
            return attachmentInstance == null
                ? null
                : attachmentInstance.Attachment;
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_attachmentCollection.Dispose();
        }
    }
}
