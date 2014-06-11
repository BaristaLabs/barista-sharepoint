namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPListItemCollectionConstructor : ClrFunction
    {
        public SPListItemCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPListItemCollection", new SPListItemCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPListItemCollectionInstance Construct()
        {
            return new SPListItemCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPListItemCollectionInstance : ObjectInstance
    {
        private readonly SPListItemCollection m_listItemCollection;

        public SPListItemCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPListItemCollectionInstance(ObjectInstance prototype, SPListItemCollection listItemCollection)
            : this(prototype)
        {
            if (listItemCollection == null)
                throw new ArgumentNullException("listItemCollection");

            m_listItemCollection = listItemCollection;
        }

        public SPListItemCollection SPListItemCollection
        {
            get { return m_listItemCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_listItemCollection.Count;
            }
        }

        [JSProperty(Name = "fields")]
        public SPFieldCollectionInstance Fields
        {
            get
            {
                if (m_listItemCollection.Fields == null)
                    return null;

                return new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_listItemCollection.Fields);
            }
        }

        [JSProperty(Name ="listItemCollectionPosition")]
        public SPListItemCollectionPositionInstance ListItemCollectionPosition
        {
            get
            {
                if (m_listItemCollection.ListItemCollectionPosition == null)
                    return null;

                return new SPListItemCollectionPositionInstance(this.Engine.Object.InstancePrototype, m_listItemCollection.ListItemCollectionPosition);
            }
        }

        [JSProperty(Name = "numberOfFields")]
        public int NumberOfFields
        {
            get
            {
                return m_listItemCollection.NumberOfFields;
            }
        }

        [JSProperty(Name = "queryFieldNames")]
        public ArrayInstance QueryFieldNames
        {
            get
            {
                var result = this.Engine.Array.Construct(m_listItemCollection.QueryFieldNames.OfType<string>().ToArray());
                return result;
            }
        }

        //SourceQuery
        [JSFunction(Name = "add")]
        public SPListItemInstance Add()
        {
            var result = m_listItemCollection.Add();
            if (result == null)
                return null;

            return new SPListItemInstance(this.Engine, result);
        }

        [JSFunction(Name = "add2")]
        public SPListItemInstance Add2(string folderUrl, string underlyingObjectType, object leafName)
        {
            SPFileSystemObjectType type;
            if (underlyingObjectType.TryParseEnum(true, out type))
            {
                SPListItem result;
                if (leafName == null || leafName == Null.Value || leafName == Undefined.Value)
                    result = m_listItemCollection.Add(folderUrl, type);
                else
                    result = m_listItemCollection.Add(folderUrl, type, TypeConverter.ToString(leafName));

                if (result == null)
                    return null;

                return new SPListItemInstance(this.Engine, result);
            }
            return null;
        }

        [JSFunction(Name = "beginLoadData")]
        public void BeginLoadData()
        {
            m_listItemCollection.BeginLoadData();
        }

        [JSFunction(Name = "delete")]
        public void Delete(int index)
        {
            m_listItemCollection.Delete(index);
        }

        [JSFunction(Name = "deleteItemById")]
        public void DeleteItemById(int id)
        {
            m_listItemCollection.DeleteItemById(id);
        }

        [JSFunction(Name = "endLoadData")]
        public void EndLoadData()
        {
            m_listItemCollection.EndLoadData();
        }

        //GetDataTable

        [JSFunction(Name = "getItemByGuid")]
        public SPListItemInstance GetItemByGuid(object guid)
        {
            var iGuid = GuidInstance.ConvertFromJsObjectToGuid(guid);
            var listItem = m_listItemCollection[iGuid];

            return listItem == null
                ? null
                : new SPListItemInstance(this.Engine, listItem);
        }

        [JSFunction(Name = "getItemByIndex")]
        public SPListItemInstance GetItemByIndex(int index)
        {
            var listItem = m_listItemCollection[index];

            return listItem == null
                ? null
                : new SPListItemInstance(this.Engine, listItem);
        }

        [JSFunction(Name = "getItemById")]
        public SPListItemInstance GetItemById(int id)
        {
            var listItem = m_listItemCollection.GetItemById(id);

            return listItem == null
                ? null
                : new SPListItemInstance(this.Engine, listItem);
        }

        [JSFunction(Name = "getList")]
        public SPListInstance GetList()
        {
            if (m_listItemCollection.List == null)
                return null;

            return new SPListInstance(this.Engine, this.m_listItemCollection.List.ParentWeb.Site, this.m_listItemCollection.List.ParentWeb, this.m_listItemCollection.List);
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_listItemCollection.Xml;
        }

        [JSFunction(Name = "getXmlDataSchema")]
        public string GetXmlDataSchema()
        {
            return m_listItemCollection.XmlDataSchema;
        }

        //ReorderItems

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (SPListItem listItem in m_listItemCollection)
                ArrayInstance.Push(result, new SPListItemInstance(this.Engine, listItem));

            return result;
        }
    }
}
