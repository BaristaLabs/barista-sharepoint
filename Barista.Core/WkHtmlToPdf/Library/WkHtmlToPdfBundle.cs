namespace Barista.WkHtmlToPdf.Library
{
    using System;
    using Barista.TuesPechkin;

    public class WkHtmlToPdfBundle : IBundle
    {
        private readonly string m_binDirectory;
        private static readonly object ConverterLock = new Object();
        private volatile static Lazy<IConverter> s_pdfConverter;
        //private volatile static Lazy<IConverter> s_imageConverter;

        public WkHtmlToPdfBundle(string binDirectory)
        {
            m_binDirectory = binDirectory;

            if (s_pdfConverter == null)
            {
                lock (ConverterLock)
                {
                    if (s_pdfConverter == null)
                    {
                        s_pdfConverter = new Lazy<IConverter>(() =>
                        {
                            IConverter converter =
                                new ThreadSafeConverter(
                                    new PdfToolset(new BaristaDeployment(m_binDirectory)));

                            return converter;
                        });
                    }
                }
            }

            //Toolset locks if multiple instances are created.
            /*if (s_imageConverter == null)
            {
                lock (ConverterLock)
                {
                    if (s_imageConverter == null)
                    {
                        s_imageConverter = new Lazy<IConverter>(() =>
                        {
                            IConverter converter =
                                new ThreadSafeConverter(
                                    new ImageToolset(new BaristaDeployment(m_binDirectory)));

                            return converter;
                        });
                    }
                }
            }*/
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
            result.SetPropertyValue("GlobalSettings", new GlobalSettingsConstructor(engine), false);
            result.SetPropertyValue("ObjectSettings", new ObjectSettingsConstructor(engine), false);
            result.SetPropertyValue("WebSettings", new WebSettingsConstructor(engine), false);
            result.SetPropertyValue("LoadSettings", new LoadSettingsConstructor(engine), false);
            result.SetPropertyValue("HeaderSettings", new HeaderSettingsConstructor(engine), false);
            result.SetPropertyValue("FooterSettings", new FooterSettingsConstructor(engine), false);
            result.SetPropertyValue("MarginSettings", new MarginSettingsConstructor(engine), false);

            
            result.SetPropertyValue("PaperSize", new PaperSizeConstructor(engine), false);
           
            result.SetPropertyValue("pdfConverter", new PdfConverterInstance(engine.Object.InstancePrototype, s_pdfConverter.Value), false);

            //Hmmm... move this to a different bundle with a different directory???
            //result.SetPropertyValue("HtmlToImageDocument", new HtmlToImageDocumentConstructor(engine), false);
            //result.SetPropertyValue("CropSettings", new CropSettingsConstructor(engine), false);
            //result.SetPropertyValue("imageConverter", new ImageConverterInstance(engine.Object.InstancePrototype, s_imageConverter.Value), false);
            return result;
        }
    }
}
