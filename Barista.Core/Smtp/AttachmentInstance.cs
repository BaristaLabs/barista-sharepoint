namespace Barista.Smtp
{
    using System.Text;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Reflection;

    [Serializable]
    public class AttachmentConstructor : ClrFunction
    {
        public AttachmentConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Attachment", new AttachmentInstance(engine))
        {
            PopulateFunctions();
        }

        [JSConstructorFunction]
        public AttachmentInstance Construct(Base64EncodedByteArrayInstance attachment)
        {
            if (attachment == null)
                throw new JavaScriptException(this.Engine, "Error", "An attachment must be specified as the first argument.");

            var ms = new MemoryStream(attachment.Data);
            return new AttachmentInstance(this.Engine, new Attachment(ms, attachment.FileName, attachment.MimeType));
        }

        [JSFunction(Name = "createAttachmentFromString")]
        public AttachmentInstance CreateAttachmentFromString(string content, string name, object encoding, object contentType)
        {
            if (encoding == Undefined.Value && contentType == Undefined.Value)
                return new AttachmentInstance(this.Engine, Attachment.CreateAttachmentFromString(content, name));

            var enc = encoding as EncodingInstance;

            var actualEncoding = enc == null
                ? Encoding.GetEncoding(TypeConverter.ToString(encoding))
                : enc.Encoding;

            if (enc == null)
                actualEncoding = Encoding.Default;

            var ct = TypeConverter.ToString(contentType);
            return new AttachmentInstance(this.Engine, Attachment.CreateAttachmentFromString(content, name, actualEncoding, ct));
        }
    }

    [Serializable]
    public class AttachmentInstance : AttachmentBaseInstance
    {
        private readonly Attachment m_attachment;

        public AttachmentInstance(ScriptEngine engine)
            : base(new AttachmentBaseInstance(engine))
        {
        }

        public AttachmentInstance(ScriptEngine engine, Attachment attachment)
            : base(new AttachmentBaseInstance(engine))
        {
            if (attachment == null)
                throw new ArgumentNullException("attachment");

            m_attachment = attachment;

            this.AttachmentBase = m_attachment;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public Attachment Attachment
        {
            get { return m_attachment; }
        }

        [JSProperty(Name = "contentDisposition")]
        public ContentDispositionInstance ContentDisposition
        {
            get
            {
                return new ContentDispositionInstance(this.Engine.Object.InstancePrototype, m_attachment.ContentDisposition);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_attachment.Name;
            }
            set
            {
                m_attachment.Name = value;
            }
        }

        [JSProperty(Name = "nameEncoding")]
        public EncodingInstance NameEncoding
        {
            get
            {
                return new EncodingInstance(this.Engine.Object.InstancePrototype, m_attachment.NameEncoding);
            }
            set
            {
                if (value == null)
                    m_attachment.NameEncoding = Encoding.Default;
                else
                    m_attachment.NameEncoding = value.Encoding;
            }
        }
    }
}
