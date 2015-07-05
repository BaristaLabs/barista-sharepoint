namespace Barista.TuesPechkin
{
    using System;
    using System.ComponentModel;

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ObjectSettings : IObject
    {
        [WkhtmltoxSetting("includeInOutline")]
        public bool? IncludeInOutline { get; set; }

        [WkhtmltoxSetting("pagesCount")]
        public bool? CountPages { get; set; }

        [WkhtmltoxSetting("page")]
        public string PageUrl { get; set; }

        [WkhtmltoxSetting("produceForms")]
        public bool? ProduceForms { get; set; }

        [WkhtmltoxSetting("useExternalLinks")]
        public bool? ProduceExternalLinks { get; set; }

        [WkhtmltoxSetting("useLocalLinks")]
        public bool? ProduceLocalLinks { get; set; }

        public FooterSettings FooterSettings
        {
            get
            {
                return m_footer;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_footer = value;
            }
        }

        public HeaderSettings HeaderSettings
        {
            get
            {
                return m_header;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_header = value;
            }
        }

        public string HtmlText
        {
            get
            {
                return System.Text.Encoding.UTF8.GetString(m_data);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_data = System.Text.Encoding.UTF8.GetBytes(value);
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

        [Browsable(false)]
        public byte[] RawData
        {
            get
            {
                return m_data;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_data = value;
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

        public byte[] GetData()
        {
            return RawData;
        }

        public static implicit operator ObjectSettings(string html)
        {
            return new ObjectSettings { HtmlText = html };
        }

        private byte[] m_data = new byte[0];

        private FooterSettings m_footer = new FooterSettings();

        private HeaderSettings m_header = new HeaderSettings();

        private LoadSettings m_load = new LoadSettings();

        private WebSettings m_web = new WebSettings();
    }
}