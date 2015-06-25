namespace Barista.Smtp
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using System;
    using System.IO;
    using System.Net.Mail;
    using System.Reflection;

    [Serializable]
    public class LinkedResourceConstructor : ClrFunction
    {
        public LinkedResourceConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "LinkedResource", new LinkedResourceInstance(engine))
        {
        }

        [JSConstructorFunction]
        public LinkedResourceInstance Construct(Base64EncodedByteArrayInstance linkedResource)
        {
            if (linkedResource == null)
                throw new JavaScriptException(this.Engine, "Error", "A linkedResource must be specified as the first argument.");

             var ms = new MemoryStream(linkedResource.Data);
            return new LinkedResourceInstance(this.Engine, new LinkedResource(ms, linkedResource.MimeType));
        }
    }

    [Serializable]
    public class LinkedResourceInstance : AttachmentBaseInstance
    {
        private readonly LinkedResource m_linkedResource;

        public LinkedResourceInstance(ScriptEngine engine)
            : base(new AttachmentBaseInstance(engine))
        {
        }

        public LinkedResourceInstance(ScriptEngine engine, LinkedResource linkedResource)
            : base(new AttachmentBaseInstance(engine))
        {
            if (linkedResource == null)
                throw new ArgumentNullException("linkedResource");

            m_linkedResource = linkedResource;

            this.AttachmentBase = m_linkedResource;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public LinkedResource LinkedResource
        {
            get { return m_linkedResource; }
        }

        [JSProperty(Name = "contentLink")]
        public UriInstance ContentLink
        {
            get
            {
                return new UriInstance(this.Engine.Object.InstancePrototype, m_linkedResource.ContentLink);
            }
        }
    }
}
