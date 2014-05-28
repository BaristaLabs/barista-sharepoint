namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class MailAddressCollectionConstructor : ClrFunction
    {
        public MailAddressCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "MailAddressCollection", new MailAddressCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public MailAddressCollectionInstance Construct()
        {
            return new MailAddressCollectionInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class MailAddressCollectionInstance : CollectionInstance<MailAddressCollection, MailAddressInstance, MailAddress>
    {
        private readonly MailAddressCollection m_mailAddressCollection;

        public MailAddressCollectionInstance(ScriptEngine engine, MailAddressCollection mailAddressCollection)
            : base(new CollectionInstance<MailAddressCollection, MailAddressCollectionInstance, MailAddress>(engine))
        {
            this.m_mailAddressCollection = mailAddressCollection;
            Collection = mailAddressCollection;

            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override MailAddressInstance Wrap(MailAddress mailAddress)
        {
            return mailAddress == null
                ? null
                : new MailAddressInstance(this.Engine.Object.InstancePrototype, mailAddress);
        }

        protected override MailAddress Unwrap(MailAddressInstance mailAddressInstance)
        {
            return mailAddressInstance == null
                ? null
                : mailAddressInstance.MailAddress;
        }

        [JSFunction(Name = "addAddresses")]
        public void Add(string addresses)
        {
            m_mailAddressCollection.Add(addresses);
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_mailAddressCollection.ToString();
        }
    }
}
