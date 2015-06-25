namespace Barista.SharePoint.Library
{
    using System.Web.UI.WebControls.WebParts;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using WebPart = Microsoft.SharePoint.WebPartPages.WebPart;

    [Serializable]
    public class SPWebPartConstructor : ClrFunction
    {
        public SPWebPartConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebPart", new SPWebPartInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWebPartInstance Construct()
        {
            return new SPWebPartInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWebPartInstance : ObjectInstance
    {
        private readonly WebPart m_webPart;

        public SPWebPartInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWebPartInstance(ObjectInstance prototype, WebPart webPart)
            : this(prototype)
        {
            if (webPart == null)
                throw new ArgumentNullException("webPart");

            m_webPart = webPart;
        }

        public WebPart WebPart
        {
            get
            {
                return m_webPart;
            }
        }

        [JSProperty(Name = "allowClose")]
        public bool AllowClose
        {
            get
            {
                return m_webPart.AllowClose;
            }
            set
            {
                m_webPart.AllowClose = value;
            }
        }

        [JSProperty(Name = "allowConnect")]
        public bool AllowConnect
        {
            get
            {
                return m_webPart.AllowConnect;
            }
            set
            {
                m_webPart.AllowConnect = value;
            }
        }

        [JSProperty(Name = "allowEdit")]
        public bool AllowEdit
        {
            get
            {
                return m_webPart.AllowEdit;
            }
            set
            {
                m_webPart.AllowEdit = value;
            }
        }

        [JSProperty(Name = "allowHide")]
        public bool AllowHide
        {
            get
            {
                return m_webPart.AllowHide;
            }
            set
            {
                m_webPart.AllowHide = value;
            }
        }

        [JSProperty(Name = "allowMinimize")]
        public bool AllowMinimize
        {
            get
            {
                return m_webPart.AllowMinimize;
            }
            set
            {
                m_webPart.AllowMinimize = value;
            }
        }

        [JSProperty(Name = "allowZoneChange")]
        public bool AllowZoneChange
        {
            get
            {
                return m_webPart.AllowZoneChange;
            }
            set
            {
                m_webPart.AllowZoneChange = value;
            }
        }

        [JSProperty(Name = "authorizationFilter")]
        public string AuthorizationFilter
        {
            get
            {
                return m_webPart.AuthorizationFilter;
            }
            set
            {
                m_webPart.AuthorizationFilter = value;
            }
        }

        [JSProperty(Name = "catalogIconImageUrl")]
        public string CatalogIconImageUrl
        {
            get
            {
                return m_webPart.CatalogIconImageUrl;
            }
            set
            {
                m_webPart.CatalogIconImageUrl = value;
            }
        }

        [JSProperty(Name = "chromeState")]
        public string ChromeState
        {
            get
            {
                return m_webPart.ChromeState.ToString();
            }
            set
            {
                PartChromeState state;
                if (value.TryParseEnum(true, out state))
                    m_webPart.ChromeState = state;
            }
        }

        [JSProperty(Name = "chromeType")]
        public string ChromeType
        {
            get
            {
                return m_webPart.ChromeType.ToString();
            }
            set
            {
                PartChromeType state;
                if (value.TryParseEnum(true, out state))
                    m_webPart.ChromeType = state;
            }
        }

        [JSProperty(Name = "connectErrorMessage")]
        public string ConnectErrorMessage
        {
            get
            {
                return m_webPart.ConnectErrorMessage;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_webPart.Description;
            }
            set
            {
                m_webPart.Description = value;
            }
        }

        [JSProperty(Name = "displayTitle")]
        public string DisplayTitle
        {
            get
            {
                return m_webPart.DisplayTitle;
            }
        }

        [JSProperty(Name = "exportMode")]
        public string ExportMode
        {
            get
            {
                return m_webPart.ExportMode.ToString();
            }
            set
            {
                WebPartExportMode state;
                if (value.TryParseEnum(true, out state))
                    m_webPart.ExportMode = state;
            }
        }

        [JSProperty(Name = "hasSharedData")]
        public bool HasSharedData
        {
            get
            {
                return m_webPart.HasSharedData;
            }
        }

        [JSProperty(Name = "hasUserData")]
        public bool HasUserData
        {
            get
            {
                return m_webPart.HasUserData;
            }
        }

        [JSProperty(Name = "height")]
        public string Height
        {
            get
            {
                return m_webPart.Height;
            }
            set
            {
                m_webPart.Height = value;
            }
        }

        [JSProperty(Name = "helpMode")]
        public string HelpMode
        {
            get
            {
                return m_webPart.HelpMode.ToString();
            }
            set
            {
                WebPartHelpMode helpMode;
                if (value.TryParseEnum(true, out helpMode))
                    m_webPart.HelpMode = helpMode;
            }
        }

        [JSProperty(Name = "helpUrl")]
        public string HelpUrl
        {
            get
            {
                return m_webPart.HelpUrl;
            }
            set
            {
                m_webPart.HelpUrl = value;
            }
        }

        [JSProperty(Name = "hidden")]
        public bool Hidden
        {
            get
            {
                return m_webPart.Hidden;
            }
            set
            {
                m_webPart.Hidden = value;
            }
        }

        [JSProperty(Name = "importErrorMessage")]
        public string ImportErrorMessage
        {
            get
            {
                return m_webPart.ImportErrorMessage;
            }
            set
            {
                m_webPart.ImportErrorMessage = value;
            }
        }

        [JSProperty(Name = "isClosed")]
        public bool IsClosed
        {
            get
            {
                return m_webPart.IsClosed;
            }
        }

        [JSProperty(Name = "isShared")]
        public bool IsShared
        {
            get
            {
                return m_webPart.IsShared;
            }
        }

        [JSProperty(Name = "isStandalone")]
        public bool IsStandalone
        {
            get
            {
                return m_webPart.IsStandalone;
            }
        }

        [JSProperty(Name = "isStatic")]
        public bool IsStatic
        {
            get
            {
                return m_webPart.IsStatic;
            }
        }

        [JSProperty(Name = "subtitle")]
        public string Subtitle
        {
            get
            {
                return m_webPart.Subtitle;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_webPart.Title;
            }
            set
            {
                m_webPart.Title = value;
            }
        }

        [JSProperty(Name = "titleIconImageUrl")]
        public string TitleIconImageUrl
        {
            get
            {
                return m_webPart.TitleIconImageUrl;
            }
            set
            {
                m_webPart.TitleIconImageUrl = value;
            }
        }

        [JSProperty(Name = "titleUrl")]
        public string TitleUrl
        {
            get
            {
                return m_webPart.TitleUrl;
            }
            set
            {
                m_webPart.TitleUrl = value;
            }
        }

        //Verbs
        //WebBrowsableObject
        //WebPartManager

        [JSProperty(Name = "width")]
        public string Width
        {
            get
            {
                return m_webPart.Width;
            }
            set
            {
                m_webPart.Width = value;
            }
        }

        [JSProperty(Name = "zone")]
        public WebPartZoneBaseInstance Zone
        {
            get
            {
                return m_webPart.Zone == null
                    ? null
                    : new WebPartZoneBaseInstance(this.Engine.Object.InstancePrototype, m_webPart.Zone);
            }
        }

        [JSProperty(Name = "zoneIndex")]
        public int ZoneIndex
        {
            get
            {
                return m_webPart.ZoneIndex;
            }
        }

        //CreateEditorParts
        //SetPersonalizationDirty
    }
}
