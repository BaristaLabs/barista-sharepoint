namespace Barista.Smtp
{
    using System.Net.Mime;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class ContentDispositionConstructor : ClrFunction
    {
        public ContentDispositionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ContentDisposition", new ContentDispositionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ContentDispositionInstance Construct(object contentDisposition)
        {
            if (contentDisposition == Null.Value || contentDisposition == null || contentDisposition == Undefined.Value)
                return new ContentDispositionInstance(this.InstancePrototype, new ContentDisposition());
            
            return new ContentDispositionInstance(this.InstancePrototype, new ContentDisposition(TypeConverter.ToString(contentDisposition)));
        }
    }

    [Serializable]
    public class ContentDispositionInstance : ObjectInstance
    {
        private readonly ContentDisposition m_contentDisposition;

        public ContentDispositionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public ContentDispositionInstance(ObjectInstance prototype, ContentDisposition contentDisposition)
            : this(prototype)
        {
            if (contentDisposition == null)
                throw new ArgumentNullException("contentDisposition");

            m_contentDisposition = contentDisposition;
        }

        public ContentDisposition ContentDisposition
        {
            get { return m_contentDisposition; }
        }

        [JSProperty(Name = "creationDate")]
        public DateInstance CreationDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_contentDisposition.CreationDate);
            }
            set {
                m_contentDisposition.CreationDate = value == null
                    ? default(DateTime)
                    : value.Value;
            }
        }

        [JSProperty(Name = "dispositionType")]
        public string DispositionType
        {
            get
            {
                return m_contentDisposition.DispositionType;
            }
            set
            {
                m_contentDisposition.DispositionType = value;
            }
        }

        [JSProperty(Name = "fileName")]
        public string FileName
        {
            get
            {
                return m_contentDisposition.FileName;
            }
            set
            {
                m_contentDisposition.FileName = value;
            }
        }

        [JSProperty(Name = "inline")]
        public bool Inline
        {
            get
            {
                return m_contentDisposition.Inline;
            }
            set
            {
                m_contentDisposition.Inline = value;
            }
        }

        [JSProperty(Name = "modificationDate")]
        public DateInstance ModificationDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_contentDisposition.ModificationDate);
            }
            set
            {
                m_contentDisposition.ModificationDate = value == null
                    ? default(DateTime)
                    : value.Value;
            }
        }


        [JSProperty(Name = "parameters")]
        public StringDictionaryInstance Parameters
        {
            get
            {
                return new StringDictionaryInstance(this.Engine.Object.InstancePrototype, m_contentDisposition.Parameters);
            }
        }

        [JSProperty(Name = "readDate")]
        public DateInstance ReadDate
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_contentDisposition.ReadDate);
            }
            set
            {
                m_contentDisposition.ReadDate = value == null
                    ? default(DateTime)
                    : value.Value;
            }
        }

        [JSProperty(Name = "size")]
        public Double Size
        {
            get
            {
                return m_contentDisposition.Size;
            }
            set
            {
                m_contentDisposition.Size = (long)value;
            }
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_contentDisposition.ToString();
        }
    }
}
