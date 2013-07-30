namespace Barista.Web
{
  using System.IO;
  using System.Linq;
  using Barista.Configuration;
  using Barista.Framework;
  using Barista.Search;
  using System;
  using System.Configuration;
  using System.ServiceModel;

  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
  public class BaristaWebSearchService : BaristaSearchService
  {
    protected override Lucene.Net.Store.Directory GetLuceneDirectoryFromIndexName(string indexName)
    {
      var baristaConfig = (BaristaConfigurationSection)ConfigurationManager.GetSection("barista");

      if (baristaConfig == null || baristaConfig.IndexDefinitions == null || baristaConfig.IndexDefinitions.Count == 0)
        throw new InvalidOperationException("No index definitions have been defined.");

      var indexDefinitionToUse =
         baristaConfig.IndexDefinitions.FirstOrDefault(id => id.IndexName == indexName);

      if (indexDefinitionToUse == null)
        throw new InvalidOperationException(String.Format("Unable to locate an index with the specified name: {0}. Please add an indexDefinition to the web.config with the specified name and type.", indexName));

      //Lets create the Directory object from the index definition!!
      var directoryType = Type.GetType(indexDefinitionToUse.TypeName, false, true);
      if (directoryType == null)
        throw new InvalidOperationException(
          String.Format("An index definition named {0} was located, however, the type {1} could not be found.",
                        indexName, indexDefinitionToUse.TypeName));

      if (typeof(Lucene.Net.Store.Directory).IsAssignableFrom(directoryType) == false)
        throw new InvalidOperationException(
          String.Format("An index definition named {0} was located, however, the type {1} is not a directory type.",
                        indexName, indexDefinitionToUse.TypeName));

      if (directoryType == typeof(Lucene.Net.Store.SimpleFSDirectory))
      {
        var di = new DirectoryInfo(indexDefinitionToUse.IndexStoragePath);
        if (di.Exists == false)
          di.Create();

        return new Lucene.Net.Store.SimpleFSDirectory(di);
      }

      if (directoryType == typeof(Lucene.Net.Store.RAMDirectory))
      {
        return new Lucene.Net.Store.RAMDirectory();
      }

      //A little bit of extensibility...
      var directory = (Lucene.Net.Store.Directory)Activator.CreateInstance(directoryType, indexDefinitionToUse.IndexStoragePath);
      return directory;
    }
  }
}
