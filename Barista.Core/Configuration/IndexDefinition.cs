namespace Barista.Configuration
{
    using System;
    using System.Configuration;
    using Barista.Newtonsoft.Json;

    /// <summary>
    /// Represents an index definition app.config configuration element.
    /// </summary>
    [Serializable]
    public class IndexDefinition : ConfigurationElementBase, IIndexDefinitionConfig
    {
        [JsonProperty("name")]
        [ConfigurationProperty("indexName", IsRequired = true)]
        public string IndexName
        {
            get { return this["indexName"] as string; }
            set { this["indexName"] = value; }
        }

        [JsonProperty("description")]
        [ConfigurationProperty("description", IsRequired = false)]
        public string Description
        {
            get { return this["description"] as string; }
            set { this["description"] = value; }
        }

        [JsonProperty("typeName")]
        [ConfigurationProperty("typeName", IsRequired = true)]
        public string TypeName
        {
            get { return this["typeName"] as string; }
            set { this["typeName"] = value; }
        }

        [JsonProperty("indexStoragePath")]
        [ConfigurationProperty("indexStoragePath", IsRequired = false)]
        public string IndexStoragePath
        {
            get { return this["indexStoragePath"] as string; }
            set { this["indexStoragePath"] = value; }
        }
    }
}
