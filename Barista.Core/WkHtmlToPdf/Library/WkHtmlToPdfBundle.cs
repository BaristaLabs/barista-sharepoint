namespace Barista.WkHtmlToPdf.Library
{
    using System;
    using Barista.TuesPechkin;

    public class WkHtmlToPdfBundle : IBundle
    {
        private readonly string m_binDirectory;
        private static readonly object ConverterLock = new Object();
        private volatile static Lazy<IConverter> s_converter;

        public WkHtmlToPdfBundle(string binDirectory)
        {
            m_binDirectory = binDirectory;

            if (s_converter == null)
            {
                lock (ConverterLock)
                {
                    if (s_converter == null)
                    {
                        s_converter = new Lazy<IConverter>(() =>
                        {
                            IConverter converter =
                                new ThreadSafeConverter(
                                    new PdfToolset(new BaristaDeployment(m_binDirectory)));

                            return converter;
                        });
                    }
                }
            }
        }

        public bool IsSystemBundle
        {
            get
            {
                return true;
            }
        }

        public string BundleName
        {
            get
            {
                return "WkHtmlToPdf";
            }
        }

        public string BundleDescription
        {
            get
            {
                return "Provides Html to Pdf capability via the WkHtmlToPdf library.";
            }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            var result = engine.Object.Construct();
            result.SetPropertyValue("HtmlToPdfDocument", new HtmlToPdfDocumentConstructor(engine), false);
            result.SetPropertyValue("ObjectSettings", new ObjectSettingsConstructor(engine), false);
            //result.SetPropertyValue("HtmlToPdfImage", new HtmlToPdfImageConstructor(engine), false);
            result.SetPropertyValue("converter", new ConverterInstance(engine.Object.InstancePrototype, s_converter.Value), false);

            return result;
        }
    }
}
