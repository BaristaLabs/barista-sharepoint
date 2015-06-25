namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Text;

    [Serializable]
    public class EncodingConstructor : ClrFunction
    {
        public EncodingConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Encoding", new EncodingInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public EncodingInstance Construct()
        {
            return new EncodingInstance(this.InstancePrototype);
        }

        [JSProperty(Name = "ASCII")]
        public EncodingInstance Ascii
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.ASCII); }
        }

        [JSProperty(Name = "BigEndianUnicode")]
        public EncodingInstance BigEndianUnicode
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.BigEndianUnicode); }
        }

        [JSProperty(Name = "Default")]
        public EncodingInstance Default
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.Default); }
        }

        [JSProperty(Name = "UTF32")]
        public EncodingInstance Utf32
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.UTF32); }
        }

        [JSProperty(Name = "UTF7")]
        public EncodingInstance Utf7
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.UTF7); }
        }

        [JSProperty(Name = "UTF8")]
        public EncodingInstance Utf8
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.UTF8); }
        }

        [JSProperty(Name = "Unicode")]
        public EncodingInstance Unicode
        {
            get { return new EncodingInstance(this.Engine.Object.InstancePrototype, Encoding.Unicode); }
        }

        [JSFunction(Name = "getEncoding")]
        public EncodingInstance GetEncoding(string name)
        {
            var result = Encoding.GetEncoding(name);

            return new EncodingInstance(this.Engine.Object.InstancePrototype, result);
        }
    }

    [Serializable]
    public class EncodingInstance : ObjectInstance
    {
        private readonly Encoding m_encoding;

        public EncodingInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public EncodingInstance(ObjectInstance prototype, Encoding encoding)
            : this(prototype)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            m_encoding = encoding;
        }

        public Encoding Encoding
        {
            get { return m_encoding; }
        }

        [JSProperty(Name = "bodyName")]
        public string BodyName
        {
            get
            {
                return m_encoding.BodyName;
            }
        }

        [JSProperty(Name = "codePage")]
        public int CodePage
        {
            get
            {
                return m_encoding.CodePage;
            }
        }

        [JSProperty(Name = "encodingName")]
        public string EncodingName
        {
            get
            {
                return m_encoding.EncodingName;
            }
        }

        [JSProperty(Name = "headerName")]
        public string HeaderName
        {
            get
            {
                return m_encoding.HeaderName;
            }
        }

        [JSProperty(Name = "isBrowserDisplay")]
        public bool IsBrowserDisplay
        {
            get
            {
                return m_encoding.IsBrowserDisplay;
            }
        }

        [JSProperty(Name = "isBrowserSave")]
        public bool IsBrowserSave
        {
            get
            {
                return m_encoding.IsBrowserSave;
            }
        }

        [JSProperty(Name = "isMailNewsDisplay")]
        public bool IsMailNewsDisplay
        {
            get
            {
                return m_encoding.IsMailNewsDisplay;
            }
        }

        [JSProperty(Name = "isReadOnly")]
        public bool IsReadOnly
        {
            get
            {
                return m_encoding.IsReadOnly;
            }
        }

        [JSProperty(Name = "isSingleByte")]
        public bool IsSingleByte
        {
            get
            {
                return m_encoding.IsSingleByte;
            }
        }

        [JSProperty(Name = "webName")]
        public string WebName
        {
            get
            {
                return m_encoding.WebName;
            }
        }

        [JSProperty(Name = "windowsCodePage")]
        public int WindowsCodePage
        {
            get
            {
                return m_encoding.WindowsCodePage;
            }
        }

        [JSFunction(Name = "getBytes")]
        public Base64EncodedByteArrayInstance GetBytes(string s)
        {
            var result = m_encoding.GetBytes(s);

            return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getPreamble")]
        public Base64EncodedByteArrayInstance GetPreamble()
        {
            var result = m_encoding.GetPreamble();

            return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getString")]
        public string GetString(Base64EncodedByteArrayInstance bytes)
        {
            if (bytes == null)
                return null;

            var result = m_encoding.GetString(bytes.Data);
            return result;
        }
    }
}
