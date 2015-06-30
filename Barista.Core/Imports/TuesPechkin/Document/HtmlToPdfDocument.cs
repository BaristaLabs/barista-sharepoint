namespace Barista.TuesPechkin
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class HtmlToPdfDocument : IDocument
    {
        public HtmlToPdfDocument()
        {
            Objects = new List<ObjectSettings>();
        }

        public HtmlToPdfDocument(string html) : this()
        {
            Objects.Add(new ObjectSettings { HtmlText = html });
        }

        public List<ObjectSettings> Objects { get; private set; }

        public IEnumerable<IObject> GetObjects()
        {
            return Objects.ToArray();
        }

        private GlobalSettings m_global = new GlobalSettings();

        public GlobalSettings GlobalSettings
        {
            get
            {
                return m_global;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_global = value;
            }
        }

        public static implicit operator HtmlToPdfDocument(string html)
        {
            return new HtmlToPdfDocument(html);
        }
    }
}