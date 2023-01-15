namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using System;
    using System.IO;

    [Serializable]
    public class SPSiteCollectionConstructor : ClrFunction
    {
        public SPSiteCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPSiteCollection", new SPSiteCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPSiteCollectionInstance Construct()
        {
            return new SPSiteCollectionInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPSiteCollectionInstance : ObjectInstance
    {
        private readonly SPSiteCollection m_siteCollection;

        public SPSiteCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPSiteCollectionInstance(ObjectInstance prototype, SPSiteCollection siteCollection)
            : this(prototype)
        {
            if (siteCollection == null)
                throw new ArgumentNullException("siteCollection");

            m_siteCollection = siteCollection;
        }

        public SPSiteCollection SPSiteCollection
        {
            get { return m_siteCollection; }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_siteCollection.Count;
            }
        }

        [JSProperty(Name = "names")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Names
        {
            get
            {
                object[] names = m_siteCollection.Names;
                return Engine.Array.Construct(names);
            }
        }

        //TODO: More Add Overloads...

        [JSFunction(Name = "add")]
        public SPSiteInstance Add(string siteUrl, string ownerLogin, string ownerEmail)
        {
            var result = m_siteCollection.Add(siteUrl, ownerLogin, ownerEmail);
            return result == null
              ? null
              : new SPSiteInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "backup")]
        public void Backup(string siteUrl, string fileName, bool overwrite)
        {
            m_siteCollection.Backup(siteUrl, fileName, overwrite);
        }

        [JSFunction(Name = "delete")]
        public void Delete(string siteUrl, object deleteADAccounts, object gradualDelete)
        {
            if (deleteADAccounts == null || deleteADAccounts == Null.Value || deleteADAccounts == Undefined.Value)
                m_siteCollection.Delete(siteUrl);
            else if (gradualDelete == null || gradualDelete == Null.Value || gradualDelete == Undefined.Value)
                m_siteCollection.Delete(siteUrl, TypeConverter.ToBoolean(deleteADAccounts));
            else
                m_siteCollection.Delete(siteUrl, TypeConverter.ToBoolean(deleteADAccounts), TypeConverter.ToBoolean(gradualDelete));
        }

        [JSFunction(Name = "restore")]
        public void Restore(string siteUrl, string fileName, bool overwrite, object hostHeaderAsSiteName)
        {
            if (hostHeaderAsSiteName == null || hostHeaderAsSiteName == Null.Value || hostHeaderAsSiteName == Undefined.Value)
                m_siteCollection.Restore(siteUrl, fileName, overwrite);
            else
                m_siteCollection.Restore(siteUrl, fileName, overwrite, TypeConverter.ToBoolean(hostHeaderAsSiteName));
        }

        [JSFunction(Name = "getSiteByName")]
        public SPSiteInstance GetSiteByName(string name)
        {
            var site = m_siteCollection[name];
            return site == null
              ? null
              : new SPSiteInstance(Engine.Object, site);
        }

        [JSFunction(Name = "getSiteByIndex")]
        public SPSiteInstance GetSiteByIndex(int index)
        {
            var site = m_siteCollection[index];
            return site == null
              ? null
              : new SPSiteInstance(Engine.Object, site);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+SPSite]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (SPSite site in m_siteCollection)
            {
                try
                {
                    //Test to see if the site is "good";
                    var allow = site.AllowDesigner;
                    ArrayInstance.Push(result, new SPSiteInstance(Engine.Object.InstancePrototype, site));
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
            return result;
        }
    }
}
