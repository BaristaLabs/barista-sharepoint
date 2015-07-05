namespace Barista.WkHtmlToPdf.Library
{
    using Barista.Jurassic.Library;
    using Barista.Jurassic;
    using System;
    using Barista.TuesPechkin;

    [Serializable]
    public class HtmlToPdfDocumentConstructor : ClrFunction
    {
        public HtmlToPdfDocumentConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "HtmlToPdfDocument", new HtmlToPdfDocumentInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public HtmlToPdfDocumentInstance Construct()
        {
            var doc = new HtmlToPdfDocument();
            
            return new HtmlToPdfDocumentInstance(InstancePrototype, doc);
        }
    }

    [Serializable]
    public class HtmlToPdfDocumentInstance : ObjectInstance
    {
        private readonly HtmlToPdfDocument m_htmlToPdfDocument;

        public HtmlToPdfDocumentInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public HtmlToPdfDocumentInstance(ObjectInstance prototype, HtmlToPdfDocument htmlToPdfDocument)
            : this(prototype)
        {
            if (htmlToPdfDocument == null)
                throw new ArgumentNullException("htmlToPdfDocument");

            m_htmlToPdfDocument = htmlToPdfDocument;
        }

        public HtmlToPdfDocument HtmlToPdfDocument
        {
            get
            {
                return m_htmlToPdfDocument;
            }
        }

        private GlobalSettingsInstance m_globalSettings;

        [JSProperty(Name = "globalSettings")]
        public GlobalSettingsInstance GlobalSettings
        {
            get
            {
                return m_globalSettings ??
                       (m_globalSettings =
                           new GlobalSettingsInstance(Engine.Object.InstancePrototype,
                               m_htmlToPdfDocument.GlobalSettings));
            }
        }

        private ObjectSettingsListInstance m_objectSettingsList;

        [JSProperty(Name = "objects")]
        public ObjectSettingsListInstance Objects
        {
            get
            {
                return m_objectSettingsList ??
                       (m_objectSettingsList =
                           new ObjectSettingsListInstance(Engine,
                               m_htmlToPdfDocument.Objects));
            }
        }
    }
}
