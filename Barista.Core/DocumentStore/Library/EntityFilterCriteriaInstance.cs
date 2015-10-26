﻿namespace Barista.Library
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
        public IDictionary<string, string> QueryPairs
        {
            get { return EntityFilterCriteria.QueryPairs; }
            set { EntityFilterCriteria.QueryPairs = value; }
        }

        [JSProperty(Name = "skip")]
        [JsonProperty("skip")]
        public int? Skip
        {
            get
            {
                if (EntityFilterCriteria.Skip != null)
                    return (int)EntityFilterCriteria.Skip;
                return null;
            }
            set
            {
                if (value != null)
                    EntityFilterCriteria.Skip = (uint)value;
                else
                    EntityFilterCriteria.Skip = null;
            }
        }

        [JSProperty(Name = "top")]
        [JsonProperty("top")]
        public int? Top
        {
            get
            {
                if (EntityFilterCriteria.Top != null)
                    return (int)EntityFilterCriteria.Top;
                return null;
            }
            set
            {
                if (value != null)
                    EntityFilterCriteria.Top = (uint)value;
                else
                    EntityFilterCriteria.Top = null;
            }
        }
    }
}
