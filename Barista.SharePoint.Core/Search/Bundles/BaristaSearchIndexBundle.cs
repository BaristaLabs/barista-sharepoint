namespace Barista.SharePoint.Search.Bundles
{
    using System;
    using Barista.Search.Library;

    [Serializable]
    public class BaristaSearchIndexBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "Barista Search Index"; }
        }

        public string BundleDescription
        {
            get { return "Barista Search Index Bundle. Provides Information Retrieval functionality via Lucene within SharePoint."; }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalValue("SearchArguments", new SearchArgumentsConstructor(engine));
            engine.SetGlobalValue("JsonDocument", new JsonDocumentConstructor(engine));

            var searchServiceInstance = new SearchServiceInstance(engine.Object.InstancePrototype,
                new SPBaristaSearchServiceProxy())
            {
                GetIndexDefinitionFromName = indexName => BaristaHelper.GetBaristaIndexDefinitionFromIndexName(indexName)
            };

            return searchServiceInstance;
        }
    }
}
