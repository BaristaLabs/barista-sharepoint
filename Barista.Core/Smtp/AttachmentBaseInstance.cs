namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Net.Mime;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class AttachmentBaseInstance : ObjectInstance
    {
        public AttachmentBaseInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        protected AttachmentBaseInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        public AttachmentBase AttachmentBase
        {
            get;
            set;
        }

        [JSProperty(Name = "contentId")]
        public string ContentId
        {
            get
            {
                return AttachmentBase.ContentId;
            }
            set
            {
                AttachmentBase.ContentId = value;
            }
        }

        [JSProperty(Name = "content")]
        public Base64EncodedByteArrayInstance Content
        {
            get
            {
                if (AttachmentBase.ContentStream == null)
                    return null;

                var data = AttachmentBase.ContentStream.ToByteArray();
                return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data);
            }
        }

        [JSProperty(Name = "contentType")]
        public string ContentType
        {
            get
            {
                return AttachmentBase.ContentType.ToString();
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                    AttachmentBase.ContentType = new ContentType();
                else
                {
                    var contentType = new ContentType(value);
                    AttachmentBase.ContentType = contentType;
                }
            }
        }

        [JSProperty(Name = "transferEncoding")]
        public string TransferEncoding
        {
            get
            {
                return AttachmentBase.TransferEncoding.ToString();
            }
            set
            {
                TransferEncoding transferEncoding;
                if (value.TryParseEnum(true, out transferEncoding))
                    AttachmentBase.TransferEncoding = transferEncoding;
            }
        }

        [JSFunction(Name = "dispose")]
        public virtual void Dispose()
        {
            AttachmentBase.Dispose();
        }
    }
}
