namespace Barista.Smtp
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using System;
    using System.Net.Mail;
    using System.Text;

    [Serializable]
    public class MailMessageConstructor : ClrFunction
    {
        public MailMessageConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "MailMessage", new MailMessageInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public MailMessageInstance Construct(object from, object to, object subject, object body)
        {
            if (from == Undefined.Value && to == Undefined.Value)
                return new MailMessageInstance(this.InstancePrototype, new MailMessage());
            
            if (from is MailAddressInstance && to is MailAddressInstance)
            {
                var maFrom = from as MailAddressInstance;
                var maTo = to as MailAddressInstance;
                return new MailMessageInstance(this.InstancePrototype, new MailMessage(maFrom.MailAddress, maTo.MailAddress));
            }


            if (subject == Undefined.Value && body == Undefined.Value)
                return new MailMessageInstance(this.InstancePrototype, new MailMessage(TypeConverter.ToString(from), TypeConverter.ToString(to)));


            return new MailMessageInstance(this.InstancePrototype, new MailMessage(TypeConverter.ToString(from), TypeConverter.ToString(to), TypeConverter.ToString(subject), TypeConverter.ToString(body)));
        }
    }

    [Serializable]
    public class MailMessageInstance : ObjectInstance
    {
        private readonly MailMessage m_mailMessage;

        public MailMessageInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public MailMessageInstance(ObjectInstance prototype, MailMessage mailMessage)
            : this(prototype)
        {
            if (mailMessage == null)
                throw new ArgumentNullException("mailMessage");

            m_mailMessage = mailMessage;
        }

        public MailMessage MailMessage
        {
            get { return m_mailMessage; }
        }

        [JSProperty(Name = "alternateViews")]
        public AlternateViewCollectionInstance AlternateViews
        {
            get
            {
                return new AlternateViewCollectionInstance(this.Engine, m_mailMessage.AlternateViews);
            }
        }

        [JSProperty(Name = "attachments")]
        public AttachmentCollectionInstance Attachments
        {
            get
            {
                return new AttachmentCollectionInstance(this.Engine, m_mailMessage.Attachments);
            }
        }

        [JSProperty(Name = "bcc")]
        public MailAddressCollectionInstance Bcc
        {
            get
            {
                return new MailAddressCollectionInstance(this.Engine, m_mailMessage.Bcc);
            }
        }

        [JSProperty(Name = "body")]
        public string Body
        {
            get
            {
                return m_mailMessage.Body;
            }
            set
            {
                m_mailMessage.Body = value;
            }
        }

        [JSProperty(Name = "bodyEncoding")]
        public EncodingInstance BodyEncoding
        {
            get
            {
                return new EncodingInstance(this.Engine.Object.InstancePrototype, m_mailMessage.BodyEncoding);
            }
            set
            {
                m_mailMessage.BodyEncoding = value == null
                    ? Encoding.Default
                    : value.Encoding;
            }
        }

        [JSProperty(Name = "cc")]
        public MailAddressCollectionInstance Cc
        {
            get
            {
                return new MailAddressCollectionInstance(this.Engine, m_mailMessage.CC);
            }
        }

        [JSProperty(Name = "deliveryNotificationOptions")]
        [JSDoc(
            "Gets or sets the delivery notifications for this e-mail message. Possible values are 'Delay', 'Never', 'None', 'OnFailure', 'OnSuccess'"
            )]
        public string DeliveryNotificationOptions
        {
            get
            {
                return m_mailMessage.DeliveryNotificationOptions.ToString();
            }
            set
            {
                DeliveryNotificationOptions options;
                if (value.TryParseEnum(true, out options))
                    m_mailMessage.DeliveryNotificationOptions = options;
            }
        }

        [JSProperty(Name = "from")]
        public MailAddressInstance From
        {
            get
            {
                if (m_mailMessage.From == null)
                    return null;

                return new MailAddressInstance(this.Engine.Object.InstancePrototype, m_mailMessage.From);
            }
            set
            {
                m_mailMessage.From = value == null
                    ? null
                    : value.MailAddress;
            }
        }

        [JSProperty(Name = "isBodyHtml")]
        public bool IsBodyHtml
        {
            get
            {
                return m_mailMessage.IsBodyHtml;
            }
            set
            {
                m_mailMessage.IsBodyHtml = value;
            }
        }

        [JSProperty(Name = "priority")]
        [JSDoc("Specifies the priority of the mail message. Possible values are 'High', 'Low', 'Normal'")]
        public string Priority
        {
            get
            {
                return m_mailMessage.Priority.ToString();
            }
            set
            {
                MailPriority priority;
                if (value.TryParseEnum(true, out priority))
                    m_mailMessage.Priority = priority;
            }
        }

        [JSProperty(Name = "replyTo")]
        public MailAddressInstance ReplyTo
        {
            get
            {
                if (m_mailMessage.ReplyTo == null)
                    return null;

                return new MailAddressInstance(this.Engine.Object.InstancePrototype, m_mailMessage.ReplyTo);
            }
            set
            {
                m_mailMessage.ReplyTo = value == null
                    ? null
                    : value.MailAddress;
            }
        }

        [JSProperty(Name = "sender")]
        public MailAddressInstance Sender
        {
            get
            {
                if (m_mailMessage.Sender == null)
                    return null;

                return new MailAddressInstance(this.Engine.Object.InstancePrototype, m_mailMessage.Sender);
            }
            set
            {
                m_mailMessage.Sender = value == null
                    ? null
                    : value.MailAddress;
            }
        }

        [JSProperty(Name = "subject")]
        public string Subject
        {
            get
            {
                return m_mailMessage.Subject;
            }
            set
            {
                m_mailMessage.Subject = value;
            }
        }

        [JSProperty(Name = "subjectEncoding")]
        public EncodingInstance SubjectEncoding
        {
            get
            {
                return new EncodingInstance(this.Engine.Object.InstancePrototype, m_mailMessage.SubjectEncoding);
            }
            set
            {
                m_mailMessage.SubjectEncoding = value == null
                    ? Encoding.Default
                    : value.Encoding;
            }
        }

        [JSProperty(Name = "to")]
        public MailAddressCollectionInstance To
        {
            get
            {
                return new MailAddressCollectionInstance(this.Engine, m_mailMessage.To);
            }
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_mailMessage.Dispose();
        }
    }
}
