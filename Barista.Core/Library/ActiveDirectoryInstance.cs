namespace Barista.Library
{
    using System;
    using System.Security.Principal;
    using Barista.Extensions;
    using Jurassic;
    using Jurassic.Library;
    using Barista.DirectoryServices;

    [Serializable]
    public class ActiveDirectoryInstance : ObjectInstance
    {
        private string m_ldapPathOverride;

        public ActiveDirectoryInstance(ScriptEngine engine)
            : base(engine)
        {

            CurrentUserLoginNameFactory = () =>
            {
                var currentWindowsIdentity = WindowsIdentity.GetCurrent();
                return currentWindowsIdentity == null ? "" : currentWindowsIdentity.Name;
            };

            PopulateFields();
            PopulateFunctions();
        }

        /// <summary>
        /// Gets or sets the username of the current user.
        /// </summary>
        public Func<string> CurrentUserLoginNameFactory
        {
            get;
            set;
        }

        [JSProperty(Name = "ldapPath")]
        [JSDoc("Gets or sets the current ldap path that will be used.")]
        public string LdapPath
        {
            get
            {
                return m_ldapPathOverride.IsNullOrWhiteSpace()
                    ? ADHelper.LdapPath
                    : m_ldapPathOverride;
            }
            set
            {
                if (value.IsNullOrWhiteSpace())
                {
                    m_ldapPathOverride = null;
                    return;
                }

                m_ldapPathOverride = TypeConverter.ToString(value);
            }
        }

        [JSProperty(Name = "currentDomain")]
        [JSDoc("Gets the current domain, if the current machine is not joined to a domain, null is returned.")]
        public string CurrentDomain
        {
            get { return ADHelper.GetJoinedDomain(); }
        }

        [JSFunction(Name = "getADUser")]
        [JSDoc("Returns an object representating the specified user. If no login name is specified, returns the current user.")]
        public object GetADUser(object loginName)
        {
            ADUser user;
            if (loginName == null || loginName == Undefined.Value || loginName == Null.Value ||
                TypeConverter.ToString(loginName).IsNullOrWhiteSpace())
            {
                user = ADHelper.GetADUser(CurrentUserLoginNameFactory(), LdapPath);
            }
            else
                user = ADHelper.GetADUser(TypeConverter.ToString(loginName), LdapPath);

            return user == null
              ? null
              : new ADUserInstance(Engine.Object.InstancePrototype, user, LdapPath);
        }

        [JSFunction(Name = "getADUserByDistinguishedName")]
        [JSDoc("Returns an object representating the specified user.")]
        public object GetADUserByDistinguishedName(object distinguishedName)
        {
            if (distinguishedName == null || distinguishedName == Undefined.Value || distinguishedName == Null.Value ||
                TypeConverter.ToString(distinguishedName).IsNullOrWhiteSpace())
                throw new JavaScriptException(Engine, "Error", "Distinguished name must be specified.");
            
            var user = ADHelper.GetADUserByDistinguishedName(TypeConverter.ToString(distinguishedName), LdapPath);

            return user == null
              ? null
              : new ADUserInstance(Engine.Object.InstancePrototype, user, LdapPath);
        }

        [JSFunction(Name = "getADGroup")]
        [JSDoc("Returns an object representating the specified group.")]
        public ADGroupInstance GetADGroup(string groupName)
        {
            var group = ADHelper.GetADGroup(groupName, LdapPath);

            return group == null
                ? null
                : new ADGroupInstance(Engine.Object.InstancePrototype, group, LdapPath);
        }

        [JSFunction(Name = "getADGroupByDistinguishedName")]
        [JSDoc("Returns an object representating the specified group.")]
        public object GetADGroupByDistinguishedName(object distinguishedName)
        {
            if (distinguishedName == null || distinguishedName == Undefined.Value || distinguishedName == Null.Value ||
                TypeConverter.ToString(distinguishedName).IsNullOrWhiteSpace())
                throw new JavaScriptException(Engine, "Error", "Distinguished name must be specified.");

            var @group = ADHelper.GetADGroupByDistinguishedName(TypeConverter.ToString(distinguishedName), LdapPath);

            return group == null
              ? null
              : new ADGroupInstance(Engine.Object.InstancePrototype, group, LdapPath);
        }

        [JSFunction(Name = "searchAllDirectoryEntries")]
        [JSDoc("Searches all directory entries for the specified search text, optionally indicating a maximium number of results and to limit to the specified principal type.")]
        public ArrayInstance SearchAllDirectoryEntities(string searchText, int maxResults, string principalType)
        {
            var principalTypeEnum = PrincipalType.All;
            if (String.IsNullOrEmpty(principalType) == false)
                principalTypeEnum = (PrincipalType)Enum.Parse(typeof(PrincipalType), principalType);

            var entities = ADHelper.SearchAllDirectoryEntities(searchText, maxResults, principalTypeEnum, LdapPath);

            var result = Engine.Array.Construct();

            foreach (var entity in entities)
            {
                if (entity is ADGroup)
                {
                    ArrayInstance.Push(result, new ADGroupInstance(Engine.Object.InstancePrototype, entity as ADGroup, LdapPath));
                }
                else if (entity is ADUser)
                {
                    ArrayInstance.Push(result, new ADUserInstance(Engine.Object.InstancePrototype, entity as ADUser, LdapPath));
                }
            }

            return result;
        }

        [JSFunction(Name = "searchAllGroups")]
        [JSDoc("Searches all groups for the specified search text, optionally indicating a maximium number of results.")]
        [JSDoc("ternReturnType", "[+ADGroup]")]
        public ArrayInstance SearchAllGroups(string searchText, int maxResults)
        {
            var groups = ADHelper.SearchAllGroups(searchText, maxResults, LdapPath);

            var result = Engine.Array.Construct();
            foreach (var group in groups)
            {
                ArrayInstance.Push(result, new ADGroupInstance(Engine.Object.InstancePrototype, group, LdapPath));
            }
            return result;
        }

        [JSFunction(Name = "searchAllUsers")]
        [JSDoc("Searches all users for the specified search text contained within a user's firstname, lastname, displayname, email or logon name. Optionally indicating a maximium number of results.")]
        [JSDoc("ternReturnType", "[+ADUser]")]
        public ArrayInstance SearchAllUsers(string searchText, int maxResults)
        {
            var users = ADHelper.SearchAllUsers(searchText, maxResults, LdapPath);

            var result = Engine.Array.Construct();
            foreach (var user in users)
            {
                ArrayInstance.Push(result, new ADUserInstance(Engine.Object.InstancePrototype, user, LdapPath));
            }
            return result;
        }

        [JSFunction(Name = "searchAllUsersByLogonAndEmail")]
        [JSDoc("Searches all users for the specified search text contained within a user's email or logon. Optionally indicating a maximium number of results.")]
        [JSDoc("ternReturnTyep", "[+ADUser]")]
        public ArrayInstance SearchAllUsersByLogonAndEmail(string searchText, int maxResults)
        {
            var users = ADHelper.SearchAllUsersByLogonAndEmail(searchText, maxResults, LdapPath);

            var result = Engine.Array.Construct();
            foreach (var user in users)
            {
                ArrayInstance.Push(result, new ADUserInstance(Engine.Object.InstancePrototype, user, LdapPath));
            }
            return result;
        }

    }
}
