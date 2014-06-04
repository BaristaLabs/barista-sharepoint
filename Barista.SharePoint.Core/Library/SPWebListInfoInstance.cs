namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPWebListInfoConstructor : ClrFunction
    {
        public SPWebListInfoConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebListInfo", new SPWebListInfoInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWebListInfoInstance Construct()
        {
            return new SPWebListInfoInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWebListInfoInstance : ObjectInstance
    {
        private readonly SPWebListInfo m_webListInfo;

        public SPWebListInfoInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWebListInfoInstance(ObjectInstance prototype, SPWebListInfo webListInfo)
            : this(prototype)
        {
            if (webListInfo == null)
                throw new ArgumentNullException("webListInfo");

            m_webListInfo = webListInfo;
        }

        public SPWebListInfo SPWebListInfo
        {
            get
            {
                return m_webListInfo;
            }
        }

        [JSProperty(Name = "hasUniqueRoleAssignments")]
        public bool HasUniqueRoleAssignments
        {
            get
            {
                return m_webListInfo.HasUniqueRoleAssignments;
            }
        }

        [JSProperty(Name = "listId")]
        public GuidInstance ListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_webListInfo.ListId);
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_webListInfo.Title;
            }
        }

        [JSProperty(Name = "type")]
        public string Type
        {
            get
            {
                return m_webListInfo.Type.ToString();
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_webListInfo.Url;
            }
        }

        [JSProperty(Name = "webId")]
        public GuidInstance WebId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_webListInfo.WebId);
            }
        }
    }
}
