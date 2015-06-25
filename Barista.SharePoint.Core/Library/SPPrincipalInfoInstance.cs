namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint.Utilities;
    using System;

    [Serializable]
    public class SPPrincipalInfoInstance : ObjectInstance
    {
        private readonly SPPrincipalInfo m_principalInfo;

        internal SPPrincipalInfoInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public SPPrincipalInfoInstance(ScriptEngine engine, SPPrincipalInfo principalInfo)
            : this(engine)
        {
            if (principalInfo == null)
                throw new JavaScriptException(engine, "Error", "$camelCasedName must be specified.");

            m_principalInfo = principalInfo;
        }

        protected SPPrincipalInfoInstance(ObjectInstance prototype, SPPrincipalInfo principalInfo)
            : base(prototype)
        {
            if (principalInfo == null)
                throw new ArgumentNullException("principalInfo");

            m_principalInfo = principalInfo;
        }

        public SPPrincipalInfo SPPrincipalInfo
        {
            get
            {
                return m_principalInfo;
            }
        }

        [JSProperty(Name = "department")]
        public string Department
        {
            get
            {
                return m_principalInfo.Department;
            }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get
            {
                return m_principalInfo.DisplayName;
            }
        }

        [JSProperty(Name = "email")]
        public string Email
        {
            get
            {
                return m_principalInfo.Email;
            }
        }

        [JSProperty(Name = "isSharePointGroup")]
        public bool IsSharePointGroup
        {
            get
            {
                return m_principalInfo.IsSharePointGroup;
            }
        }

        [JSProperty(Name = "jobTitle")]
        public string JobTitle
        {
            get
            {
                return m_principalInfo.JobTitle;
            }
        }

        [JSProperty(Name = "loginName")]
        public string LoginName
        {
            get
            {
                return m_principalInfo.LoginName;
            }
        }

        [JSProperty(Name = "mobile")]
        public string Mobile
        {
            get
            {
                return m_principalInfo.Mobile;
            }
        }

        [JSProperty(Name = "principalId")]
        public int PrincipalId
        {
            get
            {
                return m_principalInfo.PrincipalId;
            }
        }

        [JSProperty(Name = "principalType")]
        public string PrincipalType
        {
            get
            {
                return m_principalInfo.PrincipalType.ToString();
            }
        }

        [JSProperty(Name = "sipAddress")]
        public string SipAddress
        {
            get
            {
                return m_principalInfo.SIPAddress;
            }
        }
    }
}
