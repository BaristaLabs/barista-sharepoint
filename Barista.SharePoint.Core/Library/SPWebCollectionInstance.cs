namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;

    [Serializable]
    public class SPWebCollectionConstructor : ClrFunction
    {
        public SPWebCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebCollection", new SPWebCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWebCollectionInstance Construct()
        {
            return new SPWebCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWebCollectionInstance : ObjectInstance
    {
        private readonly SPWebCollection m_webCollection;

        public SPWebCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWebCollectionInstance(ObjectInstance prototype, SPWebCollection webCollection)
            : this(prototype)
        {
            if (webCollection == null)
                throw new ArgumentNullException("webCollection");

            m_webCollection = webCollection;
        }

        public SPWebCollection SPWebCollection
        {
            get { return m_webCollection; }
        }

        [JSProperty(Name = "count")]
        public object Count
        {
            get
            {
                try
                {
                    return m_webCollection.Count;
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }

            }
        }

        [JSProperty(Name = "names")]
        public object Names
        {
            get
            {
                try
                {
                    // ReSharper disable CoVariantArrayConversion
                    return m_webCollection.Names == null
                      ? null
                      : this.Engine.Array.Construct(m_webCollection.Names);
                    // ReSharper restore CoVariantArrayConversion
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
        }

        [JSProperty(Name = "websInfo")]
        public object WebsInfo
        {
            get
            {
                try
                {
                    // ReSharper disable CoVariantArrayConversion
                    return m_webCollection.WebsInfo == null
                        ? null
                        : new SPWebInfoListInstance(this.Engine, m_webCollection.WebsInfo);
                    // ReSharper restore CoVariantArrayConversion
                }
                catch (Exception)
                {
                    return Undefined.Value;
                }
            }
        }

        [JSFunction(Name = "add")]
        public SPWebInstance Add(string strWebUrl)
        {
            var result = m_webCollection.Add(strWebUrl);
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "add2")]
        public SPWebInstance Add2(string strWebUrl, string strTitle, string strDescription, double nLcid, object webTemplate, bool useUniquePermissions, bool bConvertIfThere)
        {
            SPWeb result;
            var spWebTemplate = webTemplate as SPWebTemplateInstance;
            if (spWebTemplate != null)
            {
                result = m_webCollection.Add(strWebUrl, strTitle, strDescription, (uint)nLcid, spWebTemplate.WebTemplate, useUniquePermissions, bConvertIfThere);
            }
            else
            {
                var strWebTemplate = TypeConverter.ToString(webTemplate);
                result = m_webCollection.Add(strWebUrl, strTitle, strDescription, (uint)nLcid, strWebTemplate, useUniquePermissions, bConvertIfThere);
            }
            
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name="delete")]
        public void Delete(string strWebUrl)
        {
            m_webCollection.Delete(strWebUrl);
        }

        [JSFunction(Name = "getWebByGuid")]
        public SPWebInstance GetWebByGuid(object id)
        {
            var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

            var result = m_webCollection[guid];

            return result == null
              ? null
              : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "getWebByName")]
        public SPWebInstance GetWebByName(string name)
        {
            var result = m_webCollection[name];
            return result == null
              ? null
              : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "getWebByIndex")]
        public SPWebInstance GetWebByIndex(int index)
        {
            var result = m_webCollection[index];
            return result == null
              ? null
              : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (SPWeb web in m_webCollection)
            {
                ArrayInstance.Push(result, new SPWebInstance(this.Engine, web));
            }
            return result;
        }
    }
}
