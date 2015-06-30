namespace Barista.WkHtmlToPdf.Library
{
    using Barista.TuesPechkin;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Jurassic;
    using Barista.Library;

    [Serializable]
    public class PdfConverterInstance : ObjectInstance
    {
        private readonly IConverter m_converter;

        public PdfConverterInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public PdfConverterInstance(ObjectInstance prototype, IConverter converter)
            : this(prototype)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            m_converter = converter;
        }

        public IConverter Converter
        {
            get
            {
                return m_converter;
            }
        }

        [JSFunction(Name = "convert")]
        public Base64EncodedByteArrayInstance Convert(HtmlToPdfDocumentInstance document, object fileName)
        {
            var pdfBytes = m_converter.Convert(document.HtmlToPdfDocument);
            var result = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, pdfBytes)
            {
                MimeType = "application/pdf"
            };

            if (fileName != null && fileName != Null.Value && fileName != Undefined.Value)
                result.FileName = fileName.ToString();

            return result;
        }
    }
}
