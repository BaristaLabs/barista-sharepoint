namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;
    using System.Linq;

    [Serializable]
    public class SPUserCollectionConstructor : ClrFunction
    {
        public SPUserCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserCollection", new SPUserCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPUserCollectionInstance Construct()
        {
            return new SPUserCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPUserCollectionInstance : ObjectInstance
    {
        private readonly SPUserCollection m_userCollection;

        public SPUserCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserCollectionInstance(ObjectInstance prototype, SPUserCollection userCollection)
            : this(prototype)
        {
            if (userCollection == null)
                throw new ArgumentNullException("userCollection");

            m_userCollection = userCollection;
        }

        public SPUserCollection SPUserCollection
        {
            get { return m_userCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get { return m_userCollection.Count; }
        }

        [JSFunction(Name = "add")]
        public void Add(string loginName, string email, string name, string notes)
        {
            m_userCollection.Add(loginName, email, name, notes);
        }

        [JSFunction(Name = "getUserByEmail")]
        public SPUserInstance GetUserByEmail(string emailAddress)
        {
            var result = m_userCollection.GetByEmail(emailAddress);
            return result == null
              ? null
              : new SPUserInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getUserById")]
        public SPUserInstance GetUserById(int id)
        {
            var result = m_userCollection.GetByID(id);
            return result == null
              ? null
              : new SPUserInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getUserByIndex")]
        public SPUserInstance GetUserByIndex(int index)
        {
            var result = m_userCollection[index];
            return result == null
              ? null
              : new SPUserInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getUserByLogonName")]
        public SPUserInstance GetUserByLogonName(string loginName)
        {
            var result = m_userCollection[loginName];
            return result == null
              ? null
              : new SPUserInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getUsersByLogonName")]
        public SPUserCollectionInstance GetUsersByLoginName(ArrayInstance loginNames)
        {
            if (loginNames == null || loginNames.Length == 0)
                return null;

            var strLoginNames = loginNames.ElementValues.Select(TypeConverter.ToString).ToList();
            var users = m_userCollection.GetCollection(strLoginNames.ToArray());
            return users == null
                ? null
                : new SPUserCollectionInstance(this.Engine.Object.InstancePrototype, users);
        }

        [JSFunction(Name = "remove")]
        public void Remove(object user)
        {
            if (user == Null.Value || user == Undefined.Value || user == null)
                throw new ArgumentNullException("user", @"A user to remove must be specified either by id or login name.");

            if (TypeUtilities.IsNumeric(user))
                m_userCollection.Remove(TypeConverter.ToInteger(user));
            else
                m_userCollection.Remove(TypeConverter.ToString(user));
        }

        [JSFunction(Name = "removeById")]
        public void RemoveById(ArrayInstance loginNames)
        {
            if (loginNames == null || loginNames.Length == 0)
                return;

            var strLoginNames = loginNames.ElementValues.Select(TypeConverter.ToString).ToList();
            m_userCollection.RemoveCollection(strLoginNames.ToArray());
        }

        [JSFunction(Name = "removeUsersByLoginName")]
        public void RemoveById(int id)
        {
            m_userCollection.RemoveByID(id);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();

            foreach (var user in m_userCollection.OfType<SPUser>())
            {
                ArrayInstance.Push(result, new SPUserInstance(this.Engine.Object.InstancePrototype, user));
            }
            return result;
        }


        //WHAAAAAAT?!
        [JSFunction(Name = "getSchemaXmlEx")]
        public string GetSchemaXmlEx()
        {
            return m_userCollection.SchemaXmlEx;
        }

        [JSFunction(Name = "getViewSchemaXmlEx")]
        public string GetViewSchemaXmlEx()
        {
            return m_userCollection.ViewSchemaXmlEx;
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_userCollection.Xml;
        }

        [JSFunction(Name = "getXmlEx")]
        public string GetXmlEx()
        {
            return m_userCollection.XmlEx;
        }

    }
}
