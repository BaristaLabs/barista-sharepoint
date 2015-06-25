namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserCustomActionCollectionConstructor : ClrFunction
    {
        public SPUserCustomActionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserCustomActionCollection", new SPUserCustomActionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPUserCustomActionCollectionInstance Construct()
        {
            return new SPUserCustomActionCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPUserCustomActionCollectionInstance : ObjectInstance
    {
        private readonly SPUserCustomActionCollection m_userCustomActionCollection;

        public SPUserCustomActionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserCustomActionCollectionInstance(ObjectInstance prototype, SPUserCustomActionCollection userCustomActionCollection)
            : this(prototype)
        {
            if (userCustomActionCollection == null)
                throw new ArgumentNullException("userCustomActionCollection");

            m_userCustomActionCollection = userCustomActionCollection;
        }

        public SPUserCustomActionCollection SPUserCustomActionCollection
        {
            get
            {
                return m_userCustomActionCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_userCustomActionCollection.Count;
            }
        }

        [JSProperty(Name = "scope")]
        public string Scope
        {
            get
            {
                return m_userCustomActionCollection.Scope.ToString();
            }
        }

        [JSFunction(Name = "add")]
        public SPUserCustomActionInstance Add()
        {
            var result = m_userCustomActionCollection.Add();
            return result == null
                ? null
                : new SPUserCustomActionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "clear")]
        public void Clear()
        {
            m_userCustomActionCollection.Clear();
        }

        [JSFunction(Name = "getUserCustomActionById")]
        public SPUserCustomActionInstance GetUserCustomActionById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_userCustomActionCollection[guidId];
            return result == null
                ? null
                : new SPUserCustomActionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var action in m_userCustomActionCollection
                .Select(a => new SPUserCustomActionInstance(this.Engine.Object.InstancePrototype, a)))
            {
                ArrayInstance.Push(result, action);
            }

            return result;
        }
    }
}
