namespace Barista.Configuration
{
  using System.Configuration;
 
  [ConfigurationCollection(typeof(Barista.Configuration.IndexDefinition), AddItemName = "indexDefinition")]
  public class IndexDefinitionCollection : GenericConfigurationElementCollection<Barista.Configuration.IndexDefinition, IIndexDefinitionConfig>
  {
  }
}
