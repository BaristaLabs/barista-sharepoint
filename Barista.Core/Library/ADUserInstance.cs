namespace Barista.Library
{
    using System;
    using System.Linq;
    using Jurassic;
    using Jurassic.Library;
    using Barista.DirectoryServices;

    [Serializable]
    public class ADUserConstructor : ClrFunction
    {
        public ADUserConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ADUser", new ADUserInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ADUserInstance Construct()
        {
            var user = new ADUser();

            return new ADUserInstance(InstancePrototype, user);
        }

        public ADUserInstance Construct(ADUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            return new ADUserInstance(InstancePrototype, user);
        }
    }

    [Serializable]
    public class ADUserInstance : ObjectInstance
    {
        private readonly ADUser m_user;
        private readonly string m_ldap;

        public ADUserInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public ADUserInstance(ObjectInstance prototype, ADUser user)
            : this(prototype)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            m_user = user;
        }

        public ADUserInstance(ObjectInstance prototype, ADUser user, string ldap)
            : this(prototype, user)
        {
            m_ldap = ldap;
        }

        #region General

        [JSProperty(Name = "rawSid")]
        // ReSharper disable InconsistentNaming
        public object RawsID
        // ReSharper restore InconsistentNaming
        {
            get { return StringHelper.ByteArrayToString((byte[])m_user.RawsID); }
        }

        [JSProperty(Name = "sId")]
        // ReSharper disable InconsistentNaming
        public object sID
        // ReSharper restore InconsistentNaming
        {
            get { return m_user.sID; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_user.Name; }
        }

        [JSProperty(Name = "firstName")]
        public string FirstName
        {
            get { return m_user.FirstName; }
        }

        [JSProperty(Name = "initials")]
        public string Initials
        {
            get { return m_user.Initials; }
        }

        [JSProperty(Name = "lastName")]
        public string LastName
        {
            get { return m_user.LastName; }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return m_user.DisplayName; }
        }

        [JSProperty(Name = "distinguishedName")]
        public string DistinguishedName
        {
            get
            {
                return m_user.DistinguishedName;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get { return m_user.Description; }
        }

        [JSProperty(Name = "office")]
        public string Office
        {
            get { return m_user.Office; }
        }

        [JSProperty(Name = "email")]
        public string Email
        {
            get { return m_user.Email; }
        }

        [JSProperty(Name = "homePage")]
        public string HomePage
        {
            get { return m_user.HomePage; }
        }
        #endregion

        #region Address

        [JSProperty(Name = "street")]
        [DirectoryAttribute("streetAddress")]
        public string Street
        {
            get { return m_user.Street; }
        }

        [JSProperty(Name = "poBox")]
        // ReSharper disable InconsistentNaming
        public string POBox
        // ReSharper restore InconsistentNaming
        {
            get { return m_user.POBox; }
        }
        [JSProperty(Name = "city")]
        public string City
        {
            get { return m_user.City; }
        }

        [JSProperty(Name = "state")]
        public string State
        {
            get { return m_user.State; }
        }

        [JSProperty(Name = "zip")]
        public string Zip
        {
            get { return m_user.Zip; }
        }

        [JSProperty(Name = "country")]
        public string Country
        {
            get { return m_user.Country; }
        }
        #endregion

        #region Account
        [JSProperty(Name = "userLogonName")]
        public string UserLogonName
        {
            get { return m_user.UserLogonName; }
        }

        [JSProperty(Name = "preWin2kLogonName")]
        // ReSharper disable InconsistentNaming
        public string PreWin2kLogonName
        // ReSharper restore InconsistentNaming
        {
            get { return m_user.PreWin2kLogonName; }
        }

        [JSProperty(Name = "isAccountDisabled")]
        public int AccountDisabled
        {
            get { return m_user.AccountDisabled; }
        }

        [JSProperty(Name = "logonCount")]
        public int LogonCount
        {
            get { return m_user.LogonCount; }
        }

        [JSProperty(Name = "passwordLastSet")]
        public DateInstance PasswordLastSet
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.PasswordLastSet); }
        }

        [JSProperty(Name = "lastLogon")]
        public DateInstance LastLogon
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.LastLogon); }
        }

        [JSProperty(Name = "lastLogoff")]
        public DateInstance LastLogoff
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.LastLogoff); }
        }

        [JSProperty(Name = "badPasswordTime")]
        public DateInstance BadPasswordTime
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.BadPasswordTime); }
        }

        [JSProperty(Name = "badPasswordCount")]
        public int BadPasswordCount
        {
            get { return m_user.BadPasswordCount; }
        }

        [JSProperty(Name = "lastSuccessfulInteractiveLogonTime")]
        public DateInstance LastSuccessfulInteractiveLogonTime
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.LastSuccessfulInteractiveLogonTime); }
        }

        [JSProperty(Name = "lastFailedInteractiveLogonTime")]
        public DateInstance LastFailedInteractiveLogonTime
        {
            get { return JurassicHelper.ToDateInstance(Engine, m_user.LastFailedInteractiveLogonTime); }
        }

        [JSProperty(Name = "failedInteractiveLogonCount")]
        public int FailedInteractiveLogonCount
        {
            get { return m_user.FailedInteractiveLogonCount; }
        }

        [JSProperty(Name = "failedInteractiveLogonCountAtLastSuccessfulLogon")]
        public int FailedInteractiveLogonCountAtLastSuccessfulLogon
        {
            get { return m_user.FailedInteractiveLogonCountAtLastSuccessfulLogon; }
        }
        #endregion

        #region Phone

        [JSProperty(Name = "homePhone")]
        public string HomePhone
        {
            get { return m_user.HomePhone; }
        }

        [JSProperty(Name = "phoneNumber")]
        public string PhoneNumber
        {
            get { return m_user.PhoneNumber; }
        }

        [JSProperty(Name = "mobileNumber")]
        public string MobileNumber
        {
            get { return m_user.MobileNumber; }
        }

        [JSProperty(Name = "faxNumber")]
        [DirectoryAttribute("facsimileTelephoneNumber")]
        public string FaxNumber
        {
            get { return m_user.FaxNumber; }
        }

        [JSProperty(Name = "pager")]
        public string Pager
        {
            get { return m_user.Pager; }
        }

        [JSProperty(Name = "ipPhone")]
        public string IpPhone
        {
            get { return m_user.IpPhone; }
        }
        #endregion

        #region Organization
        [JSProperty(Name = "title")]
        public string Title
        {
            get { return m_user.Title; }
        }

        [JSProperty(Name = "department")]
        public string Department
        {
            get { return m_user.Department; }
        }

        [JSProperty(Name = "company")]
        public string Company
        {
            get { return m_user.Company; }
        }

        [JSProperty(Name = "managerLdap")]
        public string ManagerLdap
        {
            get { return m_user.ManagerLdap; }
        }

        [JSProperty(Name = "managerName")]
        public string ManagerName
        {
            get { return m_user.ManagerName; }
        }
        #endregion

        [JSProperty(Name = "groups")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Groups
        {
            get
            {
                var result = Engine.Array.Construct();

                foreach (var group in m_user.Groups)
                {
                    ArrayInstance.Push(result, group);
                }

                return result;
            }
        }

        [JSFunction(Name = "expandGroups")]
        [JSDoc("ternReturnType", "[+ADGroup]")]
        public ArrayInstance ExpandGroups()
        {
            var result = Engine.Array.Construct();

            foreach (var group in m_user.Groups.Select(memberLogonName => ADHelper.GetADGroupByDistinguishedName(memberLogonName, m_ldap)))
            {
                if (group == null)
                    continue;

                ArrayInstance.Push(result, new ADGroupInstance(Engine.Object.InstancePrototype, group));
            }

            return result;
        }
    }
}
