namespace Barista
{
    using System;
    using System.Collections.Generic;
    using Barista.Newtonsoft.Json;
    using Barista.Newtonsoft.Json.Linq;
    using Barista.Newtonsoft.Json.Schema;
    using Barista.Properties;

    public class Package
    {
        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("version")]
        public string Version
        {
            get;
            set;
        }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData
        {
            get;
            set;
        }

        private static readonly Lazy<JsonSchema> PackageSchema = new Lazy<JsonSchema>(() => JsonSchema.Parse(Resources.package));

        /// <summary>
        /// Returns a value that indicates if the specifed object is a valid JSON Object according to the package.org specification.
        /// </summary>
        /// <param name="packageObj"></param>
        /// <returns></returns>
        public static bool IsValidPackage(JObject packageObj)
        {
            //TODO: Use schema validation to determine if the object is valid.
            JToken temp;
            if (!packageObj.TryGetValue("name", out temp))
                return false;

            if (!packageObj.TryGetValue("version", out temp))
                return false;

            return true;
        }
    }
}
