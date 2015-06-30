namespace Barista.TuesPechkin
{
    using System;
    using System.Collections.Generic;

    public class HtmlToImageDocument : IDocument
    {
        [WkhtmltoxSetting("screenHeight")]
        public double ScreenHeight { get; set; }

        [WkhtmltoxSetting("screenWidth")]
        public double? ScreenWidth { get; set; }

        [WkhtmltoxSetting("quality")]
        public double? Quality { get; set; }

        [WkhtmltoxSetting("fmt")]
        public string Format { get; set; }

        [WkhtmltoxSetting("out")]
        public string Out { get; set; }

        [WkhtmltoxSetting("in")]
        public string In { get; set; }

        [WkhtmltoxSetting("transparent")]
        public bool? Transparent { get; set; }

        public CropSettings CropSettings
        {
            get
            {
                return m_crop;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_crop = value;
            }
        }

        public LoadSettings LoadSettings
        {
            get
            {
                return m_load;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_load = value;
            }
        }

        public WebSettings WebSettings
        {
            get
            {
                return m_web;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_web = value;
            }
        }

        public IEnumerable<IObject> GetObjects()
        {
            return new IObject[0];
        }

        private CropSettings m_crop = new CropSettings();

        private LoadSettings m_load = new LoadSettings();

        private WebSettings m_web = new WebSettings();
    }
}
