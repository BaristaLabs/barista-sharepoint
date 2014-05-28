namespace Barista.SharePoint.Library
{
    using System;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserConstructor : ClrFunction
    {
        public SPUserConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUser", new SPUserInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPUserInstance Construct(object arg1)
        {
            if (arg1 is SPUserInstance)
            {
                return new SPUserInstance(this.Engine.Object.InstancePrototype, (arg1 as SPUserInstance).User);
            }

            var loginName = TypeConverter.ToString(arg1);

            SPUser user;
            if (SPHelper.TryGetSPUserFromLoginName(loginName, out user) == false)
            {
                throw new JavaScriptException(this.Engine, "Error", "User cannot be found.");
            }

            return new SPUserInstance(this.InstancePrototype, user);
        }

        [JSFunction(Name = "doesUserExist")]
        public bool DoesUserExist(string loginName)
        {
            SPUser user;
            return SPHelper.TryGetSPUserFromLoginName(loginName, out user) && user != null;
        }

    }

    [Serializable]
    public class SPUserInstance : ObjectInstance
    {
        private readonly SPUser m_user;

        public SPUserInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserInstance(ObjectInstance prototype, SPUser user)
            : this(prototype)
        {
            this.m_user = user;
        }

        internal SPUser User
        {
            get { return m_user; }
        }

        [JSProperty(Name = "email")]
        public string Email
        {
            get { return m_user.Email; }
        }

        [JSProperty(Name = "groups")]
        public SPGroupCollectionInstance Groups
        {
            get
            {
                return m_user.Groups == null
                  ? null
                  : new SPGroupCollectionInstance(this.Engine.Object.InstancePrototype, m_user.Groups);
            }
        }

        [JSProperty(Name = "id")]
        public int Id
        {
            get { return m_user.ID; }
        }

        [JSProperty(Name = "isApplicationPrincipal")]
        public bool IsApplicationPrincipal
        {
            get { return m_user.IsApplicationPrincipal; }
        }

        [JSProperty(Name = "isDomainGroup")]
        public bool IsDomainGroup
        {
            get { return m_user.IsDomainGroup; }
        }

        [JSProperty(Name = "isSiteAdmin")]
        public bool IsSiteAdmin
        {
            get { return m_user.IsSiteAdmin; }
        }

        [JSProperty(Name = "isSiteAuditor")]
        public bool IsSiteAuditor
        {
            get { return m_user.IsSiteAuditor; }
        }

        [JSProperty(Name = "loginName")]
        public string LoginName
        {
            get { return m_user.LoginName; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_user.Name; }
        }

        [JSProperty(Name = "notes")]
        public string Notes
        {
            get { return m_user.Notes; }
        }

        [JSProperty(Name = "sid")]
        public string Sid
        {
            get { return m_user.Sid; }
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_user.Xml;
        }
    }
}
