namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Web.UI.WebControls.WebParts;

    [Serializable]
    public class WebPartZoneBaseConstructor : ClrFunction
    {
        public WebPartZoneBaseConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "WebPartZoneBase", new WebPartZoneBaseInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public WebPartZoneBaseInstance Construct()
        {
            return new WebPartZoneBaseInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class WebPartZoneBaseInstance : ObjectInstance
    {
        private readonly WebPartZoneBase m_webPartZoneBase;

        public WebPartZoneBaseInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public WebPartZoneBaseInstance(ObjectInstance prototype, WebPartZoneBase webPartZoneBase)
            : this(prototype)
        {
            if (webPartZoneBase == null)
                throw new ArgumentNullException("webPartZoneBase");

            m_webPartZoneBase = webPartZoneBase;
        }

        public WebPartZoneBase WebPartZoneBase
        {
            get
            {
                return m_webPartZoneBase;
            }
        }

        [JSProperty(Name = "headerText")]
        public string HeaderText
        {
            get
            {
                return m_webPartZoneBase.HeaderText;
            }
            set
            {
                m_webPartZoneBase.HeaderText = value;
            }
        }

        [JSProperty(Name = "emptyZoneText")]
        public string EmptyZoneText
        {
            get
            {
                return m_webPartZoneBase.EmptyZoneText;
            }
            set
            {
                m_webPartZoneBase.EmptyZoneText = value;
            }
        }

        //TODO: SOOOO many properties
    }
}
