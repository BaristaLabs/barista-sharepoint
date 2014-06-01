namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPFolderCollectionConstructor : ClrFunction
    {
        public SPFolderCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPFolderCollection", new SPFolderCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPFolderCollectionInstance Construct()
        {
            return new SPFolderCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPFolderCollectionInstance : ObjectInstance
    {
        private readonly SPFolderCollection m_folderCollection;

        public SPFolderCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPFolderCollectionInstance(ObjectInstance prototype, SPFolderCollection folderCollection)
            : this(prototype)
        {
            if (folderCollection == null)
                throw new ArgumentNullException("folderCollection");

            m_folderCollection = folderCollection;
        }

        public SPFolderCollection SPFolderCollection
        {
            get { return m_folderCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_folderCollection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public SPFolderInstance Add(string strUrl)
        {
            var result = m_folderCollection.Add(strUrl);
            if (result == null)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, result.ParentWeb.Site, result.ParentWeb,
                result);
        }

        [JSFunction(Name = "delete")]
        public void Delete(string strUrl)
        {
            m_folderCollection.Delete(strUrl);
        }

        [JSFunction(Name = "getFolderByIndex")]
        public SPFolderInstance GetFolderByIndex(int index)
        {
            var result = m_folderCollection[index];
            if (result == null)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, result.ParentWeb.Site, result.ParentWeb,
                result);
        }

        [JSFunction(Name = "getFolderByUrl")]
        public SPFolderInstance GetFolderByUrl(string urlOfFolder)
        {
            var result = m_folderCollection[urlOfFolder];
            if (result == null)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, result.ParentWeb.Site, result.ParentWeb,
                result);
        }

        [JSFunction(Name = "getParentFolder")]
        public SPFolderInstance GetParenFolder()
        {
            var result = m_folderCollection.Folder;
            if (result == null)
                return null;

            return new SPFolderInstance(this.Engine.Object.InstancePrototype, result.ParentWeb.Site, result.ParentWeb,
                result);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            var result = m_folderCollection.Web;
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (SPFolder folder in this.m_folderCollection)
                ArrayInstance.Push(result, new SPFolderInstance(this.Engine.Object.InstancePrototype, folder.ParentWeb.Site, folder.ParentWeb, folder));
            return result;
        }
    }
}
