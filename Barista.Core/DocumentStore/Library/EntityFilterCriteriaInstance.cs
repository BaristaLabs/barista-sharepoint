namespace Barista.Library
{
    using Barista.DocumentStore;
    using Barista.Extensions;
    using Jurassic;
    using Jurassic.Library;
    using Barista.Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class EntityFilterCriteriaInstance : ObjectInstance
    {
        public EntityFilterCriteriaInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.EntityFilterCriteria = new EntityFilterCriteria();

            this.PopulateFunctions();
        }

        public EntityFilterCriteria EntityFilterCriteria
        {
            get;
            set;
        }

        [JSProperty(Name = "path")]
        [JsonProperty("path")]
        public string Path
        {
            get { return EntityFilterCriteria.Path; }
            set { EntityFilterCriteria.Path = value; }
        }

        [JSProperty(Name = "includeData")]
        [JsonProperty("includeData")]
        public bool IncludeData
        {
            get { return EntityFilterCriteria.IncludeData; }
            set { EntityFilterCriteria.IncludeData = value; }
        }

        [JSProperty(Name = "namespace")]
        [JsonProperty("namespace")]
        public string Namespace
        {
            get { return EntityFilterCriteria.Namespace; }
            set { EntityFilterCriteria.Namespace = value; }
        }

        [JSProperty(Name = "namespaceMatchType")]
        [JsonProperty("namespaceMatchType")]
        public string NamespaceMatchType
        {
            get { return EntityFilterCriteria.NamespaceMatchType.ToString(); }
            set
            {
                NamespaceMatchType enumValue;
                if (value.TryParseEnum(true, out enumValue))
                    EntityFilterCriteria.NamespaceMatchType = enumValue;
                else
                    throw new JavaScriptException(this.Engine, "Error", "Unknown or invalid namespace match type: " + value);
            }
        }

        [JSProperty(Name = "queryPairs")]
        [JsonProperty("queryPairs")]
        public object QueryPairs
        {
            get
            {
                var pairs = this.Engine.Object.Construct();
                foreach (var key in EntityFilterCriteria.QueryPairs.Keys)
                {
                    pairs.SetPropertyValue(key, EntityFilterCriteria.QueryPairs[key], false);
                }

                return pairs;
            }
            set
            {
                var jsObject = value as ObjectInstance;
                if (value == Undefined.Value || value == Null.Value || value == null || jsObject == null)
                {
                    EntityFilterCriteria.QueryPairs = null;
                    return;
                }
                
                var pairs = new Dictionary<string, string>();
                foreach (var property in jsObject.Properties)
                {
                    if (!pairs.ContainsKey(property.Name))
                        pairs.Add(property.Name, TypeConverter.ToString(property.Value));
                }

                EntityFilterCriteria.QueryPairs = pairs;
            }
        }

        [JSProperty(Name = "skip")]
        [JsonProperty("skip")]
        public object Skip
        {
            get
            {
                if (EntityFilterCriteria.Skip.HasValue)
                    return EntityFilterCriteria.Skip.Value;

                return Null.Value;
            }
            set
            {
                if (value != Undefined.Value && value != Null.Value && value != null)
                    EntityFilterCriteria.Skip = TypeConverter.ToUint32(value);
                else
                    EntityFilterCriteria.Skip = null;
            }
        }

        [JSProperty(Name = "top")]
        [JsonProperty("top")]
        public object Top
        {
            get
            {
                if (EntityFilterCriteria.Top.HasValue)
                    return EntityFilterCriteria.Top.Value;

                return Null.Value;
            }
            set
            {
                if (value != Undefined.Value && value != Null.Value && value != null)
                    EntityFilterCriteria.Top = TypeConverter.ToUint32(value);
                else
                    EntityFilterCriteria.Top = null;
            }
        }
    }
}
