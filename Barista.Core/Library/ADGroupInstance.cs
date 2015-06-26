namespace Barista.Library
{
    using System.Linq;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using Barista.DirectoryServices;

    [Serializable]
    public class ADGroupConstructor : ClrFunction
    {
        public ADGroupConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "ADGroup", new ADGroupInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public ADGroupInstance Construct()
        {
            var group = new ADGroup();

            return new ADGroupInstance(InstancePrototype, group);
        }

        public ADGroupInstance Construct(ADGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            return new ADGroupInstance(InstancePrototype, group);
        }
    }

    [Serializable]
    public class ADGroupInstance : ObjectInstance
    {
        private readonly ADGroup m_group;
        private readonly string m_ldap;

        public ADGroupInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFunctions();
        }

        public ADGroupInstance(ObjectInstance prototype, ADGroup group)
            : this(prototype)
        {
            m_group = group;
        }

        public ADGroupInstance(ObjectInstance prototype, ADGroup group, string ldap)
            : this(prototype, group)
        {
            m_ldap = ldap;
        }

        #region Properties
        [JSProperty(Name = "rawSid")]
        // ReSharper disable InconsistentNaming
        public object RawsID
        // ReSharper restore InconsistentNaming
        {
            get { return StringHelper.ByteArrayToString((byte[])m_group.RawsID); }
        }

        [JSProperty(Name = "sId")]
        // ReSharper disable InconsistentNaming
        public object sID
        // ReSharper restore InconsistentNaming
        {
            get { return m_group.sID; }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get { return m_group.Name; }
        }

        [JSProperty(Name = "displayName")]
        public string DisplayName
        {
            get { return m_group.DisplayName; }
        }

        [JSProperty(Name = "members")]
        [JSDoc("ternPropertyType", "[+ADUser]")]
        public ArrayInstance Members
        {
            get
            {
                var result = Engine.Array.Construct();

                foreach (var user in m_group.Members)
                {
                    ArrayInstance.Push(result, user);
                }

                return result;
            }
        }

        #endregion

        [JSFunction(Name = "expandUsers")]
        [JSDoc("ternReturnTYpe", "[+ADUser]")]
        public ArrayInstance ExpandUsers()
        {
            var result = Engine.Array.Construct();

            foreach (var user in m_group.Members.Select(memberLogonName => ADHelper.GetADUserByDistinguishedName(memberLogonName, m_ldap)))
            {
                if (user == null)
                    continue;

                ArrayInstance.Push(result, new ADUserInstance(Engine.Object.InstancePrototype, user));
            }

            return result;
        }

        [JSFunction(Name = "expandGroups")]
        [JSDoc("ternReturnType", "[+ADGroup]")]
        public ArrayInstance ExpandGroups()
        {
            var result = Engine.Array.Construct();

            foreach (var group in m_group.Members.Select(memberLogonName => ADHelper.GetADGroupByDistinguishedName(memberLogonName, m_ldap)))
            {
                if (group == null)
                    continue;

                ArrayInstance.Push(result, new ADGroupInstance(Engine.Object.InstancePrototype, group));
            }

            return result;
        }
    }
}
