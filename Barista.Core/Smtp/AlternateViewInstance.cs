namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Text;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class AlternateViewConstructor : ClrFunction
    {
        public AlternateViewConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "AlternateView", new AlternateViewInstance(engine.Object.InstancePrototype))
        {
            PopulateFunctions();
        }

        [JSConstructorFunction]
        public AlternateViewInstance Construct(string fileName)
        {
            return new AlternateViewInstance(this.InstancePrototype, new AlternateView(fileName));
        }

        [JSFunction(Name = "createAlternateViewFromString")]
        public AlternateViewInstance CreateAlternateViewFromString(string content, object encoding, object contentType)
        {
            if (encoding == Undefined.Value && contentType == Undefined.Value)
                return new AlternateViewInstance(this.InstancePrototype, AlternateView.CreateAlternateViewFromString(content));

            var enc = encoding as EncodingInstance;

            var actualEncoding = enc == null
                ? Encoding.GetEncoding(TypeConverter.ToString(encoding))
                : enc.Encoding;

            if (enc == null)
                actualEncoding = Encoding.Default;

            var ct = TypeConverter.ToString(contentType);
            return new AlternateViewInstance(this.InstancePrototype, AlternateView.CreateAlternateViewFromString(content, actualEncoding, ct));
        }
    }

    [Serializable]
    public class AlternateViewInstance : ObjectInstance
    {
        private readonly AlternateView m_alternateView;

        public AlternateViewInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public AlternateViewInstance(ObjectInstance prototype, AlternateView alternateView)
            : this(prototype)
        {
            if (alternateView == null)
                throw new ArgumentNullException("alternateView");

            m_alternateView = alternateView;
        }

        public AlternateView AlternateView
        {
            get { return m_alternateView; }
        }

        [JSProperty(Name = "baseUri")]
        public UriInstance BaseUri
        {
            get;
            set;
        }

        [JSProperty(Name = "linkedResources")]
        public LinkedResourceCollectionInstance LinkedResources
        {
            get;
            set;
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_alternateView.Dispose();
        }
    }
}
