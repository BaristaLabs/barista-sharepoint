namespace Barista.SharePoint.Library
{
    using System.Text.RegularExpressions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPListItemVersionConstructor : ClrFunction
    {
        public SPListItemVersionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPListItemVersion", new SPListItemVersionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPListItemVersionInstance Construct()
        {
            return new SPListItemVersionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPListItemVersionInstance : ObjectInstance
    {
        private static readonly Regex DigitRegex = new Regex(@"^\d+;#.*$", RegexOptions.Compiled);
        private readonly SPListItemVersion m_listItemVersion;

        public SPListItemVersionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPListItemVersionInstance(ObjectInstance prototype, SPListItemVersion listItemVersion)
            : this(prototype)
        {
            if (listItemVersion == null)
                throw new ArgumentNullException("listItemVersion");

            m_listItemVersion = listItemVersion;
        }

        public SPListItemVersion SPListItemVersion
        {
            get { return m_listItemVersion; }
        }

        [JSProperty(Name = "created")]
        public DateInstance Created
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_listItemVersion.Created);
            }
        }

        [JSProperty(Name = "createdBy")]
        public SPUserInstance CreatedBy
        {
            get
            {
                var user = m_listItemVersion.CreatedBy.User;
                if (user == null)
                    return null;

                return new SPUserInstance(this.Engine.Object.InstancePrototype, user);
            }
        }

        [JSProperty(Name = "fields")]
        public SPFieldCollectionInstance Fields
        {
            get
            {
                if (m_listItemVersion.Fields == null)
                    return null;

                return new SPFieldCollectionInstance(this.Engine.Object.InstancePrototype, m_listItemVersion.Fields);
            }
        }

        [JSProperty(Name = "isCurrentVersion")]
        public bool IsCurrentVersion
        {
            get
            {
                return m_listItemVersion.IsCurrentVersion;
            }
        }

        [JSProperty(Name = "level")]
        public string Level
        {
            get
            {
                return m_listItemVersion.Level.ToString();
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_listItemVersion.Url;
            }
        }

        [JSProperty(Name = "versionId")]
        public int VersionId
        {
            get
            {
                return m_listItemVersion.VersionId;
            }
        }

        [JSProperty(Name = "versionLabel")]
        public string VersionLabel
        {
            get
            {
                return m_listItemVersion.VersionLabel;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_listItemVersion.Delete();
        }

        [JSFunction(Name = "getListItem")]
        public SPListItemInstance GetListItem()
        {
            if (m_listItemVersion.ListItem == null)
                return null;

            return new SPListItemInstance(this.Engine, m_listItemVersion.ListItem);
        }

        [JSFunction(Name = "getFieldValueByFieldName")]
        public object GetFieldValueByIndex(int index)
        {
            var field = m_listItemVersion.Fields[index];
            return field != null
                ? GetListItemVersionObject(this.Engine, m_listItemVersion, field)
                : null;
        }

        [JSFunction(Name = "getFieldValueByFieldName")]
        public object GetFieldValueByFieldName(string fieldName)
        {
            var field = m_listItemVersion.Fields[fieldName];
            return field != null
                ? GetListItemVersionObject(this.Engine, m_listItemVersion, field)
                : null;
        }

        [JSFunction(Name = "recycle")]
        public void Recycle()
        {
            m_listItemVersion.Recycle();
        }
        #region Static Functions

        public static object GetListItemVersionObject(ScriptEngine engine, SPListItemVersion version, SPField field)
        {
            switch (field.Type)
            {
                case SPFieldType.Integer:
                    {
                        int value;
                        if (version.TryGetSPFieldValue(field.InternalName, out value))
                            return value;
                    }
                    break;
                case SPFieldType.Boolean:
                    {
                        bool value;
                        if (version.TryGetSPFieldValue(field.InternalName, out value))
                            return value;
                    }
                    break;
                case SPFieldType.Number:
                    {
                        double value;
                        if (version.TryGetSPFieldValue(field.InternalName, out value))
                            return value;
                    }
                    break;
                case SPFieldType.DateTime:
                    {
                        DateTime value;
                        if (version.TryGetSPFieldValue(field.InternalName, out value))
                        {
                            return JurassicHelper.ToDateInstance(engine, new DateTime(value.Ticks, DateTimeKind.Local));
                        }
                    }
                    break;
                case SPFieldType.URL:
                    {
                        string urlFieldValue;
                        if (version.TryGetSPFieldValue(field.InternalName, out urlFieldValue))
                        {
                            var urlValue = new SPFieldUrlValue(urlFieldValue);

                            var item = engine.Object.Construct();
                            item.SetPropertyValue("description", urlValue.Description, false);
                            item.SetPropertyValue("url", urlValue.Url, false);

                            return item;
                        }
                    }
                    break;
                case SPFieldType.User:
                    {
                        string userToken;
                        if (version.TryGetSPFieldValue(field.InternalName, out userToken))
                        {
                            var fieldUserValue = new SPFieldUserValue(version.ListItem.Web, userToken);
                            var userInstance = new SPUserInstance(engine.Object.InstancePrototype, fieldUserValue.User);
                            return userInstance;
                        }
                    }
                    break;
                case SPFieldType.Lookup:
                    {
                        var fieldType = field as SPFieldLookup;
                        if (fieldType == null)
                            return null;

                        if (fieldType.AllowMultipleValues)
                        {
                            object fv;
                            if (!version.TryGetSPFieldValue(field.InternalName, out fv))
                                return null;

                            var fieldValue = fv as SPFieldLookupValueCollection;

                            var array = engine.Array.Construct();

                            if (fieldValue != null)
                            {
                                foreach (var lookupValue in fieldValue)
                                {
                                    var item = engine.Object.Construct();
                                    item.SetPropertyValue("lookupId", lookupValue.LookupId, false);
                                    item.SetPropertyValue("lookupValue", lookupValue.LookupValue, false);

                                    ArrayInstance.Push(array, item);
                                }
                            }

                            return array;
                        }
                        else
                        {
                            object fieldValue;
                            if (!version.TryGetSPFieldValue(field.InternalName, out fieldValue))
                                return null;

                            if (fieldValue is SPFieldUrlValue)
                            {
                                var urlValue = fieldValue as SPFieldUrlValue;
                                var item = engine.Object.Construct();
                                item.SetPropertyValue("description", urlValue.Description, false);
                                item.SetPropertyValue("url", urlValue.Url, false);

                                return item;
                            }

                            if (fieldValue is DateTime)
                            {
                                var value = (DateTime)fieldValue;
                                return JurassicHelper.ToDateInstance(engine,
                                    new DateTime(value.Ticks, DateTimeKind.Local));
                            }
                            
                            if (fieldValue is SPFieldUserValue)
                            {
                                var fieldUserValue = (SPFieldUserValue)fieldValue;
                                var userInstance = new SPUserInstance(engine.Object.InstancePrototype, fieldUserValue.User);
                                return userInstance;
                            }
                            
                            if (fieldValue is Guid)
                            {
                                var guidValue = (Guid)fieldValue;
                                var guidInstance = new GuidInstance(engine.Object.InstancePrototype, guidValue);
                                return guidInstance;
                            }
                            
                            if (fieldValue is string)
                            {
                                //Attempt to create a new SPFieldLookupValue from the string
                                if (!DigitRegex.IsMatch((string) fieldValue, 0))
                                    return fieldValue;

                                try
                                {
                                    var lookupValue = new SPFieldLookupValue((string)fieldValue);

                                    var item = engine.Object.Construct();
                                    item.SetPropertyValue("lookupId", lookupValue.LookupId, false);
                                    item.SetPropertyValue("lookupValue", lookupValue.LookupValue, false);
                                    return item;
                                }
                                catch (ArgumentException)
                                {
                                    return fieldValue;
                                }
                            }

                            return fieldValue;
                        }
                    }
                default:
                    {
                        object value;
                        if (version.TryGetSPFieldValue(field.InternalName, out value))
                        {
                            var stringValue = field.GetFieldValueAsText(value);

                            return stringValue;
                        }
                    }
                    break;
            }

            return null;
        }

        #endregion
    }
}
