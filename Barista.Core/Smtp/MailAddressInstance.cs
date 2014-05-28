namespace Barista.Smtp
{
    using System.Net.Mail;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class MailAddressConstructor : ClrFunction
    {
        public MailAddressConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "MailAddress", new MailAddressInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public MailAddressInstance Construct(string address, object displayName, object encoding)
        {
            if (displayName == Undefined.Value && encoding == Undefined.Value)
                return new MailAddressInstance(this.InstancePrototype, new MailAddress(address));

            var enc = encoding as EncodingInstance;
            if (encoding == Undefined.Value || enc == null)
                return new MailAddressInstance(this.InstancePrototype, new MailAddress(address, TypeConverter.ToString(displayName)));

            return new MailAddressInstance(this.InstancePrototype, new MailAddress(address, TypeConverter.ToString(displayName), enc.Encoding));
        }
    }

    [Serializable]
    public class MailAddressInstance : ObjectInstance
    {
        private readonly MailAddress m_mailAddress;

        public MailAddressInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public MailAddressInstance(ObjectInstance prototype, MailAddress mailAddress)
            : this(prototype)
        {
            if (mailAddress == null)
                throw new ArgumentNullException("mailAddress");

            m_mailAddress = mailAddress;
        }

        public MailAddress MailAddress
        {
            get { return m_mailAddress; }
        }

        [JSProperty(Name = "address")]
        public string Address
        {
            get
            {
                return m_mailAddress.Address;
            }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get
            {
                return m_mailAddress.DisplayName;
            }
        }

        [JSProperty(Name = "host")]
        public string Host
        {
            get
            {
                return m_mailAddress.Host;
            }
        }

        [JSProperty(Name = "user")]
        public string User
        {
            get
            {
                return m_mailAddress.User;
            }
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_mailAddress.ToString();
        }
    }
}
