namespace Barista.Configuration
{
  using System.Configuration;

  public class BaristaConfigurationSection : ConfigurationSection
  {
    /// <summary>
    /// Gets all the index definition configurations
    /// </summary>
    [ConfigurationProperty("indexDefinitions")]
    public IndexDefinitionCollection IndexDefinitions
    {
      get
      {
        return this["indexDefinitions"] as IndexDefinitionCollection;
      }
    }
  }
}
