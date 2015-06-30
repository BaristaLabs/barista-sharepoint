namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Newtonsoft.Json;
    using Barista.TuesPechkin;

    [Serializable]
    public class PaperSizeConstructor : ClrFunction
    {
        public PaperSizeConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "PaperSize", new PaperSizeInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public PaperSizeInstance Construct()
        {
            return new PaperSizeInstance(InstancePrototype, new PechkinPaperSize("8.5in", "11in"));
        }
    }

    [Serializable]
    public class PaperSizeInstance : ObjectInstance
    {
        private readonly PechkinPaperSize m_pechkinPaperSize;

        public PaperSizeInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public PaperSizeInstance(ObjectInstance prototype, PechkinPaperSize pechkinPaperSize)
            : this(prototype)
        {
            if (pechkinPaperSize == null)
                throw new ArgumentNullException("pechkinPaperSize");

            m_pechkinPaperSize = pechkinPaperSize;
        }

        public PechkinPaperSize PechkinPaperSize
        {
            get
            {
                return m_pechkinPaperSize;
            }
        }

        [JSProperty(Name = "height")]
        [JsonProperty("height")]
        public string Height
        {
            get
            {
                return m_pechkinPaperSize.Height;
            }
            set
            {
                m_pechkinPaperSize.Height = value;
            }
        }

        [JSProperty(Name = "width")]
        [JsonProperty("width")]
        public string Width
        {
            get
            {
                return m_pechkinPaperSize.Width;
            }
            set
            {
                m_pechkinPaperSize.Width = value;
            }
        }
    }
}
