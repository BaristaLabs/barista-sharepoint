namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPListItemVersionCollectionConstructor : ClrFunction
    {
        public SPListItemVersionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPListItemVersionCollection", new SPListItemVersionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPListItemVersionCollectionInstance Construct()
        {
            return new SPListItemVersionCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPListItemVersionCollectionInstance : ObjectInstance
    {
        private readonly SPListItemVersionCollection m_listItemVersionCollection;

        public SPListItemVersionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPListItemVersionCollectionInstance(ObjectInstance prototype, SPListItemVersionCollection listItemVersionCollection)
            : this(prototype)
        {
            if (listItemVersionCollection == null)
                throw new ArgumentNullException("listItemVersionCollection");

            m_listItemVersionCollection = listItemVersionCollection;
        }

        public SPListItemVersionCollection SPListItemVersionCollection
        {
            get { return m_listItemVersionCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_listItemVersionCollection.Count;
            }
        }

        [JSFunction(Name = "deleteAll")]
        public void DeleteAll()
        {
            m_listItemVersionCollection.DeleteAll();
        }

        [JSFunction(Name = "getListItem")]
        public SPListItemInstance GetListItem()
        {
            if (m_listItemVersionCollection.ListItem == null)
                return null;

            return new SPListItemInstance(this.Engine, m_listItemVersionCollection.ListItem);
        }

        [JSFunction(Name = "getVersionFromId")]
        public SPListItemVersionInstance GetVersionFromId(int versionId)
        {
            var result = m_listItemVersionCollection.GetVersionFromID(versionId);
            if (result == null)
                return null;

            return new SPListItemVersionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getVersionFromLabel")]
        public SPListItemVersionInstance GetVersionFromLabel(string versionLabel)
        {
            var result = m_listItemVersionCollection.GetVersionFromLabel(versionLabel);
            if (result == null)
                return null;

            return new SPListItemVersionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getVersionFromIndex")]
        public SPListItemVersionInstance GetVersionFromIndex(int index)
        {
            var result = m_listItemVersionCollection[index];
            if (result == null)
                return null;

            return new SPListItemVersionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            if (m_listItemVersionCollection.Web == null)
                return null;

            return new SPWebInstance(this.Engine, m_listItemVersionCollection.Web);
        }

        [JSFunction(Name = "recycleAll")]
        public void RecycleAll()
        {
            m_listItemVersionCollection.RecycleAll();
        }

        [JSFunction(Name = "restore")]
        public void Restore(int index)
        {
            m_listItemVersionCollection.Restore(index);
        }

        [JSFunction(Name = "restoreById")]
        public void RestoreById(int id)
        {
            m_listItemVersionCollection.RestoreByID(id);
        }

        [JSFunction(Name = "restoreByLabel")]
        public void RestoreByLabel(string versionLabel)
        {
            m_listItemVersionCollection.RestoreByLabel(versionLabel);
        }
    }
}
